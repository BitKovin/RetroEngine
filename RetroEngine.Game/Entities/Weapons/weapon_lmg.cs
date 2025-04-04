﻿using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Weapons
{
    internal class weapon_lmg : Weapon
    {
        AnimatedStaticMesh mesh = new AnimatedStaticMesh();

        Delay attackDelay = new Delay();

        SoundPlayer fireSoundPlayer;

        Random random = new Random();

        public override void Start()
        {
            base.Start();

            fireSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            fireSoundPlayer.Volume = 0.1f;
            fireSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/pistol_fire.wav"));

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            //LoadVisual();

            

        }

        public override void Update()
        {
            base.Update();

            if (loadedAssets == false) return;

            mesh.AddTime(Time.DeltaTime);
            mesh.Update();

            if (Input.GetAction("attack").Holding())
                Shoot();
        }

        public override void Destroy()
        {
            base.Destroy();

            fireSoundPlayer.Destroy(2);
            mesh.Dispose();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            mesh.Position = Position;
            mesh.Rotation = Rotation + DrawRotation;

            fireSoundPlayer.Position = Camera.position;
        }

        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.11f);

            fireSoundPlayer.Play(true);

            Console.WriteLine($"playing shot sound {Time.GameTime}");

            mesh.Play();



            Bullet bullet = new Bullet();
            bullet.weapon = this;
            bullet.ignore.Add(player);

            Vector3 bulletRotation;

            float x = ((float)random.NextDouble() - 0.5f) * 4f;
            float y = ((float)random.NextDouble() - 0.5f) * 4f;

            Vector3 startPos = Camera.position;
            Vector3 endPos = Camera.position + Camera.rotation.GetForwardVector() * 100 + Camera.rotation.GetRightVector() * x + Camera.rotation.GetUpVector() * y;

            bulletRotation = MathHelper.FindLookAtRotation(startPos, endPos);

            bullet.Rotation = bulletRotation;

            bullet.LifeTime = 0.6f;

            Level.GetCurrent().AddEntity(bullet);


            bullet.Position = (Camera.position.ToPhysics() + Camera.rotation.GetForwardVector().ToPhysics() * 0.4f + Camera.rotation.GetRightVector().ToPhysics() / 20f - Camera.rotation.GetUpVector().ToPhysics() / 8f);

            bullet.Start();
            bullet.Speed = 100;
            bullet.Damage = 16;

            bullet.ignore.Add(player);



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

            mesh.Scale = new Vector3(1f);


            for (int i = 1; i <= 30; i += 1)
            {
                mesh.AddFrame($"Animations/LMG/Fire/frame_{i}.obj");
            }

            mesh.frameTime = 1f / 35f;

            mesh.textureSearchPaths.Add("textures/weapons/arms/");
            mesh.textureSearchPaths.Add("textures/weapons/LMG/");
            mesh.textureSearchPaths.Add("textures/weapons/general/");
            mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;
            mesh.UseAlternativeRotationCalculation = true;
            mesh.Transperent = true;
            mesh.AddFrameVertexData();

            Console.WriteLine("loaded lmg");


            meshes.Add(mesh);

            mesh.isLoaded = true;

        }
    }
}
