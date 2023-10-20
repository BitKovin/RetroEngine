
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RetroEngine;
using RetroEngine.Physics;

namespace Engine.Entities
{
    public class Player:Entity
    {

        Button buttonUp = new Button();
        Button buttonUpRight = new Button();
        Button buttonUpLeft = new Button();
        Button buttonDown = new Button();
        Button buttonLeft = new Button();
        Button buttonRight = new Button();
        Button buttonRotate = new Button();

        StaticMesh mesh = new StaticMesh();

        StaticMesh cylinder = new StaticMesh();

        RigidBody body;

        float speed = 10;

        float bobProgress = 0;

        Vector3 OldCameraPos;

        Vector3 bob;

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

            collision.size = new Vector3(1,1,1);



            //Model model = GameMain.content.Load<Model>("pistol");
            meshes.Add(mesh);

            mesh.LoadFromFile("pistol.obj");
            mesh.texture = Utils.LoadTextureFromFile("usp.png");

            cylinder.LoadFromFile("cylinder.obj");
            //meshes.Add(cylinder);

            buttonRotate.onClicked += ButtonRotate_onClicked;

        }

        private void ButtonRotate_onClicked()
        {
            Shoot();
        }

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 2);
            body.Gravity = new BulletSharp.Math.Vector3(0, -50, 0);

            body.SetPosition(new Vector3(3, 20, 3).ToPhysics());

            body.CcdMotionThreshold = 0.000001f;
            body.CcdSweptSphereRadius = 0.3f;

        }

        public override void Update()
        {
            base.Update();


            Vector2 input = new Vector2();

            if (buttonUp.pressing || Keyboard.GetState().IsKeyDown(Keys.W)|| buttonUpRight.pressing || buttonUpLeft.pressing)
                input += new Vector2(0, 100)*Time.deltaTime;

            if (buttonDown.pressing || Keyboard.GetState().IsKeyDown(Keys.S))
                input -= new Vector2(0, 100) * Time.deltaTime;

            if (buttonRight.pressing || Keyboard.GetState().IsKeyDown(Keys.D) || buttonUpRight.pressing)
                input -= new Vector2(100, 0) * Time.deltaTime;

            if (buttonLeft.pressing || Keyboard.GetState().IsKeyDown(Keys.A) || buttonUpLeft.pressing)
                input += new Vector2(100, 0) * Time.deltaTime;

            Camera.rotation += new Vector3(Input.MouseDelta.Y, -Input.MouseDelta.X, 0)/2f;
            Camera.rotation = new Vector3(Math.Clamp(Camera.rotation.X, -89, 89), Camera.rotation.Y, 0);

            //Camera.rotation = new Vector3();

            Vector3 motion = new Vector3();

            

            if (input.Length()>0)
            {
                input.Normalize();

                bobProgress += Time.deltaTime;

                motion += Camera.rotation.GetRightVector().XZ() * input.X * speed;
                motion += Camera.rotation.GetForwardVector().XZ() * input.Y * speed;


                
            }


            body.Activate(true);
            body.LinearVelocity = new Vector3(motion.X, (float)body.LinearVelocity.Y, motion.Z).ToPhysics();

            Vector3 newCameraPos = Position + new Vector3(0, 0.8f, 0);

            Camera.position = Vector3.Lerp(OldCameraPos, newCameraPos, 1);

            OldCameraPos = Camera.position;

            bob = Vector3.Zero;

            bob += Camera.rotation.GetRightVector() * ((float)Math.Sin(bobProgress*1 * 7)) * 0.2f;
            bob += new Vector3(0, (float)(Math.Abs(Math.Sin(bobProgress * 7 * 1))) * 0.2f, 0) ;

            //Camera.position += bob;


            if (Input.pressedKeys.Contains(Keys.Space))
                Jump();

            if (Input.pressedKeys.Contains(Keys.E))
                Shoot();




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
            body.ApplyCentralImpulse(new BulletSharp.Math.Vector3(0, 40, 0));
        }

        void Shoot()
        {

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


        bool IsCollide()
        {
            foreach(Entity entity in GameMain.inst.curentLevel.entities)
            {
                if(entity!=this)
                if(Collision.MakeCollionTest(collision, entity.collision))
                    return true;
            }
            return false;
        }



    }
}
