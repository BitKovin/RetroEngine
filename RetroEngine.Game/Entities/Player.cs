using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using RetroEngine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RetroEngine;
using RetroEngine.Game.Entities;
using RetroEngine.Map;
using RetroEngine.Game.Entities.Weapons;

namespace RetroEngine.Entities
{

    [LevelObject("info_player_start")]
    public class Player : Entity
    {

        Button buttonUp = new Button();
        Button buttonUpRight = new Button();
        Button buttonUpLeft = new Button();
        Button buttonDown = new Button();
        Button buttonLeft = new Button();
        Button buttonRight = new Button();
        Button buttonRotate = new Button();


        StaticMesh cylinder = new StaticMesh();

        RigidBody body;

        float speed = 10;

        float bobProgress = 0;

        Vector3 OldCameraPos;

        Vector3 bob;



        float cameraRoll;

        bool FirstTick = true;

        SoundPlayer stepSoundPlayer;

        bool onGround = false;

        List<WeaponData> weapons = new List<WeaponData>();

        Weapon currentWeapon;
        int currentSlot = -1;
        int lastSlot = -1;

        Image crosshair = new Image();

        float bobSpeed = 8;

        public Player() : base()
        {
            if (GameMain.platform == Platform.Mobile)
            {

                buttonLeft = new Button();
                buttonLeft.position = new Vector2(59, 601);
                buttonLeft.size = new Vector2(100, 100);
                UiElement.main.childs.Add(buttonLeft);

                buttonRight = new Button();
                buttonRight.position = new Vector2(112 + 50, 601);
                buttonRight.size = new Vector2(100, 100);
                UiElement.main.childs.Add(buttonRight);

                buttonRotate = new Button();
                buttonRotate.position = new Vector2(-200, 601);
                buttonRotate.size = new Vector2(100, 100);
                buttonRotate.originH = Origin.Right;
                UiElement.main.childs.Add(buttonRotate);
            }
            //Camera.Follow(this);

            crosshair.baseColor = new Color(0.9f, 0.8f, 0.6f) * 0.6f;

            crosshair.originH = Origin.CenterH;
            crosshair.originV = Origin.CenterV;
            crosshair.position = new Vector2(-4);
            crosshair.size = new Vector2(8, 8);
            UiElement.main.childs.Add(crosshair);



            //cylinder.LoadFromFile("cylinder.obj");
            //meshes.Add(cylinder);

            buttonRotate.onClicked += ButtonRotate_onClicked;

            Tags.Add("player");

            Input.CenterCursor();

        }

        private void ButtonRotate_onClicked()
        {

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            crosshair.SetTexture("ui/crosshair.png");

            Weapon.PreloadAllWeapons();

        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Camera.position = Position = OldCameraPos = data.GetPropertyVectorPosition("origin");



            Camera.rotation = new Vector3(0, data.GetPropertyFloat("angle") - 90, 0);

        }

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 2);
            body.Gravity = new Vector3(0, -30, 0).ToNumerics();


            body.SetPosition(Position.ToPhysics());

            Input.MouseDelta = new Vector2();

            body.CcdMotionThreshold = 0.000001f;
            body.CcdSweptSphereRadius = 0.3f;

            bodies.Add(body);

            stepSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            stepSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/step.wav"));
            stepSoundPlayer.Volume = 0.5f;

