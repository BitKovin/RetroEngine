using BulletSharp;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
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
    internal class weapon_pistol_double : Weapon
    {
        SkeletalMesh mesh = new SkeletalMesh();
        SkeletalMesh meshTp = new SkeletalMesh();

        SkeletalMesh arms = new SkeletalMesh();

        Delay attackDelay = new Delay();

        SoundPlayer fireSoundPlayer;


        float aim = 0;

        float aimAnimation = 0;

        Animation pistolAnimationAim = new Animation();
        Animation pistolAnimationIdle = new Animation();
        public override void Start()
        {
            base.Start();

            fireSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            fireSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/pistol_fire.wav"));
            fireSoundPlayer.AudioClip.Is3D = false;
            fireSoundPlayer.Volume = 0.15f;


        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            LoadVisual();


        }

        public override void Update()
        {
            base.Update();

            if (((ICharacter)player).isFirstPerson())
            {
                mesh.Update(Time.DeltaTime * 1.2f);
            }
            else
            {

                float targetRot = Camera.rotation.X;

                MathHelper.Transform transformBig = new MathHelper.Transform();
                transformBig.Rotation.X = MathHelper.Lerp(0, targetRot, 0.6f);

                MathHelper.Transform transformSmall = new MathHelper.Transform();
                transformSmall.Rotation.X = MathHelper.Lerp(0, targetRot, 0.4f);

                pistolAnimationAim.SetBoneMeshTransformModification("spine_03", transformSmall.ToMatrix());
                pistolAnimationAim.SetBoneMeshTransformModification("upperarm_l", transformBig.ToMatrix());
                //pistolAnimation.SetBoneMeshTransformModification("head", transformSmall.ToMatrix());
                pistolAnimationAim.SetBoneMeshTransformModification("upperarm_r", transformBig.ToMatrix());

                pistolAnimationAim.Update(Time.DeltaTime);
            }



            //pistolAnimation.SetBoneMeshTransformModification("spine_02", transform.ToMatrix());

            aimAnimation -= Time.DeltaTime * 2;
            aimAnimation = MathF.Max(0, aimAnimation);

            if (Input.GetAction("attack").Holding())
                Shoot();


            arms.Visible = ((ICharacter)player).isFirstPerson();
            mesh.Visible = ((ICharacter)player).isFirstPerson();

            meshTp.Visible = ((ICharacter)player).isFirstPerson() == false;

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
        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            mesh.Position = Position + GetWorldSway() * (1f - aim) + GetWorldOffset();
            mesh.Rotation = Rotation + DrawRotation;

            arms.Position = mesh.Position;
            arms.Rotation = mesh.Rotation;

            arms.PastePose(mesh.GetPose());

            ICharacter character = ((ICharacter)player);

            if (character.isFirstPerson() == false)
            {
                meshTp.Position = character.GetSkeletalMesh().Position;
                meshTp.Rotation = character.GetSkeletalMesh().Rotation;
                //mesh.PastePose(character.GetSkeletalMesh().GetPose());
            }

            fireSoundPlayer.Position = Camera.position + Camera.rotation.GetForwardVector()*0.2f;
        }
        bool r;
        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.2f);

            fireSoundPlayer.Play(true);

            mesh.PlayAnimation(0, false);

            pistolAnimationAim.PlayAnimation(0, false);

            Bullet bullet = new Bullet();
            bullet.ignore.Add(player);

            Vector3 bulletRotation;

            float x = 0;
            float y = 0;

            Vector3 startPos = Camera.position + Camera.rotation.GetForwardVector() * 0.5f + Camera.rotation.GetRightVector() / 8f - Camera.rotation.GetUpVector() / 5f;

            ICharacter character = ((ICharacter)player);
            if (character.isFirstPerson() == false)
            {
                if (r)
                    startPos = meshTp.GetBoneMatrix("muzzle_r").DecomposeMatrix().Position;
                else
                    startPos = meshTp.GetBoneMatrix("muzzle_l").DecomposeMatrix().Position;

                r = !r;
            }

            Vector3 endPos = Camera.position + Camera.rotation.GetForwardVector() * 50 + Camera.rotation.GetRightVector() * x + Camera.rotation.GetUpVector() * y;

            bulletRotation = MathHelper.FindLookAtRotation(startPos, endPos);

            bullet.Rotation = bulletRotation;

            bullet.LifeTime = 2f;

            Level.GetCurrent().AddEntity(bullet);


            bullet.Position = startPos;

            bullet.Start();
            bullet.Speed = 100;
            bullet.Damage = 50;

            bullet.ignore.Add(player);

            aimAnimation = 3;


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

        public override AnimationPose ApplyWeaponAnimation(AnimationPose inPose)
        {

            AnimationPose pose = inPose;

            AnimationPose weaponPose = Animation.LerpPose(pistolAnimationIdle.GetPoseLocal(), pistolAnimationAim.GetPoseLocal(), MathHelper.Saturate(aimAnimation));

            pose.LayeredBlend(pistolAnimationAim.GetBoneByName("spine_02"), weaponPose,1, MathHelper.Saturate(aimAnimation));

            meshTp.PastePoseLocal(pose);
            return pose;
        }
        void LoadVisual()
        {
            mesh.Scale = new Vector3(1f);

            mesh.LoadFromFile("models/weapons/pistol2.fbx");

            arms.LoadFromFile("models/weapons/arms.fbx");

            mesh.textureSearchPaths.Add("textures/weapons/arms/");
            mesh.textureSearchPaths.Add("textures/weapons/pistol/");
            mesh.textureSearchPaths.Add("textures/weapons/general/");


            mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;
            mesh.UseAlternativeRotationCalculation = true;


            meshTp.LoadFromFile("models/weapons/pistol_tp.fbx");

            meshTp.textureSearchPaths.Add("textures/weapons/pistol/");
            meshTp.textureSearchPaths.Add("textures/weapons/general/");

            meshTp.PreloadTextures();

            meshTp.DisableOcclusionCulling = true;

            arms.CastShadows = false;
            arms.PreloadTextures();
            arms.Viewmodel = true;
            arms.UseAlternativeRotationCalculation = true;

            pistolAnimationAim.LoadFromFile("models/weapons/animations/pistolTP/pistols.fbx");
            pistolAnimationAim.SetAnimation(0);

            pistolAnimationIdle.LoadFromFile("models/weapons/animations/pistolTP/pistols.fbx");
            pistolAnimationIdle.SetAnimation("pistol_idle");

            mesh.SetInterpolationEnabled(true);


            Console.WriteLine("loaded pistol double");

            meshes.Add(mesh);
            meshes.Add(arms);
            meshes.Add(meshTp);

            new Bullet().LoadAssetsIfNeeded();

        }
    }
}
