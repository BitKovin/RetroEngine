using Assimp;
using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.Game.Effects.Particles;
using RetroEngine.PhysicsSystem;
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
            hitSoundPlayer.Volume = 0.5f;
            hitSoundPlayer.MaxDistance = 5;
            hitSoundPlayer.MinDistance = 0f;

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            LoadVisual();

        }

        public override void Update()
        {
            base.Update();

            mesh.AddTime(Time.DeltaTime);
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

            mesh.Position = Position + GetWorldSway()*1.3f;
            mesh.Rotation = Rotation + DrawRotation;

            attackSoundPlayer.Position = Camera.position;
        }

        void Attack()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.4f);

            pendingAttackDelay.AddDelay(0.16f);
            pendingMeleeAttack = true;

            attackSoundPlayer.Play(true);

            mesh.Play();

        }

        void PerformHitCheck()
        {
            var hit = Physics.LineTrace(Camera.position.ToPhysics(), Camera.rotation.GetForwardVector().ToPhysics() * 2f + Camera.position.ToPhysics(), new List<CollisionObject> { player.bodies[0] });

            if (hit.HasHit)
            {
                if (hit.CollisionObject is not null)
                {
                    //RigidBody.Upcast(hit.CollisionObject).Activate(true);
                    //RigidBody.Upcast(hit.CollisionObject).ApplyCentralImpulse(Camera.rotation.GetForwardVector().ToPhysics() * 10);

                    Entity hitEnt = hit.CollisionObject.UserObject as Entity;

                    Console.WriteLine((hit.CollisionObject.CollisionShape.UserObject as Physics.CollisionShapeData).surfaceType);

                    if (hitEnt != null)
                    {
                        hitEnt.OnPointDamage(30, hit.HitPointWorld, Camera.rotation.GetForwardVector(), player, this);
                    }

                    CreateHitParticle(hit.HitPointWorld + hit.HitNormalWorld * 0.1f);

                    hitSoundPlayer.Position = hit.HitPointWorld;
                    hitSoundPlayer.Update();

                    hitSoundPlayer.Play(true);
                }
            }
        }

        void CreateHitParticle(Vector3 pos)
        {

            ParticleSystem system = ParticleSystem.Create("hitSmoke");
            system.Position = pos;
            system.Start();

        }

        void LoadVisual()
        {
            return;
            mesh.Scale = new Vector3(0.4f);

            for (int i = 1; i <= 50; i+=3)
            {
                mesh.AddFrame($"Animations/Hammer/Attack/frame_{i}.obj");
            }

            mesh.frameTime = 1f / 30f;
            //mesh.texture = AssetRegistry.LoadTextureFromFile("usp.png");
            mesh.textureSearchPaths.Add("textures/weapons/arms/");
            mesh.textureSearchPaths.Add("textures/weapons/hammer/");
            mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;
            mesh.UseAlternativeRotationCalculation = true;
            mesh.isLoaded = true;
            mesh.AddFrameVertexData();

            ParticleSystem.Preload("hitSmoke");

            meshes.Add(mesh);
        }
    }
}
