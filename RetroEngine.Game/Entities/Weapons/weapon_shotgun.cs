using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Weapons
{
    public class weapon_shotgun : Weapon
    {

        AnimatedStaticMesh mesh = new AnimatedStaticMesh();

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

            mesh.AddTime(Time.deltaTime);
            mesh.Update();

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

            mesh.Position = Position;
            mesh.Rotation = Rotation + DrawRotation;

            fireSoundPlayer.Position = Camera.position;
        }

        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.6f);

            fireSoundPlayer.Play(true);

            mesh.Play();

            for (float y = -2; y <= 2; y += 2f)
                for (float x = -5; x <= 5; x += 2.5f)
                {
                    Bullet bullet = new Bullet();

                    Vector3 bulletRotation;

                    Vector3 startPos = Camera.position;
                    Vector3 endPos = Camera.position - new Vector3(0,-1,0) + Camera.rotation.GetForwardVector()*60 + Camera.rotation.GetRightVector()*x + Camera.rotation.GetUpVector()*y;

                    bulletRotation = MathHelper.FindLookAtRotation(startPos, endPos);

                    bullet.Rotation = bulletRotation;

                    bullet.LifeTime = 0.2f;

                    Level.GetCurrent().AddEntity(bullet);
                    

                    bullet.Position = (Camera.position.ToPhysics() + Camera.rotation.GetForwardVector().ToPhysics() * 0.5f + Camera.rotation.GetRightVector().ToPhysics() / 10f - Camera.rotation.GetUpVector().ToPhysics() / 4f);

                    bullet.Start();
                    bullet.Speed = 100;
                    bullet.Damage = 8;

                    bullet.ignore.Add(player);
                    
                }

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
            mesh.Scale = new Vector3(0.7f);


            for (int i = 1; i <= 30; i++)
            {
                mesh.AddFrame($"Animations/Shotgun/Fire/frame_{i}.obj");
            }

            mesh.frameTime = 1f / 70f;

            mesh.textureSearchPaths.Add("textures/weapons/arms/");
            mesh.textureSearchPaths.Add("textures/weapons/shotgun/");
            mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;
            mesh.UseAlternativeRotationCalculation = true;

            meshes.Add(mesh);

            new Bullet().LoadAssetsIfNeeded();

        }

    }
}
