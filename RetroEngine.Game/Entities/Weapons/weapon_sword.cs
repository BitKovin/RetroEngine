using BulletSharp;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using RetroEngine.Audio;
using RetroEngine.Entities;
using RetroEngine.Game.Entities.Player;
using RetroEngine.PhysicsSystem;
using RetroEngine.Skeletal;
using System;
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


        float aim = 0;

        float aimAnimation = 0;

        bool pendingAttack = false;
        Delay pendingAttackDelay = new Delay();

        Delay reAttack = new Delay();

        int attack = 0;

        bool hadHit = false;

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

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            LoadVisual();
            AssetRegistry.LoadFmodBankIntoMemory("sounds/banks/weapons.bank");

        }

        public override void Update()
        {
            base.Update();

            mesh.Update(Time.DeltaTime);



            //pistolAnimation.SetBoneMeshTransformModification("spine_02", transform.ToMatrix());

            aimAnimation -= Time.DeltaTime * 2;
            aimAnimation = MathF.Max(0, aimAnimation);

            if (Input.GetAction("attack").Holding())
                Shoot();

            if(pendingAttack && pendingAttackDelay.Wait() == false)
                PerformAttack();

            arms.Visible = mesh.Visible = ((ICharacter)player).isFirstPerson();


            UpdateAim();

        }

        void IncreaseAim()
        {
            aim += Time.DeltaTime * 4;
        }

        void DecreaseAim()
        {
            aim -= Time.DeltaTime * 3;
        }

        void UpdateAim()
        {
            if (Input.GetAction("attack2").Holding())
            {
                IncreaseAim();
            }
            else
            {
                DecreaseAim();
            }

            if(Input.GetAction("attack2").Pressed())
            {
                BoxDynamic boxDynamic = new BoxDynamic();
                boxDynamic.Position = Camera.position + Camera.rotation.GetForwardVector()*2;
                boxDynamic.Start();
                Level.GetCurrent().AddEntity(boxDynamic);
            }

            aim = Math.Clamp(aim, 0, 1);

            Offset = Vector3.Lerp(Vector3.Zero, new Vector3(-0.052488543f, 0.07340118f, -0.12f), aim);

            BobScale = 1 - aim;

        }

        public override void Destroy()
        {
            base.Destroy();

            fireSoundPlayer.Destroy(2);
            fireSoundPlayer2.Destroy(2);
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
        }

        bool a = false;

        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            if (reAttack.Wait())
            {
                attack++;
            }
            else
            {
                attack = 0;
            }



            if (attack > 2)
                attack = 0;

            if(attack == 2 && hadHit == false)
                attack = 0;

            Console.WriteLine(attack);

            if (attack == 0)
            {
                mesh.PlayAnimation("attack", false, 0.1f);
                attackDelay.AddDelay(0.5f);
                pendingAttackDelay.AddDelay(0.25f);
            }
            else if (attack == 1)
            {
                mesh.PlayAnimation("attack2", false, 0.1f);
                attackDelay.AddDelay(0.5f);
                pendingAttackDelay.AddDelay(0.25f);
            }
            else if (attack == 2)
            {
                mesh.PlayAnimation("attack_finish", false, 0.1f);
                attackDelay.AddDelay(0.7f);
                pendingAttackDelay.AddDelay(0.3f);
            }

            if (a)
                fireSoundPlayer.Play(true);
            else
                fireSoundPlayer2.Play(true);



            pendingAttack = true;


            aimAnimation = 3;

            a = !a;

            reAttack.AddDelay(0.8f);

            return;

            

        }

        void PerformAttack()
        {
            var hit = Physics.SphereTrace(Camera.position, Camera.position + Camera.Forward*1.3f, 0.15f, new List<CollisionObject> { player.bodies[0] }, BodyType.GroupHitTest);

            hadHit = false;

            if (hit.HasHit)
            {

                Entity hitEnt = ((RigidbodyData)hit.HitCollisionObject.UserObject).Entity;

                Console.WriteLine((hit.HitCollisionObject.CollisionShape.UserObject as Physics.CollisionShapeData).surfaceType);

                if (hitEnt != null)
                {
                    hitEnt.OnPointDamage(30, hit.HitPointWorld, Camera.rotation.GetForwardVector(), player, this);
                    hadHit = true;
                }

                CreateHitParticle(hit.HitPointWorld + hit.HitNormalWorld * 0.1f);

                hitSoundPlayer.Position = hit.HitPointWorld;
                hitSoundPlayer.Update();
                hitSoundPlayer.Play(true);

            }

            pendingAttack = false;

        }

        void CreateHitParticle(Vector3 pos)
        {

            ParticleSystem system = ParticleSystem.Create("hitSmoke");
            system.Position = pos;
            system.Start();

        }
        void LoadVisual()
        {

            const string pistolPath = "models/weapons/sword.fbx";

            mesh.Scale = new Vector3(1f);
            mesh.LoadFromFile(pistolPath);

            mesh.textureSearchPaths.Add("textures/weapons/knife/");
            mesh.textureSearchPaths.Add("textures/weapons/general/");

            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

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
            

            Console.WriteLine("loaded pistol double");

            meshes.Add(mesh);
            meshes.Add(arms);

            new Bullet().LoadAssetsIfNeeded();

        }
    }
}
