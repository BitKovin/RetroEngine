using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using RetroEngine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RetroEngine.Game.Entities;
using RetroEngine.Map;
using RetroEngine.Game.Entities.Weapons;
using RetroEngine.Entities;
using BulletSharp.SoftBody;

namespace RetroEngine.Game.Entities.Player
{

    [LevelObject("info_player_start")]
    public class PlayerCharacter : Entity
    {

        Button buttonUp = new Button();
        Button buttonUpRight = new Button();
        Button buttonUpLeft = new Button();
        Button buttonDown = new Button();
        Button buttonLeft = new Button();
        Button buttonRight = new Button();
        Button buttonRotate = new Button();


        StaticMesh cylinder = new StaticMesh();

        public RigidBody body;

        float maxSpeed = 8;
        float maxSpeedAir = 2;
        float acceleration = 90;
        float airAcceleration = 40;
        bool tryLimit = false;

        Vector3 velocity = new Vector3();

        float bobProgress = 0;

        Vector3 OldCameraPos;

        Vector3 bob;

        float cameraRoll;

        bool FirstTick = true;

        SoundPlayer stepSoundPlayer;

        bool onGround = false;

        List<WeaponData> weapons = new List<WeaponData>();

        public Weapon currentWeapon;
        int currentSlot = -1;
        int lastSlot = -1;

        float bobSpeed = 8;

        PlayerUI PlayerUI;

        Vector3 interpolatedPosition = new Vector3();

        SkeletalMesh bodyMesh = new SkeletalMesh();
        PlayerBodyAnimator PlayerBodyAnimator = new PlayerBodyAnimator();

        public PlayerCharacter() : base()
        {
            if (GameMain.platform == Platform.Mobile)
            {

                buttonLeft = new Button();
                buttonLeft.position = new Vector2(59, 601);
                buttonLeft.size = new Vector2(100, 100);
                UiElement.Viewport.childs.Add(buttonLeft);

                buttonRight = new Button();
                buttonRight.position = new Vector2(112 + 50, 601);
                buttonRight.size = new Vector2(100, 100);
                UiElement.Viewport.childs.Add(buttonRight);

                buttonRotate = new Button();
                buttonRotate.position = new Vector2(-200, 601);
                buttonRotate.size = new Vector2(100, 100);
                buttonRotate.originH = Origin.Right;
                UiElement.Viewport.childs.Add(buttonRotate);
            }

            buttonRotate.onClicked += ButtonRotate_onClicked;

            Tags.Add("player");

            Input.CenterCursor();

            PlayerUI = new PlayerUI(this);

        }

        private void ButtonRotate_onClicked()
        {

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            bodyMesh.LoadFromFile("models/player_body.FBX");

            bodyMesh.textureSearchPaths.Add("textures/weapons/arms/");

            bodyMesh.Scale = new Vector3(1.15f);

            PlayerBodyAnimator.LoadAssets();

            meshes.Add(bodyMesh);

            //Weapon.PreloadAllWeapons();
            PlayerUI.Load();
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
            body.Friction = 1f;

            bodies.Add(body);

            stepSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            stepSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/step.wav"));
            stepSoundPlayer.Volume = 0.5f;

            weapons.Add(new WeaponData { weaponType = typeof(weapon_pistol), ammo = 1 });
            weapons.Add(new WeaponData { weaponType = typeof(weapon_shotgunNew), ammo = 50 });
            weapons.Add(new WeaponData { weaponType = typeof(weapon_hammer), ammo = 50 });

            interpolatedPosition = Position;

        }


        public override void Update()
        {
            base.Update();

            CheckGround();

            InterpolatePos();

            FirstTick = false;

            

        }

