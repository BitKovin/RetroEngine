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
    internal class weapon_pistol_double : Weapon
    {
        SkeletalMesh mesh = new SkeletalMesh();
        SkeletalMesh mesh2 = new SkeletalMesh();
        SkeletalMesh meshTp = new SkeletalMesh();

        SkeletalMesh arms = new SkeletalMesh();
        SkeletalMesh arms2 = new SkeletalMesh();

        Delay attackDelay = new Delay();

        SoundPlayer fireSoundPlayer;
        SoundPlayer fireSoundPlayer2;


        float aim = 0;

        float aimAnimation = 0;

        Animation pistolAnimationAim = new Animation();
        Animation pistolAnimationIdle = new Animation();
        public override void Start()
        {
            base.Start();

            fireSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            fireSoundPlayer.SetSound(FmodEventInstance.Create("event:/Weapons/pistol/pistol_fire"));
            fireSoundPlayer.SetEventProperty("pistol_right_hand", 1);

            fireSoundPlayer2 = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            fireSoundPlayer2.SetSound(FmodEventInstance.Create("event:/Weapons/pistol/pistol_fire"));
            fireSoundPlayer2.SetEventProperty("pistol_right_hand", -1);

            ShowHandL = false;


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

            mesh.Update(Time.DeltaTime * 1.2f);
            mesh2.Update(Time.DeltaTime * 1.2f);

            if (((ICharacter)player).isFirstPerson())
            {

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


            arms2.Visible = mesh2.Visible = arms.Visible = mesh.Visible = ((ICharacter)player).isFirstPerson();

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
            fireSoundPlayer2.Destroy(2);
        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            float depthDif = 0.04f;
            float depthOffset = -0.05f;

            Vector3 forward = Camera.rotation.GetForwardVector();
            Vector3 up = Camera.rotation.GetUpVector();
            Vector3 right = Camera.rotation.GetRightVector();

            mesh.Position = Position + GetWorldSway() * (1f - aim) + GetWorldOffset() + forward * depthDif + forward*depthOffset;
            mesh.Rotation = Rotation;

            arms.Position = mesh.Position;
            arms.Rotation = mesh.Rotation;

            mesh2.Position = Position + GetWorldSway() * (1f - aim) + GetWorldOffset() - forward * depthDif + forward * depthOffset - up*0.02f + right*0.02f;
            mesh2.Rotation = Rotation;
            arms2.Position = mesh2.Position;
            arms2.Rotation = mesh2.Rotation;

            arms.PastePose(mesh.GetPose());
            arms2.PastePose(mesh2.GetPose());

            ICharacter character = ((ICharacter)player);

            if (character.isFirstPerson() == false)
            {
                meshTp.Position = character.GetSkeletalMesh().Position;
                meshTp.Rotation = character.GetSkeletalMesh().Rotation;
                //mesh.PastePose(character.GetSkeletalMesh().GetPose());
            }
        }
        bool r;
        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.17f);

            

            if (r)
            {
                mesh.PlayAnimation("fire", false, 0.05f);
                fireSoundPlayer.Play(true);
            }
            else
            {
                mesh2.PlayAnimation("fire", false, 0.05f);
                fireSoundPlayer2.Play(true);
            }

            pistolAnimationAim.PlayAnimation(0, false);

            Bullet bullet = new Bullet();
            bullet.ignore.Add(player);

            Vector3 bulletRotation;

            float x = 0;
            float y = 0;

            Vector3 startPos = Camera.position + Camera.rotation.GetForwardVector() * 0.5f + Camera.rotation.GetRightVector() / 5f * (r ? 1 :-0.8f) - Camera.rotation.GetUpVector() / 5 - (r? Vector3.Zero : Camera.Up*0.01f);

            ICharacter character = ((ICharacter)player);
            if (character.isFirstPerson() == false)
            {
                if (r)
                    startPos = meshTp.GetBoneMatrix("muzzle_r").DecomposeMatrix().Position;
                else
                    startPos = meshTp.GetBoneMatrix("muzzle_l").DecomposeMatrix().Position;

                
            }

            Vector3 endPos = Camera.position + Camera.rotation.GetForwardVector() * 50;

            bulletRotation = MathHelper.FindLookAtRotation(startPos, endPos);

            bullet.Rotation = bulletRotation;

            bullet.LifeTime = 1f;

            WeaponFireFlash.CreateAt(startPos + Camera.Forward*0.3f);

            Level.GetCurrent().AddEntity(bullet);


            bullet.Position = startPos;

            bullet.Start();
            bullet.Speed = 200;
            bullet.Damage = 50;

            bullet.ignore.Add(player);

            aimAnimation = 3;

            r = !r;
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

            const string pistolPath = "models/weapons/pistol_revolver.fbx";

            mesh.Scale = new Vector3(1f);
            mesh.LoadFromFile(pistolPath);

            mesh.textureSearchPaths.Add("textures/weapons/pistol/");
            mesh.textureSearchPaths.Add("textures/weapons/general/");

            //mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;
            mesh.UseAlternativeRotationCalculation = true;

            mesh2.LoadFromFile(pistolPath);

            mesh2.textureSearchPaths.Add("textures/weapons/pistol/");
            mesh2.textureSearchPaths.Add("textures/weapons/general/");

            mesh.Transperent = true;
            mesh2.Transperent = true;

            mesh.EmissionPower = mesh2.EmissionPower = 2;

            //mesh2.CastShadows = false;
            mesh2.PreloadTextures();
            mesh2.Viewmodel = true;
            mesh2.UseAlternativeRotationCalculation = true;

            mesh2.Scale = arms2.Scale = new Vector3(-1, 1, 1);

            mesh.AlwaysUpdateVisual = mesh2.AlwaysUpdateVisual = true;

            meshTp.LoadFromFile("models/weapons/pistol_tp.fbx");

            meshTp.textureSearchPaths.Add("textures/weapons/pistol/");
            meshTp.textureSearchPaths.Add("textures/weapons/general/");

            meshTp.PreloadTextures();

            meshTp.DisableOcclusionCulling = true;

            arms.LoadFromFile("models/weapons/arms_n.fbx");
            arms.textureSearchPaths.Add("textures/weapons/arms/");
            //arms.CastShadows = false;
            arms.PreloadTextures();
            arms.Viewmodel = true;
            arms.UseAlternativeRotationCalculation = true;

            arms2.LoadFromFile("models/weapons/arms_n.fbx");
            arms2.textureSearchPaths.Add("textures/weapons/arms/");
            //arms2.CastShadows = false;
            arms2.PreloadTextures();
            arms2.Viewmodel = true;
            arms2.UseAlternativeRotationCalculation = true;

            pistolAnimationAim.LoadFromFile("models/weapons/animations/pistolTP/pistols.fbx");
            pistolAnimationAim.SetAnimation(0);

            pistolAnimationIdle.LoadFromFile("models/weapons/animations/pistolTP/pistols.fbx");
            pistolAnimationIdle.SetAnimation("pistol_idle");

            mesh.SetInterpolationEnabled(true);

            mesh.PlayAnimation("draw", false,0);
            mesh2.PlayAnimation("draw",false,0);

            mesh.Position = Camera.position;
            

            Console.WriteLine("loaded pistol double");

            meshes.Add(mesh);
            meshes.Add(arms);
            meshes.Add(mesh2);
            meshes.Add(arms2);
            meshes.Add(meshTp);

            new Bullet().LoadAssetsIfNeeded();

        }
    }
}
