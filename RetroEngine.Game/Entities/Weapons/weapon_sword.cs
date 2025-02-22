using BulletSharp;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using RetroEngine.Audio;
using RetroEngine.Entities;
using RetroEngine.Game.Effects.Particles;
using RetroEngine.Game.Entities.Player;
using RetroEngine.ParticleSystem;
using RetroEngine.PhysicsSystem;
using RetroEngine.Skeletal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Weapons
{
    internal class weapon_sword : Weapon
    {
        SkeletalMesh mesh = new SkeletalMesh();

        SkeletalMesh arms = new SkeletalMesh();

        Delay attackDelay = new Delay();

        SoundPlayer fireSoundPlayer;
        SoundPlayer fireSoundPlayer2;

        SoundPlayer hitSoundPlayer;


        bool pendingAttack = false;
        Delay pendingAttackStartDelay = new Delay();
        Delay pendingAttackEndDelay = new Delay();

        Delay reAttack = new Delay();

        int attack = -1;

        bool hadHit = false;

        Delay startTrailDelay = new Delay();

        particle_system_meleeTrail trail;

        float Damage = 30;

        public weapon_sword() 
        {
            IsMelee = true;
            startTrailDelay.AddDelay(100000000);
        }

        public override void Start()
        {
            base.Start();

            fireSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            fireSoundPlayer.SetSound(FmodEventInstance.Create("event:/Weapons/knife/knife_attack"));

            fireSoundPlayer2 = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            fireSoundPlayer2.SetSound(FmodEventInstance.Create("event:/Weapons/knife/knife_attack"));

            hitSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            hitSoundPlayer.SetSound(FmodEventInstance.Create("event:/Weapons/knife/knife_hit"));


            ShowHandL = true;

            LateUpdate();

            Shoot();
        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            LoadVisual();
            AssetRegistry.LoadFmodBankIntoMemory("sounds/banks/weapons.bank");

            

        }

        void StopTrail()
        {
            trail?.Destroy(2);
            trail = null;
        }

        void StartNewTrail()
        {

            trail = new particle_system_meleeTrail();

            Level.GetCurrent().AddEntity(trail);
            trail.LoadAssetsIfNeeded();
        }

        public override void Update()
        {
            base.Update();

            mesh.Update(Time.DeltaTime);


            //pistolAnimation.SetBoneMeshTransformModification("spine_02", transform.ToMatrix());

            if (Input.GetAction("slotMelee").PressedBuffered())
                Shoot();

            if(pendingAttack && pendingAttackStartDelay.Wait() == false && pendingAttackEndDelay.Wait())
                PerformAttack();

            arms.Visible = mesh.Visible = ((ICharacter)player).isFirstPerson();


        }

        void UpdateTrail()
        {

            if(startTrailDelay.Wait() == false)
            {
                StartNewTrail();
                startTrailDelay.AddDelay(1000000);
            }

            if(pendingAttackEndDelay.Wait() == false)
            {
                trail?.StopAll();
            }

            if (trail == null) return;

            trail.RelativeTransform = Camera.GetMatrix();

            Vector3 trailStart = mesh.GetBoneMatrix("trail_start").DecomposeMatrix().Position;
            Vector3 trailEnd = mesh.GetBoneMatrix("trail_end").DecomposeMatrix().Position;

            trail.SetTrailTransform(trailStart, trailEnd);

        }

        public override void Destroy()
        {
            base.Destroy();

            fireSoundPlayer.Destroy(2);
            fireSoundPlayer2.Destroy(2);
            trail?.Destroy(1);
        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            Vector3 offset = Vector3.Zero;// Camera.Forward * -0.05f + Camera.Right * -0.03f;

            mesh.Position = Position;
            mesh.Rotation = Rotation;

            arms.Position = mesh.Position;
            arms.Rotation = mesh.Rotation;

            arms.PastePose(mesh.GetPose());

            ICharacter character = ((ICharacter)player);

            UpdateTrail();

        }

        bool a = false;

        void Shoot()
        {

            if (attackDelay.Wait()) return;


                if (reAttack.Wait())
                {
                    attack++;
                }
                else
                {
                if (attack >= 0)
                    attack = 0;
                }



            if (attack > 2)
                attack = 0;

            if(attack == 2 && hadHit == false)
                attack = 0;


            if (attack == 0)
            {
                mesh.PlayAnimation("attack", false, 0.1f);
                attackDelay.AddDelay(0.4f);
                pendingAttackStartDelay.AddDelay(0.15f);
                Camera.AddCameraShake(new CameraShake(interpIn: 1, duration: 1f, positionAmplitude: new Vector3(0f, 0f, 0f), positionFrequency: new Vector3(0f, 0f, 0f), rotationAmplitude: new Vector3(-5f, -5f, 0f), rotationFrequency: new Vector3(7f, 7f, 0f), falloff: 1f, shakeType: CameraShake.ShakeType.SingleWave)); 
            }
            else if (attack == 1)
            {
                mesh.PlayAnimation("attack2", false, 0.1f);
                attackDelay.AddDelay(0.4f);
                pendingAttackStartDelay.AddDelay(0.15f);
                Camera.AddCameraShake(new CameraShake(interpIn: 1, duration: 1f, positionAmplitude: new Vector3(0f, 0f, 0f), positionFrequency: new Vector3(0f, 0f, 0f), rotationAmplitude: new Vector3(-5f, 5f, 0f), rotationFrequency: new Vector3(7f, 7f, 0f), falloff: 1f, shakeType: CameraShake.ShakeType.SingleWave));
            }
            else if (attack == 2)
            {
                mesh.PlayAnimation("attack_finish", false, 0.1f);
                attackDelay.AddDelay(0.5f);
                pendingAttackStartDelay.AddDelay(0.15f);
            }
            if (attack == -1)
            {
                mesh.PlayAnimation("attack_start", false, 0f);
                attackDelay.AddDelay(0.4f);
                pendingAttackStartDelay.AddDelay(0.0f);
                Camera.AddCameraShake(new CameraShake(interpIn: 1, duration: 1f, positionAmplitude: new Vector3(0f, 0f, 0f), positionFrequency: new Vector3(0f, 0f, 0f), rotationAmplitude: new Vector3(-5f, -5f, 0f), rotationFrequency: new Vector3(7f, 7f, 0f), falloff: 1f, shakeType: CameraShake.ShakeType.SingleWave));
            }

            pendingAttackEndDelay.AddDelay(0.35f);

            StopTrail();
            startTrailDelay.AddDelay(0.12f);

            if (a)
                fireSoundPlayer.Play(true);
            else
                fireSoundPlayer2.Play(true);

            //attackDelay.AddDelay(0.01f);
            //pendingAttackDelay.AddDelay(0);

            pendingAttack = true;


            a = !a;

            reAttack.AddDelay(0.55f);

            return;

            

        }

        public override void Blocked()
        {
            base.Blocked();

            mesh.SetCurrentAnimationFrame(5.5f);

        }

        void PerformAttack()
        {
            var hit = Physics.SphereTrace(Camera.position, Camera.position + Camera.Forward*1.3f, 0.3f, bodies, BodyType.GroupHitTest);

            hadHit = false;


            if (hit.HasHit)
            {

                Entity hitEnt = ((RigidbodyData)hit.HitCollisionObject.UserObject).Entity;


                if (hitEnt != null)
                {
                    hitEnt.OnPointDamage(Damage, hit.HitPointWorld, Camera.rotation.GetForwardVector(),"", player, this);
                    hadHit = true;
                }

                RigidBody rigidBody = (RigidBody)hit.HitCollisionObject;

                var data = rigidBody.GetData();

                if (data != null)
                {
                    if (data.Value.Surface == "default")
                    {
                        GlobalParticleSystem.EmitAt($"hitBlood", hit.HitPointWorld, MathHelper.FindLookAtRotation(Vector3.Zero, hit.HitNormalWorld), new Vector3(10, 10, 10));
                    }
                }
                hitSoundPlayer.Position = hit.HitPointWorld;
                hitSoundPlayer.Update();
                hitSoundPlayer.Play(true);

                pendingAttack = false;

            }


        }

        void CreateHitParticle(Vector3 pos)
        {

            GlobalParticleSystem.EmitAt("hitBlood", pos, Vector3.Zero, Vector3.One);

        }
        void LoadVisual()
        {

            const string pistolPath = "models/weapons/sword.fbx";

            mesh.Scale = new Vector3(1f);
            mesh.LoadFromFile(pistolPath);

            mesh.textureSearchPaths.Add("textures/weapons/knife/");
            mesh.textureSearchPaths.Add("textures/weapons/general/");

            mesh.texture = AssetRegistry.LoadTextureFromFile("textures/weapons/sword/sword.png");

            //mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;
            mesh.UseAlternativeRotationCalculation = true;

            mesh.Transperent = true;

            mesh.AlwaysUpdateVisual = true;


            arms.LoadFromFile(PlayerCharacter.armsModelPath);
            arms.textureSearchPaths.Add("textures/weapons/arms/");
            //arms.CastShadows = false;
            arms.PreloadTextures();
            arms.Viewmodel = true;
            arms.UseAlternativeRotationCalculation = true;


            mesh.SetInterpolationEnabled(true);

            mesh.PlayAnimation("draw", false,0);

            mesh.Position = Camera.position;

            ParticleSystemEnt.Preload("hitSmoke");

            Console.WriteLine("loaded pistol double");

            meshes.Add(mesh);
            meshes.Add(arms);

        }

        public override bool CanChangeSlot()
        {
            return reAttack.Wait() == false;
        }

        public override WeaponData GetDefaultWeaponData()
        {
            return new WeaponData { Slot = -1, weaponType = typeof(weapon_sword), ammo = 0 };
        }

    }
}