        void UpdatePlayerInput()
        {
            UpdateMovement();

            UpdateCamera();

            if (Input.GetAction("test").Holding())
                PlayerBodyAnimator.FireAction();

            if (Input.GetAction("jump").Holding())
                Jump();

            if (Input.GetAction("slot0").Pressed())
                SwitchToSlot(-1);

            if (Input.GetAction("slot1").Pressed())
                SwitchToSlot(0);

            if (Input.GetAction("slot2").Pressed())
                SwitchToSlot(1);

            if (Input.GetAction("slot3").Pressed())
                SwitchToSlot(2);

            if (Input.GetAction("lastSlot").Pressed())
                SwitchToSlot(lastSlot);
        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            PlayerBodyAnimator.Update();

            PlayerBodyAnimator.MovementSpeed = ((Vector3)body.LinearVelocity).XZ().Length();

            float Dx = Vector3.Dot(((Vector3)body.LinearVelocity).XZ().Normalized(), Camera.rotation.GetRightVector());
            float Dy = Vector3.Dot(((Vector3)body.LinearVelocity).XZ().Normalized(), Camera.rotation.GetForwardVector().XZ().Normalized());

            Vector2 dir = new Vector2(Dx, Dy);


            PlayerBodyAnimator.MovementDirection = dir;

            

            var pose = PlayerBodyAnimator.GetResultPose();

            bodyMesh.PastePoseLocal(pose);

            MathHelper.Transform hide = new MathHelper.Transform();
            hide.Scale = Vector3.Zero;
            MathHelper.Transform show = new MathHelper.Transform();

            Matrix showR = new Matrix();
            Matrix showL = new Matrix();

            if (currentWeapon == null)
            {
                showR = showL = show.ToMatrix();
            }else
            {
                if(currentWeapon.ShowHandR)
                {
                    showR = show.ToMatrix();
                }
                else
                {
                    showR = hide.ToMatrix();
                }

                if (currentWeapon.ShowHandL)
                {
                    showL = show.ToMatrix();
                }
                else
                {
                    showL = hide.ToMatrix();
                }

            }

            bodyMesh.SetBoneMeshTransformModification("upperarm_r",showR);
            bodyMesh.SetBoneMeshTransformModification("upperarm_l", showL);
            bodyMesh.SetBoneMeshTransformModification("head", hide.ToMatrix());

            

        }

        void InterpolatePos()
        {
            Vector3 oldPos = interpolatedPosition;

            Vector3 newPos = Position;

            interpolatedPosition = Vector3.Lerp(oldPos, newPos, Time.deltaTime*25);

        }

        void UpdateCamera()
        {
            if (!FirstTick)
                Camera.rotation += new Vector3(Input.MouseDelta.Y, -Input.MouseDelta.X, 10) / 2f;

            Camera.rotation = new Vector3(Math.Clamp(Camera.rotation.X, -89, 89), Camera.rotation.Y, 0);


            Camera.position = interpolatedPosition + new Vector3(0,0.7f,0) + Camera.rotation.GetForwardVector().XZ().Normalized()*0.1f;

            bob = Vector3.Zero;

            bob += Camera.rotation.GetForwardVector() * (float)Math.Sin(bobProgress * 1 * bobSpeed * 1) * -0.5f;
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

            body.Activate(true);

            // Ground movement

            velocity = body.LinearVelocity;
            body.Friction = 0.0f;
            if (input.Length() > 0.1f)
            {
                input.Normalize();

                motion += right * input.X;
                motion += forward * input.Y;

                if (onGround)
                {
                    
                    if (Math.Sin(bobProgress * bobSpeed * 2) <= 0 && Math.Sin((bobProgress + Time.deltaTime) * bobSpeed * 2) > 0)
                    {
                        stepSoundPlayer.Play(true);
                    }

                    bobProgress += Time.deltaTime;


                    velocity = UpdateGroundVelocity(motion, velocity);

                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();

                }
                else
                {
                    /*
                    float airControlPower = Vector3.Dot(motion, ((Vector3)body.LinearVelocity).XZ());

                    airControlPower /= 100;

                    airControlPower += 1;
                    airControlPower /= 2;

                    airControlPower = 1 - airControlPower;

                    body.ApplyCentralForce(motion.ToNumerics() * 12 * airControlPower);
                    */

                    velocity = UpdateAirVelocity(motion, velocity);

                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();

                }
            }
            else
            {

                if (onGround)
                {
                    velocity = Friction(velocity);
                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();
                }
                else
                {
                    velocity = UpdateAirVelocity(new Vector3(), velocity);
                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();
                }
                // No input, apply friction
                body.Friction = 0.2f;
            }


            cameraRoll = MathHelper.Lerp(cameraRoll, input.X * 1.5f, Time.deltaTime * 10);

            Camera.roll = cameraRoll;

            stepSoundPlayer.Position = Position;

        }

