using BulletSharp;
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
    internal class weapon_pistol : Weapon
    {
        SkeletalMesh mesh = new SkeletalMesh();
        SkeletalMesh meshTp = new SkeletalMesh();

        SkeletalMesh arms = new SkeletalMesh();

        Delay attackDelay = new Delay();

        SoundPlayer fireSoundPlayer;

        float aim = 0;

        float aimAnimation = 0;

        Animation pistolAnimation = new Animation();
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

            if (((ICharacter)player).isFirstPerson())
            {
                mesh.Update(Time.DeltaTime * 1.2f);
            }
            else
            {

                float targetRot = Camera.rotation.X;

                MathHelper.Transform transformBig = new MathHelper.Transform();
                transformBig.Rotation.X = MathHelper.Lerp(0, targetRot, 0.6f) * MathHelper.Saturate(aimAnimation);

                MathHelper.Transform transformSmall = new MathHelper.Transform();
                transformSmall.Rotation.X = MathHelper.Lerp(0, targetRot, 0.4f);

                pistolAnimation.SetBoneMeshTransformModification("spine_03", transformSmall.ToMatrix());
                pistolAnimation.SetBoneMeshTransformModification("upperarm_l", transformBig.ToMatrix());
                //pistolAnimation.SetBoneMeshTransformModification("head", transformSmall.ToMatrix());
                pistolAnimation.SetBoneMeshTransformModification("upperarm_r", transformBig.ToMatrix());

                pistolAnimation.Update(Time.DeltaTime);
            }



            //pistolAnimation.SetBoneMeshTransformModification("spine_02", transform.ToMatrix());

            aimAnimation -= Time.DeltaTime*2;
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
            aim-= Time.DeltaTime * 3;
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

            aim = Math.Clamp(aim, 0, 1);

            Offset = Vector3.Lerp(Vector3.Zero, new Vector3(-0.052488543f, 0.07340118f, -0.12f), aim);

            BobScale = 1 - aim;

        }

        public override void Destroy()
        {
            base.Destroy();

            fireSoundPlayer.Destroy(2);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            mesh.Position = Position + GetWorldSway() + GetWorldOffset();
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

            fireSoundPlayer.Position = Camera.position;
        }
        bool r;
        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.4f);

            fireSoundPlayer.Play(true);

            mesh.PlayAnimation(0,false);

            pistolAnimation.PlayAnimation(0,false);

            Bullet bullet = new Bullet();
            bullet.ignore.Add(player);

            Vector3 bulletRotation;

            float x = 0;
            float y = 0;

            Vector3 startPos = Camera.position + Camera.rotation.GetForwardVector() * 0.5f + Camera.rotation.GetRightVector() / 8f - Camera.rotation.GetUpVector() / 5f;

            ICharacter character = ((ICharacter)player);
            if (character.isFirstPerson()==false)
            {
                startPos = meshTp.GetBoneMatrix("muzzle_r").DecomposeMatrix().Position;

                r = !r;
            }

            Vector3 endPos = Camera.position + Camera.rotation.GetForwardVector() * 100 + Camera.rotation.GetRightVector() * x + Camera.rotation.GetUpVector() * y;

            bulletRotation = MathHelper.FindLookAtRotation(startPos, endPos);

            bullet.Rotation = bulletRotation;

            bullet.LifeTime = 2f;

            Level.GetCurrent().AddEntity(bullet);

            aimAnimation = 3;

            bullet.Position = startPos;

            bullet.Start();
            bullet.Speed = 100;
            bullet.Damage = 50;

            bullet.ignore.Add(player);


            WeaponFireFlash.CreateAt(startPos + Camera.Forward * 0.2f);


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

            AnimationPose p = new AnimationPose(pose);

            pose.LayeredBlend(pistolAnimation.GetBoneByName("spine_02"), pistolAnimation.GetPoseLocal(), MathHelper.Saturate(aimAnimation), 1);
            pose.LayeredBlend(pistolAnimation.GetBoneByName("hand_r"), pistolAnimation.GetPoseLocal(), 1, 0);
            pose.LayeredBlend(pistolAnimation.GetBoneByName("clavicle_l"), p, 1);

            meshTp.PastePoseLocal(pose);
            return pose;
        }
        void LoadVisual()
        {
            mesh.Scale = new Vector3(1f);

            mesh.LoadFromFile("models/weapons/pistol2.fbx");

            arms.LoadFromFile("models/weapons/arms_n.fbx");

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
            meshTp.SetBoneLocalTransformModification("weapon_l", new Matrix());
            meshTp.PreloadTextures();

            arms.CastShadows = false;
            arms.PreloadTextures();
            arms.Viewmodel = true;
            arms.UseAlternativeRotationCalculation = true;

            pistolAnimation.LoadFromFile("models/weapons/pistol_tp.fbx");
            pistolAnimation.SetAnimation(0);

            mesh.SetInterpolationEnabled(true);
            

            Console.WriteLine("loaded pistol");

            meshes.Add(mesh);
            meshes.Add(arms);
            meshes.Add(meshTp);

            new Bullet().LoadAssetsIfNeeded();

        }
    }
}
