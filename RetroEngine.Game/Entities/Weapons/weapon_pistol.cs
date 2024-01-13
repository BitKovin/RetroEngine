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
    internal class weapon_pistol : Weapon
    {
        SkeletalMesh mesh = new SkeletalMesh();

        SkeletalMesh arms = new SkeletalMesh();

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

            mesh.Update(Time.deltaTime*1.2f);

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

            arms.Position = mesh.Position;
            arms.Rotation = mesh.Rotation;

            arms.PastePose(mesh.CopyPose());

            fireSoundPlayer.Position = Camera.position;
        }

        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.35f);

            fireSoundPlayer.Play(true);

            mesh.PlayAnimation(0,false);


            Bullet bullet = new Bullet();
            bullet.ignore.Add(player);

            Vector3 bulletRotation;

            float x = 0;
            float y = 0;

            Vector3 startPos = Camera.position;
            Vector3 endPos = Camera.position + Camera.rotation.GetForwardVector() * 100 + Camera.rotation.GetRightVector() * x + Camera.rotation.GetUpVector() * y;

            bulletRotation = MathHelper.FindLookAtRotation(startPos, endPos);

            bullet.Rotation = bulletRotation;

            bullet.LifeTime = 2f;

            Level.GetCurrent().AddEntity(bullet);


            bullet.Position = (Camera.position.ToPhysics() + Camera.rotation.GetForwardVector().ToPhysics() * 0.5f + Camera.rotation.GetRightVector().ToPhysics() / 8f - Camera.rotation.GetUpVector().ToPhysics() / 5f);

            bullet.Start();
            bullet.Speed = 100;
            bullet.Damage = 50;

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

            mesh.LoadFromFile("models/weapons/pistol.fbx");

            arms.LoadFromFile("models/weapons/arms.fbx");
            arms.textureSearchPaths.Add("textures/weapons/arms/");

            mesh.textureSearchPaths.Add("textures/weapons/arms/");
            mesh.textureSearchPaths.Add("textures/weapons/pistol/");
            mesh.textureSearchPaths.Add("textures/weapons/general/");


            mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;
            mesh.UseAlternativeRotationCalculation = true;
            mesh.Transperent = true;

            arms.CastShadows = false;
            arms.PreloadTextures();
            arms.Viewmodel = true;
            arms.UseAlternativeRotationCalculation = true;


            mesh.SetInterpolationEnabled(true);
            

            Console.WriteLine("loaded pistol");

            meshes.Add(mesh);
            meshes.Add(arms);
            //new Bullet().LoadAssetsIfNeeded();

        }
    }
}