        void CheckGround()
        {
            onGround = false;

            float radius = 0.4f;

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



            if (CheckGroundAtOffset(new Vector3(radius * 0.77f, 0, radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(-radius * 0.77f, 0, radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(radius * 0.77f, 0, -radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(-radius * 0.77f, 0, -radius * 0.77f)))
                onGround = true;

            if(jumpDelay.Wait())
                onGround = false;

        }

        bool CheckGroundAtOffset(Vector3 offset)
        {
            var hit = Physics.LineTrace(Position.ToNumerics() + offset.ToNumerics(), (Position - new Vector3(0, 1.05f, 0) + offset).ToNumerics(), new List<CollisionObject>() { body });

            return hit.HasHit;
        }

        public override void LateUpdate()
        {

            
            UpdatePlayerInput();

            bodyMesh.Position = interpolatedPosition - Camera.rotation.GetForwardVector().XZ().Normalized() * 0.25f - new Vector3(0, 1.05f, 0);
            bodyMesh.Rotation = new Vector3(0, Camera.rotation.Y, 0);

            MathHelper.Transform t = bodyMesh.GetBoneMatrix("head").DecomposeMatrix();

            Camera.position = t.Position + Camera.rotation.GetForwardVector().XZ().Normalized() * 0.35f;

            if (currentWeapon is not null)
            {
                currentWeapon.Position = Camera.position + bob * 0.05f*currentWeapon.BobScale + Camera.rotation.GetForwardVector() * Camera.rotation.X / 2000f;
                currentWeapon.Rotation = Camera.rotation + new Vector3(0, 0, (float)Math.Sin(bobProgress * -1 * bobSpeed) * -1.5f) * currentWeapon.BobScale;
            }

            PlayerUI.Update();

            cylinder.Position = Position + Camera.rotation.GetForwardVector().XZ() * 3;
        }

        Delay jumpDelay = new Delay();

        void Jump()
        {
            
            if (onGround || true)
            {
                body.LinearVelocity = new Vector3(body.LinearVelocity.X, 12, body.LinearVelocity.Z).ToNumerics();
                jumpDelay.AddDelay(0.1f);
            }
        }

        Vector3 UpdateGroundVelocity(Vector3 withDir, Vector3 vel)
        {
            vel = vel.XZ();
            vel = Friction(vel);

            float currentSpeed = Vector3.Dot(vel, withDir);
            float addSpeed = Math.Clamp(maxSpeed - currentSpeed, 0, acceleration*Time.deltaTime);
    
            if(tryLimit)
                if(currentSpeed + addSpeed > maxSpeed)
                    addSpeed = maxSpeed - currentSpeed;

            return vel + addSpeed * withDir;
        }
        Vector3 UpdateAirVelocity(Vector3 wishdir, Vector3 vel)
        {
            vel = vel.XZ();

            float currentSpeed = Vector3.Dot(vel, wishdir);

            float wishspeed = maxSpeedAir;

            float addSpeed = wishspeed - currentSpeed;

            if (addSpeed <= 0f)
            {
                return vel;
            }

            float accelspeed = airAcceleration * Time.deltaTime * wishspeed;

            if (accelspeed > addSpeed)
            {
                accelspeed = addSpeed;
            }

            return vel + accelspeed * wishdir;
        }


        Vector3 Friction(Vector3 vel, float factor = 50f)
        {

            if (FirstTick)
                return new Vector3();

            vel = vel.XZ();

            float length = vel.Length();

            vel.Normalize();

            length -= factor * Time.deltaTime;

            length = Math.Max(0, length);

            vel = vel * length;

            if (float.IsNaN(vel.X))
                return new Vector3();

            return vel;
        }

        void SwitchToSlot(int slot)
        {
            if (slot == currentSlot) return;

            if (weapons.Count > slot && slot >= 0)
            {

                lastSlot = currentSlot;
                currentSlot = slot;

                SwitchWeapon(weapons[slot]);
            }
            else
            {
                currentSlot = slot;
                SwitchWeapon(null);
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
