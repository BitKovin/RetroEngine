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
    internal class weapon_shotgun_double : Weapon
    {
        SkeletalMesh mesh = new SkeletalMesh();
        SkeletalMesh mesh2 = new SkeletalMesh();
        SkeletalMesh meshTp = new SkeletalMesh();

        SkeletalMesh arms = new SkeletalMesh();
        SkeletalMesh arms2 = new SkeletalMesh();

        Delay attackDelay = new Delay();

        SoundPlayer fireSoundPlayer;

        Animation TpFire = new Animation();

        public weapon_shotgun_double()
        {
            ShowHandL = false;

            Offset = new Vector3(0.035f, 0, 0);

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
            TpFire.Update(Time.DeltaTime);
            

            if (Input.GetAction("attack").Holding())
                Shoot();

            SetHideLeftHand(mesh, ShouldHideLeftHand());
            SetHideLeftHand(mesh2, ShouldHideLeftHand());

            mesh.Update(Time.DeltaTime * 1.2f);
            mesh2.Update(Time.DeltaTime * 1.2f);

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


            arms2.Visible = ((ICharacter)player).isFirstPerson();
            mesh2.Viewmodel = ((ICharacter)player).isFirstPerson();
            mesh2.Visible = ((ICharacter)player).isFirstPerson();

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

            float depthDif = 0.04f;
            float depthOffset = -0.05f;

            Vector3 forward = Camera.rotation.GetForwardVector();
            Vector3 up = Camera.rotation.GetUpVector();
            Vector3 right = Camera.rotation.GetRightVector();

            mesh.Position = Position + GetWorldSway() + GetWorldOffset() + forward * depthDif + forward * depthOffset;
            mesh.Rotation = Rotation;

            arms.Position = mesh.Position;
            arms.Rotation = mesh.Rotation;

            mesh2.Position = Position + GetWorldSway() + GetWorldOffset(true) - forward * depthDif + forward * depthOffset - up * 0.02f + right * 0.02f;
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

            if (FreeCamera.active) return;


            if (loadedAssets == false) return;
            //return;
            if (Time.GameTime - SpawnTime > 0.02f && character.isFirstPerson())
            {

                var trans = mesh.GetBoneMatrix("camera", Matrix.CreateScale(0.01f) * Camera.GetMatrix()).DecomposeMatrix();

                //Camera.rotation = trans.Rotation;
                //Camera.position = trans.Position;

            }

            fireSoundPlayer.Position = Camera.position;
        }

        public override bool ShouldHideLeftHand()
        {
            return true;
        }

        bool a = true;

        void Shoot()
        {
            if (Drawing) return;

            if (attackDelay.Wait()) return;

            attackDelay.AddDelay(0.5f);

            fireSoundPlayer.Play(true);

            if(a)
            {
                mesh.PlayAnimation(0, false, 0.1f);
            }
            else
            {
                mesh2.PlayAnimation(0, false, 0.1f);
            }


            TpFire.PlayAnimation(0,false);

            Camera.AddCameraShake(new CameraShake(interpIn: 0.15f, duration: 1, positionAmplitude: new Vector3(0f, 0f, -0.2f), positionFrequency: new Vector3(0f, 0f, 6.4f), rotationAmplitude: new Vector3(-3f, 0.15f, 0f), rotationFrequency: new Vector3(-5f, 28.8f, 0f), falloff: 1f, shakeType: CameraShake.ShakeType.SingleWave));


            int i = 0;

            WeaponFireFlash.CreateAt(Camera.position + Camera.Forward * 0.3f, 0.15f, 15, 1f);

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

                    Vector3 right = Camera.rotation.GetRightVector();

                    if(a == false)
                        right = -right;

                    Vector3 startPos = (Camera.position + Camera.rotation.GetForwardVector() * 0.2f + right / 10f - Camera.rotation.GetUpVector() / 6f);

                    startPos = Vector3.Lerp(startPos, Camera.position, 0.6f);

                    Vector3 endPos = Camera.position - new Vector3(0, -1, 0) + Camera.rotation.GetForwardVector() * 70 + right * x + Camera.rotation.GetUpVector() * y;




                    bulletRotation = MathHelper.FindLookAtRotation(startPos, endPos);

                    bullet.Rotation = bulletRotation;

                    bullet.LifeTime = 0.4f;

                    Level.GetCurrent().AddEntity(bullet);


                    bullet.Position = startPos;

                    bullet.Start();
                    bullet.Speed = 100;
                    bullet.Damage = 4;

                    bullet.ignore.Add(player);

                }


            a = !a;

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

            mesh.LoadFromFile("models/weapons/shotgun.fbx");
            mesh.Transperent = true;

            mesh.PreloadTextures();
            mesh.Viewmodel = true;

            mesh.textureSearchPaths.Add("textures/weapons/general/");
            mesh.textureSearchPaths.Add("textures/weapons/shotgun_new/");

            mesh.SetInterpolationEnabled(true);

            mesh.CastShadows = true;

            mesh2.LoadFromFile("models/weapons/shotgun.fbx");
            mesh2.Transperent = true;

            mesh2.PreloadTextures();
            mesh2.Viewmodel = true;

            mesh2.textureSearchPaths.Add("textures/weapons/general/");
            mesh2.textureSearchPaths.Add("textures/weapons/shotgun_new/");

            mesh2.SetInterpolationEnabled(true);

            mesh2.CastShadows = true;

            arms.LoadFromFile(PlayerCharacter.armsModelPath);
            arms.textureSearchPaths.Add("textures/weapons/arms/");

            arms.PreloadTextures();
            arms.Scale = mesh.Scale;
            arms.Viewmodel = true;
            arms.CastShadows = true;


            arms2.LoadFromFile(PlayerCharacter.armsModelPath);
            arms2.textureSearchPaths.Add("textures/weapons/arms/");

            arms2.PreloadTextures();
            arms2.Scale = mesh.Scale;
            arms2.Viewmodel = true;
            arms2.CastShadows = true;

            mesh2.Scale = new Vector3(-1,1,1);
            arms2.Scale = new Vector3(-1,1,1);




            TpFire.LoadFromFile("models/weapons/shotgun.fbx");

            meshTp.LoadFromFile("models/weapons/shotgun.fbx");
            meshTp.textureSearchPaths.Add("textures/weapons/arms/");
            meshTp.textureSearchPaths.Add("textures/weapons/shotgun_new/");
            meshTp.textureSearchPaths.Add("textures/weapons/general/");
            meshTp.DisableOcclusionCulling = true;

            meshTp.PreloadTextures();


           

            Console.WriteLine("loaded shotgun double");

            mesh.Update(0);
            mesh2.Update(0);

            meshes.Add(mesh);
            meshes.Add(arms);
            meshes.Add(mesh2);
            meshes.Add(arms2);
            meshes.Add(meshTp);

        }

        public override WeaponData GetDefaultWeaponData()
        {
            return new WeaponData { Slot = 2, weaponType = typeof(weapon_shotgun_double), ammo = 6, Priority = 1 };
        }

    }
}
