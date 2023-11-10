
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

namespace RetroEngine.Entities
{

    [LevelObject("info_player_start")]
    public class Player:Entity
    {

        Button buttonUp = new Button();
        Button buttonUpRight = new Button();
        Button buttonUpLeft = new Button();
        Button buttonDown = new Button();
        Button buttonLeft = new Button();
        Button buttonRight = new Button();
        Button buttonRotate = new Button();

        AnimatedStaticMesh mesh = new AnimatedStaticMesh();

        StaticMesh cylinder = new StaticMesh();


        float speed = 10;

        float bobProgress = 0;

        Vector3 OldCameraPos;

        Vector3 bob;

        float cameraRoll;

        public Player():base()
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


            Image crosshair = new Image();

            crosshair.originH = Origin.CenterH;
            crosshair.originV = Origin.CenterV;
            crosshair.position = new Vector2(-2);
            crosshair.size = new Vector2(4, 4);
            UiElement.main.childs.Add(crosshair);

            //Model model = GameMain.content.Load<Model>("pistol");
            meshes.Add(mesh);

            mesh.Scale = new Vector3(1,1,1);

            mesh.AddFrame("Animations/Pistol/Fire/frame_0001.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0002.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0003.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0004.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0005.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0006.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0007.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0008.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0009.obj");
            mesh.AddFrame("Animations/Pistol/Fire/frame_0010.obj");
            mesh.frameTime = 1f / 30f;
            //mesh.texture = AssetRegistry.LoadTextureFromFile("usp.png");
            mesh.textureSearchPaths.Add("textures/weapons/arms/");
            mesh.textureSearchPaths.Add("textures/weapons/pistol/");

            mesh.Viewmodel = true;

            //cylinder.LoadFromFile("cylinder.obj");
            //meshes.Add(cylinder);

            buttonRotate.onClicked += ButtonRotate_onClicked;

            Tags.Add("player");

        }

        private void ButtonRotate_onClicked()
        {
            Shoot();
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Camera.position = Position = OldCameraPos = data.GetPropertyVector("origin");

        }

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 2);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();

            body.SetPosition(Position.ToPhysics());

            body.CcdMotionThreshold = 0.000001f;
            body.CcdSweptSphereRadius = 0.3f;

        }

        public override void Update()
        {
            base.Update();


            Vector2 input = new Vector2();

            if (buttonUp.pressing || Keyboard.GetState().IsKeyDown(Keys.W)|| buttonUpRight.pressing || buttonUpLeft.pressing)
                input += new Vector2(0, 1);

            if (buttonDown.pressing || Keyboard.GetState().IsKeyDown(Keys.S))
                input -= new Vector2(0, 1) ;

            if (buttonRight.pressing || Keyboard.GetState().IsKeyDown(Keys.D) || buttonUpRight.pressing)
                input += new Vector2(1, 0);

            if (buttonLeft.pressing || Keyboard.GetState().IsKeyDown(Keys.A) || buttonUpLeft.pressing)
                input -= new Vector2(1, 0);

            Camera.rotation += new Vector3(Input.MouseDelta.Y, -Input.MouseDelta.X, 10)/2f;
            Camera.rotation = new Vector3(Math.Clamp(Camera.rotation.X, -89, 89), Camera.rotation.Y, 0);

            cameraRoll = MathHelper.Lerp(cameraRoll, input.X * 2, Time.deltaTime*10);

            Camera.roll = cameraRoll;

            Vector3 motion = new Vector3();

            if (input.Length()>0)
            {
                input.Normalize();

                bobProgress += Time.deltaTime;

                motion += Camera.rotation.GetRightVector().XZ() * input.X * speed;
                motion += Camera.rotation.GetForwardVector().XZ()/ Camera.rotation.GetForwardVector().XZ().Length() * input.Y * speed;


                
            }

            body.Activate(true);
            body.LinearVelocity = new Vector3(motion.X, (float)body.LinearVelocity.Y, motion.Z).ToPhysics();

            Vector3 newCameraPos = Position + new Vector3(0, 0.7f, 0);

            Camera.position = Vector3.Lerp(OldCameraPos, newCameraPos, Time.deltaTime * 30);

            OldCameraPos = Camera.position;

            bob = Vector3.Zero;

            bob += Camera.rotation.GetRightVector() * ((float)Math.Sin(bobProgress*1 * 7)) * 0.2f;
            bob += Camera.rotation.GetUpVector() * (float)(Math.Abs(Math.Sin(bobProgress * 7 * 1))) * 0.2f;

            //Camera.position += new Vector3(0,1,0) * (float)(Math.Abs(Math.Sin(bobProgress * 7 * 1))) * 0.2f;


            if (Input.pressedKeys.Contains(Keys.Space))
                Jump();

            if (Input.pressedKeys.Contains(Keys.E))
                Shoot();

            mesh.AddTime(Time.deltaTime);
            mesh.Update();

        }

        public override void LateUpdate()
        {
            //Camera.Follow(this);

            mesh.Position = Camera.position + bob*0.05f;
            mesh.Rotation = Camera.rotation;

            cylinder.Position = Position + Camera.rotation.GetForwardVector().XZ()*3;
        }

        void Jump()
        {
            body.ApplyCentralImpulse(new Vector3(0, 25, 0).ToNumerics());
        }

        void Shoot()
        {

            mesh.Play();


            BoxDynamic box = new BoxDynamic();

            box.scale = new Vector3(0.2f);

            box.Rotation = Camera.rotation;
            GameMain.inst.curentLevel.entities.Add(box);
            box.Start();
            box.body.Activate();
            box.body.SetIgnoreCollisionCheck(body, true);
            box.body.SetPosition(Camera.position.ToPhysics() +Camera.rotation.GetForwardVector().ToPhysics()/2f + Camera.rotation.GetRightVector().ToPhysics()/4f - Camera.rotation.GetUpVector().ToPhysics() / 4f);
            box.body.CcdSweptSphereRadius = 0.1f;
            box.body.CcdMotionThreshold = 0.1f;
            box.body.ApplyCentralImpulse(Camera.rotation.GetForwardVector().ToPhysics() * 30);
            box.body.Gravity = new Vector3(0.1f).ToPhysics();
            box.Update();
            return;
            var hit = Physics.LineTrace(Camera.position.ToPhysics(), Camera.rotation.GetForwardVector().ToPhysics() * 100 + Camera.position.ToPhysics());

            if(hit is not null)
            {
                if (hit.CollisionObject is not null)
                {
                    RigidBody.Upcast(hit.CollisionObject).Activate(true);
                    RigidBody.Upcast(hit.CollisionObject).ApplyCentralImpulse(Camera.rotation.GetForwardVector().ToPhysics() * 10);
                    Console.WriteLine("pew");
                }
            }


        }

    }
}
