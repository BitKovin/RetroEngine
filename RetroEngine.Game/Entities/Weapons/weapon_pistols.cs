using Assimp;
using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Weapons
{
    public class weapon_pistols : Weapon
    {

        AnimatedStaticMesh mesh1 = new AnimatedStaticMesh();
        AnimatedStaticMesh mesh2 = new AnimatedStaticMesh();

        bool attack = false;
        Delay attackDelay = new Delay();

        SoundPlayer fireSoundPlayer;


        public override void Start()
        {
            base.Start();

            fireSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            fireSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/pistol_fire.wav"));
            fireSoundPlayer.Volume = 0.1f;

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            LoadVisual();

        }

        public override void Update()
        {
            base.Update();

            mesh1.AddTime(Time.deltaTime);
            mesh1.Update();

            mesh2.AddTime(Time.deltaTime);
            mesh2.Update();

            if (Input.GetAction("attack").Holding())
                Shoot();

        }

        public override void Destroy()
        {
            base.Destroy();

            fireSoundPlayer.Destroy(2);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            mesh1.Position = Position;
            mesh1.Rotation = Rotation + DrawRotation;

            mesh2.Position = Position;
            mesh2.Rotation = Rotation + DrawRotation;

            fireSoundPlayer.Position = Camera.position;
        }

        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.12f);

            fireSoundPlayer.Play(true);

            if (!attack)
            {
                mesh1.Play();
            }
            else
            {
                mesh2.Play();
            }
            

            Bullet bullet = new Bullet();
            
            bullet.Rotation = Camera.rotation;
            Level.GetCurrent().AddEntity(bullet);
            bullet.Start();

            if (!attack)
            {
                bullet.Position = (Camera.position.ToPhysics() + Camera.rotation.GetForwardVector().ToPhysics() * 1.2f + Camera.rotation.GetRightVector().ToPhysics() / 4f - Camera.rotation.GetUpVector().ToPhysics() / 4f);
                //bullet.body.SetPosition(mesh1.GetOffsetPointWorldSpace("muzzle") + Camera.rotation.GetForwardVector().ToPhysics() * 1.2f + Camera.rotation.GetRightVector().ToPhysics() / 4f - Camera.rotation.GetUpVector().ToPhysics() / 4f);
            }
            else
            {
                bullet.Position = (Camera.position.ToPhysics() + Camera.rotation.GetForwardVector().ToPhysics() * 1.2f - Camera.rotation.GetRightVector().ToPhysics() / 4f - Camera.rotation.GetUpVector().ToPhysics() / 4f);
                //bullet.body.SetPosition(mesh2.GetOffsetPointWorldSpace("muzzle") + Camera.rotation.GetForwardVector().ToPhysics() * 1.2f - Camera.rotation.GetRightVector().ToPhysics() / 4f - Camera.rotation.GetUpVector().ToPhysics() / 4f);
            }

            bullet.ignore.Add(player);
            

            attack = !attack;

            return;

            var hit = Physics.LineTrace(Camera.position.ToPhysics(), Camera.rotation.GetForwardVector().ToPhysics() * 100 + Camera.position.ToPhysics());

            if (hit is not null)
            {
                if (hit.CollisionObject is not null)
                {
                    RigidBody.Upcast(hit.CollisionObject).Activate(true);
                    RigidBody.Upcast(hit.CollisionObject).ApplyCentralImpulse(Camera.rotation.GetForwardVector().ToPhysics() * 10);
                    Console.WriteLine("pew");
                }
            }

        }

        void LoadVisual()
        {
            mesh1.Scale = new Vector3(1, 1, 1);

            mesh1.AddFrame("Animations/Pistol/Fire/frame_0001.obj");


            mesh1.AddFrame("Animations/Pistol/Fire/frame_0002.obj");
            mesh1.AddFrame("Animations/Pistol/Fire/frame_0003.obj");
            mesh1.AddFrame("Animations/Pistol/Fire/frame_0004.obj");
            mesh1.AddFrame("Animations/Pistol/Fire/frame_0005.obj");
            mesh1.AddFrame("Animations/Pistol/Fire/frame_0006.obj");
            mesh1.AddFrame("Animations/Pistol/Fire/frame_0007.obj");
            mesh1.AddFrame("Animations/Pistol/Fire/frame_0008.obj");
            mesh1.AddFrame("Animations/Pistol/Fire/frame_0009.obj");
            mesh1.AddFrame("Animations/Pistol/Fire/frame_0010.obj");
            mesh1.frameTime = 1f / 30f;
            //mesh.texture = AssetRegistry.LoadTextureFromFile("usp.png");
            mesh1.textureSearchPaths.Add("textures/weapons/arms/");
            mesh1.textureSearchPaths.Add("textures/weapons/pistol/");
            mesh1.CastShadows = false;
            mesh1.PreloadTextures();
            mesh1.Viewmodel = true;

            meshes.Add(mesh1);

            mesh1.UseAlternativeRotationCalculation = true;

            mesh2.Scale = new Vector3(-1, 1, 1);

            mesh2.AddFrame("Animations/Pistol/Fire/frame_0001.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0002.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0003.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0004.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0005.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0006.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0007.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0008.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0009.obj");
            mesh2.AddFrame("Animations/Pistol/Fire/frame_0010.obj");
            mesh2.frameTime = 1f / 30f;

            mesh2.UseAlternativeRotationCalculation = true;

            mesh2.textureSearchPaths.Add("textures/weapons/arms/");
            mesh2.textureSearchPaths.Add("textures/weapons/pistol/");
            mesh2.CastShadows = false;
            mesh2.PreloadTextures();

            mesh2.Viewmodel = true;
            meshes.Add(mesh2);

            Bullet bullet = new Bullet();
            Level.GetCurrent().AddEntity(bullet);
            bullet.Start();
            bullet.Position = new Vector3(0, 10000000, 0);
            bullet.LifeTime = 0.1f;

        }

    }
}
