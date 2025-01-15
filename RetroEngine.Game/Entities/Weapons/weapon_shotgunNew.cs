using BulletSharp;
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
    internal class weapon_shotgunNew : Weapon
    {
        SkeletalMesh mesh = new SkeletalMesh();
        SkeletalMesh meshTp = new SkeletalMesh();

        SkeletalMesh arms = new SkeletalMesh();

        Delay attackDelay = new Delay();

        SoundPlayer fireSoundPlayer;

        Animation TpFire = new Animation();

        public weapon_shotgunNew()
        {
            ShowHandL = false;
        }

        public override void Start()
        {
            base.Start();

            fireSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            fireSoundPlayer.SetSound(FmodEventInstance.Create("event:/Weapons/shotgun/shotgun_fire"));
            fireSoundPlayer.Volume = 1f;


            meshTp.ParrentBounds = ((PlayerCharacter)player).GetSkeletalMesh();

            LateUpdate();
        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            LoadVisual();

        }

        public override void Update()
        {
            base.Update();

            mesh.Visible = true;
            mesh.Update(Time.DeltaTime * 1.1f);
            TpFire.Update(Time.DeltaTime);
            

            if (Input.GetAction("attack").Holding())
                Shoot();


            float targetRot = Camera.rotation.X;

            MathHelper.Transform transformBig = new MathHelper.Transform();
            transformBig.Rotation.X = MathHelper.Lerp(0, targetRot, 0.6f);

            MathHelper.Transform transformSmall = new MathHelper.Transform();
            transformSmall.Rotation.X = MathHelper.Lerp(0, targetRot, 0.4f);

            //TpFire.SetBoneMeshTransformModification("spine_03", transformSmall.ToMatrix());
            //TpFire.SetBoneMeshTransformModification("upperarm_l", transformBig.ToMatrix());
            //pistolAnimation.SetBoneMeshTransformModification("head", transformSmall.ToMatrix());
            //TpFire.SetBoneMeshTransformModification("upperarm_r", transformBig.ToMatrix());

            arms.Visible = ((ICharacter)player).isFirstPerson();
            mesh.Viewmodel = ((ICharacter)player).isFirstPerson();
            mesh.Visible = ((ICharacter)player).isFirstPerson();
            meshTp.Visible = !mesh.Visible;
            TpFire.UpdatePose = ((ICharacter)player).isFirstPerson() == false;
        }

        public override void Destroy()
        {
            base.Destroy();

            fireSoundPlayer.Destroy(2);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            mesh.Position = Position + GetWorldSway();
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



            if (loadedAssets == false) return;
            //return;
            if (Time.gameTime - SpawnTime > 0.02f && character.isFirstPerson())
            {

                var trans = mesh.GetBoneMatrix("camera", Matrix.CreateScale(0.01f) * Camera.GetMatrix()).DecomposeMatrix();

                Camera.rotation = trans.Rotation;
                Camera.position = trans.Position;

            }

            fireSoundPlayer.Position = Camera.position;
        }

        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(1);

            fireSoundPlayer.Play(true);

            mesh.PlayAnimation(0,false,0.1f);

            TpFire.PlayAnimation(0,false);

            int i = 0;

            for (float y = -4; y <= 4; y += 2f)
                for (float x = -4; x <= 4; x += 2f)
                {



                    Vector2 v = new Vector2(x, y);

                    x *= 1.2f;

                    if (v.Length() > 4.4)
                        continue;

                    i++;

                    Bullet bullet = new Bullet();
                    bullet.weapon = this;

                    bullet.ignore.Add(player);

                    Vector3 bulletRotation;

                    Vector3 startPos = (Camera.position.ToPhysics() + Camera.rotation.GetForwardVector().ToPhysics() * 0.2f + Camera.rotation.GetRightVector().ToPhysics() / 10f - Camera.rotation.GetUpVector().ToPhysics() / 4f);

                    startPos = Vector3.Lerp(startPos, Camera.position, 0.6f);

                    Vector3 endPos = Camera.position - new Vector3(0, -1, 0) + Camera.rotation.GetForwardVector() * 70 + Camera.rotation.GetRightVector() * x + Camera.rotation.GetUpVector() * y;

                    bulletRotation = MathHelper.FindLookAtRotation(startPos, endPos);

                    bullet.Rotation = bulletRotation;

                    bullet.LifeTime = 0.4f;

                    Level.GetCurrent().AddEntity(bullet);


                    bullet.Position = startPos;

                    bullet.Start();
                    bullet.Speed = 100;
                    bullet.Damage = 6;

                    bullet.ignore.Add(player);

                }

            Console.WriteLine(i);

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
            AnimationPose pose = inPose.Copy();

            pose.LayeredBlend(TpFire.GetBoneByName("spine_03"), TpFire.GetPoseLocal());

            meshTp.PastePoseLocal(pose);

            meshTp.SetBoneMeshTransformModification("spine_03", PlayerBodyAnimator.GetSpineTransforms());

            meshTp.PastePoseLocal(meshTp.GetPoseLocal());

            return meshTp.GetPoseLocal();
        }

        void LoadVisual()
        {
            mesh.Scale = new Vector3(1f);

            mesh.LoadFromFile("models/weapons/shotgun.fbx");

            arms.LoadFromFile(PlayerCharacter.armsModelPath);
            arms.textureSearchPaths.Add("textures/weapons/arms/");

            //mesh.DepthTestEqual = false;
            //mesh.Transparency = 0.5f;
            mesh.Transperent = true;

            //mesh.TwoSided = true;

            mesh.textureSearchPaths.Add("textures/weapons/general/");
            mesh.textureSearchPaths.Add("textures/weapons/shotgun_new/");


            TpFire.LoadFromFile("models/weapons/shotgun.fbx");

            meshTp.LoadFromFile("models/weapons/shotgun.fbx");
            meshTp.textureSearchPaths.Add("textures/weapons/arms/");
            meshTp.textureSearchPaths.Add("textures/weapons/shotgun_new/");
            meshTp.textureSearchPaths.Add("textures/weapons/general/");
            meshTp.DisableOcclusionCulling = true;

            meshTp.PreloadTextures();

            //mesh.CastShadows = false;
            mesh.PreloadTextures();
            mesh.Viewmodel = true;
            mesh.UseAlternativeRotationCalculation = true;
            mesh.CastShadows = true;

            //mesh.DitherDisolve = 0.5f;

            //arms.CastShadows = false;
            arms.PreloadTextures();
            arms.Scale = mesh.Scale;
            arms.Viewmodel = true;
            arms.CastShadows = true;
            arms.UseAlternativeRotationCalculation = true;


            mesh.SetInterpolationEnabled(true);
            

            Console.WriteLine("loaded shotgun");

            mesh.Update(0);

            meshes.Add(mesh);
            meshes.Add(arms);
            meshes.Add(meshTp);
            //new Bullet().LoadAssetsIfNeeded();

        }
    }
}
