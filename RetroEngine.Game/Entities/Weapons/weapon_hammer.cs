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
    internal class weapon_hammer : Weapon
    {
        AnimatedStaticMesh mesh = new AnimatedStaticMesh();

        Delay attackDelay = new Delay();

        SoundPlayer attackSoundPlayer;
        SoundPlayer hitSoundPlayer;


        bool pendingMeleeAttack = false;
        Delay pendingAttackDelay = new Delay();

        public override void Start()
        {
            base.Start();

            attackSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            attackSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/weapons/hammer/swoosh.wav"));
            attackSoundPlayer.Volume = 0.1f;

            hitSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            hitSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/weapons/hammer/hit.wav"));
            hitSoundPlayer.Volume = 0.15f;
            hitSoundPlayer.MaxDistance = 60;
            hitSoundPlayer.MinDistance = 0.5f;

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
                Attack();

            if(pendingMeleeAttack)
                if(pendingAttackDelay.Wait() == false)
                {
                    PerformHitCheck();
                    pendingMeleeAttack=false;
                }

        }

        public override void Destroy()
        {
            base.Destroy();

            attackSoundPlayer.Destroy(2);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            mesh.Position = Position;
            mesh.Rotation = Camera.rotation + DrawRotation;

            attackSoundPlayer.Position = Camera.position;
        }

        void Attack()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.5f);

            pendingAttackDelay.AddDelay(0.16f);
            pendingMeleeAttack = true;

            attackSoundPlayer.Play(true);

            mesh.Play();

        }

        void PerformHitCheck()
        {
            var hit = Physics.LineTrace(Camera.position.ToPhysics(), Camera.rotation.GetForwardVector().ToPhysics() * 2f + Camera.position.ToPhysics());

            if (hit.HasHit)
            {
                if (hit.CollisionObject is not null)
                {
                    //RigidBody.Upcast(hit.CollisionObject).Activate(true);
                    //RigidBody.Upcast(hit.CollisionObject).ApplyCentralImpulse(Camera.rotation.GetForwardVector().ToPhysics() * 10);

                    Entity hitEnt = hit.CollisionObject.UserObject as Entity;

                    if (hitEnt != null)
                    {
                        hitEnt.OnPointDamage(30, hit.HitPointWorld, Camera.rotation.GetForwardVector(), player, this);
                    }

                    hitSoundPlayer.Position = hit.HitPointWorld;
                    hitSoundPlayer.Update();

                    hitSoundPlayer.Play(true);
                }
            }
        }

        void LoadVisual()
        {
            mesh.Scale = new Vector3(0.4f);

            for (int i = 1; i <= 50; i++)
            {
                mesh.AddFrame($"Animations/Hammer/Attack/frame_{i}.obj");
            }

            mesh.frameTime = 1f / 85f;
            //mesh.texture = AssetRegistry.LoadTextureFromFile("usp.png");
            mesh.textureSearchPaths.Add("textures/weapons/arms/");
            mesh.textureSearchPaths.Add("textures/weapons/hammer/");
            mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;

            meshes.Add(mesh);
        }
    }
}