            weapons.Add(new WeaponData { weaponType = typeof(weapon_hammer), ammo = 1 });
            weapons.Add(new WeaponData { weaponType = typeof(weapon_shotgun), ammo = 50 });
            weapons.Add(new WeaponData { weaponType = typeof(weapon_pistols), ammo = 50 });

        }

        public override void Update()
        {
            base.Update();

            CheckGround();

            UpdateMovement();

            UpdateCamera();

            if (Input.GetAction("jump").Pressed())
                Jump();

            if (Input.GetAction("slot1").Pressed())
                SwitchToSlot(0);

            if (Input.GetAction("slot2").Pressed())
                SwitchToSlot(1);

            if (Input.GetAction("slot3").Pressed())
                SwitchToSlot(2);

            if (Input.GetAction("lastSlot").Pressed())
                SwitchToSlot(lastSlot);

            if (Input.GetAction("test").Released())
            {
                Level.LoadFromFile("test2.map");
            }

            FirstTick = false;


        }

        void UpdateCamera()
        {
            if (!FirstTick)
                Camera.rotation += new Vector3(Input.MouseDelta.Y, -Input.MouseDelta.X, 10) / 2f;

            Camera.rotation = new Vector3(Math.Clamp(Camera.rotation.X, -89, 89), Camera.rotation.Y, 0);


            Vector3 newCameraPos = Position + new Vector3(0, 0.7f, 0);

            Camera.position = Vector3.Lerp(OldCameraPos, newCameraPos, Time.deltaTime * 30);

            OldCameraPos = Camera.position;

            bob = Vector3.Zero;

            bob += Camera.rotation.GetForwardVector() * ((float)Math.Sin(bobProgress * 1 * bobSpeed * 1)) * 0.5f;
            //bob += Camera.rotation.GetUpVector() * (float)(Math.Abs(Math.Sin(bobProgress * bobSpeed * 1))) * 0.2f;
        }

        void UpdateMovement()
        {

            Vector2 input = new Vector2();

            if (buttonUp.pressing || Input.GetAction("moveForward").Holding() || buttonUpRight.pressing || buttonUpLeft.pressing)
                input += new Vector2(0, 1);

            if (buttonDown.pressing || Input.GetAction("moveBackward").Holding())
                input -= new Vector2(0, 1);

            if (buttonRight.pressing || Input.GetAction("moveRight").Holding() || buttonUpRight.pressing)
                input += new Vector2(1, 0);

            if (buttonLeft.pressing || Input.GetAction("moveLeft").Holding() || buttonUpLeft.pressing)
                input -= new Vector2(1, 0);


            Vector3 motion = new Vector3();

            Vector3 right = Camera.rotation.GetRightVector().XZ();
            Vector3 forward = Camera.rotation.GetForwardVector().XZ().Normalized();

            // Ground movement

            GameMain.inst.IsFixedTimeStep = true;
            GameMain.inst.TargetElapsedTime = TimeSpan.FromSeconds(1 / 300f);

            if (input.Length() > 0)
            {
                input.Normalize();

                motion += right * input.X * speed;
                motion += forward * input.Y * speed;

                if (onGround)
                {

                    if (Math.Sin(bobProgress * bobSpeed * 2) <= 0 && Math.Sin((bobProgress + Time.deltaTime) * bobSpeed*2) > 0)
                    {
                        stepSoundPlayer.Play(true);
                    }

                    bobProgress += Time.deltaTime;

                    // Apply friction
                    body.Friction = 0.0f;

                    body.Activate(true);
                    body.LinearVelocity = new Vector3(motion.X, body.LinearVelocity.Y, motion.Z).ToPhysics();

                }
                else
                {

                    float airControlPower = Vector3.Dot(motion, ((Vector3)body.LinearVelocity).XZ());

                    airControlPower /= 100;

                    airControlPower += 1;
                    airControlPower /= 2;

                    airControlPower = 1 - airControlPower;

                    body.ApplyCentralForce(motion.ToNumerics() * 12 * airControlPower);

                    body.Friction = 0.0f;
                }
            }
            else
            {
                // No input, apply friction
                body.Friction = 3f;
            }


            cameraRoll = MathHelper.Lerp(cameraRoll, input.X * 3, Time.deltaTime * 10);

            Camera.roll = cameraRoll;

            stepSoundPlayer.Position = Position;

        }

        void CheckGround()
        {
            onGround = false;

            float radius = 0.495f;

            if (CheckGroundAtOffset(new Vector3(0, 0, 0)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(radius, 0, 0)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(-radius, 0, 0)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(0, 0, radius)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(0, 0, -radius)))
                onGround = true;



            if (CheckGroundAtOffset(new Vector3(radius*0.77f, 0, radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(-radius * 0.77f, 0, radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(radius * 0.77f, 0, -radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(-radius * 0.77f, 0, -radius * 0.77f)))
                onGround = true;

        }

        bool CheckGroundAtOffset(Vector3 offset)
        {
            var hit = Physics.LineTrace(Position.ToNumerics() + offset.ToNumerics(), (Position - new Vector3(0, 1.05f, 0) + offset).ToNumerics(), new List<CollisionObject>() { body });

            return hit.HasHit;
        }

        public override void LateUpdate()
        {
            if (currentWeapon is not null)
            {
                currentWeapon.Position = Camera.position + bob * 0.05f + Camera.rotation.GetForwardVector() * Camera.rotation.X / 3000f;
                currentWeapon.Rotation = Camera.rotation;
            }

            cylinder.Position = Position + Camera.rotation.GetForwardVector().XZ() * 3;
        }

        void Jump()
        {
            if (onGround)

                body.ApplyCentralImpulse(new Vector3(0, 25, 0).ToNumerics());
        }

        void SwitchToSlot(int slot)
        {
            if (slot == currentSlot) return;

            if (weapons.Count > slot && slot>=0)
            {

                lastSlot = currentSlot;
                currentSlot = slot;

                SwitchWeapon(weapons[slot]);
            }
        }

        void SwitchWeapon(WeaponData data)
        {
            if (currentWeapon is not null)
            {
                currentWeapon.Destroy();
                currentWeapon = null;
            }

            if (data is not null)
            {
                currentWeapon = Level.GetCurrent().AddEntity(Weapon.CreateFromData(data, this)) as Weapon;
            }

        }

    }
}
