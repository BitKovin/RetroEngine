using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using RetroEngine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RetroEngine.Game.Entities;
using RetroEngine.Map;
using RetroEngine.Game.Entities.Weapons;
using RetroEngine.Entities;
using System.Text.Json.Serialization;
using RetroEngine.SaveSystem;
using RetroEngine.Audio;
using RetroEngine.Entities.Light;
using RetroEngine.Game.Effects.Particles;
using RetroEngine.PhysicsSystem;
using BulletSharp.SoftBody;
using RetroEngine.Graphic;
using RetroEngine.ParticleSystem;

namespace RetroEngine.Game.Entities.Player
{

    [LevelObject("info_player_start")]
    public class PlayerCharacter : Entity, ICharacter
    {

        public static PlayerCharacter Instance = null;

        StaticMesh cylinder = new StaticMesh();

        public RigidBody body;

        float maxSpeed = 7f;
        float maxSpeedAir = 2;
        float acceleration = 90;
        float airAcceleration = 20;
        bool tryLimit = false;

        [JsonInclude]
        public Vector3 velocity = new Vector3();

        float bobProgress = 0;

        Vector3 OldCameraPos;

        Vector3 bob;

        float cameraRoll;

        SoundPlayer stepSoundPlayer;
        FmodEventInstance stepSound;

        bool onGround = false;

        [JsonInclude]
        public WeaponData[] weapons = new WeaponData[10];

        [JsonInclude]
        public WeaponData weaponMeele;

        public Weapon currentWeapon;
        [JsonInclude]
        public int currentSlot = -1;
        [JsonInclude]
        public int lastSlot = -1;

        public bool flashlightEnabled = false;

        float bobSpeed = 8;

        PlayerUI PlayerUI;

        Vector3 interpolatedPosition = new Vector3();
        float cameraHeightOffset = 0;

        SkeletalMesh bodyMesh = new SkeletalMesh();
        PlayerBodyAnimator PlayerBodyAnimator = new PlayerBodyAnimator();

        StaticMesh testCube = new StaticMesh();

        [JsonInclude]
        public bool thirdPerson = false;

        PointLight PlayerFlashLight;
        PointLight PlayerAmbientLight;

        float Gravity = -27;

        Shader underWaterEffect;
        PostProcessStep waterPP = new PostProcessStep();

        Vector3 CameraRotation = new Vector3();

        bool useThirdPersonAnimations = false;

        public static string armsModelPath = "models/weapons/arms_n.fbx";

        FmodEventInstance underWaterSound;
        FmodEventInstance underWaterAmbient;
        SoundPlayer underwaterAmbientPlayer;

        FmodEventInstance testSound;

        //particle_system_meleeTrail meleeTrail;
        public PlayerCharacter() : base()
        {
            if (GameMain.platform == Platform.Mobile)
            {

            }

            Tags.Add("player");

            DisablePhysicsInterpolation = true;

            Input.CenterCursor();

            PlayerUI = new PlayerUI(this);

            LateUpdateWhilePaused = true;

            SaveGame = true;

            Health = 100;

        }


        protected override void LoadAssets()
        {
            base.LoadAssets();

            //meleeTrail = ParticleSystemFactory.CreateByTechnicalName("meleeTrail") as particle_system_meleeTrail;

            //Level.GetCurrent().AddEntity(meleeTrail);


            bodyMesh.LoadFromFile("models/player_model_full.FBX");

            //bodyMesh.DisableOcclusionCulling = true;

            bodyMesh.CastGeometricShadow = true;

            bodyMesh.CastViewModelShadows = false;

            bodyMesh.textureSearchPaths.Add("textures/weapons/arms/");

            //bodyMesh.Scale = new Vector3(1.15f);

            PlayerBodyAnimator.LoadAssets();

            meshes.Add(bodyMesh);

            testCube.LoadFromFile("models/cube.obj");
            testCube.texture = AssetRegistry.LoadTextureFromFile("cat.png");
            testCube.Scale = new Vector3(1);
            //meshes.Add(testCube);

            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/SFX.bank");

            underWaterEffect = AssetRegistry.GetPostProcessShaderFromName("UnderWater");
            waterPP.Shader = underWaterEffect;


            Weapon.PreloadAllWeapons();
            PlayerUI.Load();
        }

        public bool Heal(float amount,float maxHealth = 100)
        {

            if (Health >= maxHealth) return false;

            float healthAfterHeal = Health + amount;

            Health = float.Clamp(healthAfterHeal, 0, maxHealth);

            return true;

        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Camera.position = Position = OldCameraPos = data.GetPropertyVectorPosition("origin");

            name = "player";
            Camera.rotation = new Vector3(0, data.GetPropertyFloat("angle") + 90, 0);
            CameraRotation = Camera.rotation;

        }

        public override void Start()
        {
            base.Start();

            Instance = this;

            testSound = FmodEventInstance.Create("event:/NPC/Dog/DogAttack");

            body = Physics.CreateCharacterCapsule(this, 1.8f, 0.4f, 2);
            //body.UserIndex = (int)BodyType.HitTest;
            //body.UserIndex2 = (int)BodyType.CharacterCapsule;
            body.Gravity = new Vector3(0, -27, 0).ToNumerics();

            underWaterSound = FmodEventInstance.Create("snapshot:/UnderWater");

            body.SetPosition(Position.ToPhysics());

            Input.MouseDelta = new Vector2();


            body.CcdMotionThreshold = 0.000001f;
            body.CcdSweptSphereRadius = 0.4f;
            body.Friction = 1f;

            bodies.Add(body);

            stepSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            stepSound = FmodEventInstance.Create("event:/Character/Player Footsteps");
            stepSoundPlayer.SetSound(stepSound);
            stepSoundPlayer.Volume = 0.5f;


            underWaterAmbient = FmodEventInstance.Create("event:/Character/Player/UnderWater");
            underwaterAmbientPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            underwaterAmbientPlayer.SetSound(underWaterAmbient);
            underwaterAmbientPlayer.Is3DSound = false;
            underwaterAmbientPlayer.IsUiSound = true;
            underwaterAmbientPlayer.Play(true);
            underwaterAmbientPlayer.Update();

            stepSound.SetParameter("Surface", 1);


            weaponMeele = new weapon_sword().GetDefaultWeaponData();
            //weapons.Add(new WeaponData { weaponType = typeof(weapon_shotgunNew), ammo = 50 });
            //weapons.Add(new WeaponData { weaponType = typeof(weapon_pistol_double), ammo = 50 });
            SwitchToSlot(0, true);

            interpolatedPosition = Position;

            PlayerFlashLight = Level.GetCurrent().AddEntity(new PointLight()) as PointLight;
            PlayerFlashLight.CastShadows = true;


            PlayerFlashLight.enabled = false;

            PlayerFlashLight.SetAngle(35);
            PlayerFlashLight.radius = 30;
            PlayerFlashLight.Intensity = 1;

            PlayerFlashLight.Start();

            PlayerFlashLight.SetInnterAngle(20);

            interpolatedPosition = Position;

            PlayerBodyAnimator.OnAnimationEvent += PlayerBodyAnimator_OnAnimationEvent;

            if (Level.GetCurrent().FindEntityByName("PlayerGlobal") == null)
                Level.GetCurrent().AddEntity(new Entities.Player.PlayerGlobal());

        }

        public override void Update()
        {
            base.Update();

            if(currentWeapon != null)
            {
                if(currentWeapon.Data.Slot != currentSlot)
                {
                    SwitchToSlot(currentSlot);
                }
            }

            if (Input.GetAction("slotMelee").Pressed())
                SwitchToMeleeWeapon();

            if (Input.GetAction("slot1").Pressed())
                SwitchToSlot(0);

            if (Input.GetAction("slot2").Pressed())
                SwitchToSlot(1);

            if (Input.GetAction("slot3").Pressed())
                SwitchToSlot(2);

            if (Input.GetAction("slot4").Pressed())
                SwitchToSlot(4);

            if (Input.GetAction("lastSlot").Pressed())
                SwitchToSlot(lastSlot);

            if (Input.GetAction("view").Pressed())
                thirdPerson = !thirdPerson;

            if (Input.GetAction("test2").Pressed())
                flashlightEnabled = !flashlightEnabled;

        }

        bool canSwitchSlot(int slot)
        {
            if (currentWeapon == null) return true;

            if(currentWeapon.IsMelee == false)
                if (slot == currentSlot) return false;

            return currentWeapon.CanChangeSlot();

        }
        private void PlayerBodyAnimator_OnAnimationEvent(AnimationEvent animationEvent)
        {
            if(animationEvent.Name == "step")
            {
                PlayStepSound();
            }
        }

        bool oldInWater = false;

        void UpdatePlayerInput()
        {


            CheckUnderWater();

            bool inWater = isInWater();

            if(inWater)
            {
                Gravity = -10;
            }
            else
            {
                Gravity = -27;
            }

            if(oldInWater != inWater)
            {

                if(inWater)
                {
                    EnteredWater();
                }
                else
                {
                    ExitedWater();
                }

            }

            oldInWater = inWater;

            if (FreeCamera.active) return;

            UpdateMovement();

            UpdateCamera();

            if (Input.GetAction("test").Holding())
                PlayerBodyAnimator.FireAction();

            if (Input.GetAction("jump").Holding())
                Jump();

            //if (Input.GetAction("slot0").Pressed())
            //SwitchToSlot(-1);

        }

        void EnteredWater()
        {
            PostProcessStep.StepsAfter.Add(waterPP);
            underWaterSound.StartEvent();
        }

        void ExitedWater()
        {
            PostProcessStep.StepsAfter.Remove(waterPP);
            underWaterSound.Stop();

            //SoundPlayer.PlaySound(testSound);

        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            CheckGround();

            InterpolatePos();

            underwaterAmbientPlayer.Position = Position;

            Camera.velocity = body.LinearVelocity;



            PlayerFlashLight.enabled = flashlightEnabled;


            

            MathHelper.Transform hide = new MathHelper.Transform();
            hide.Scale = Vector3.Zero;
            MathHelper.Transform show = new MathHelper.Transform();

            Matrix showR = new Matrix();
            Matrix showL = new Matrix();

            bodyMesh.MeshHideList.Remove("head");

            if(thirdPerson == false)
                bodyMesh.MeshHideList.Add("head");

            if (thirdPerson || useThirdPersonAnimations)
            {
                bodyMesh.SetBoneMeshTransformModification("upperarm_r", show.ToMatrix());
                bodyMesh.SetBoneMeshTransformModification("upperarm_l", show.ToMatrix());
                bodyMesh.SetBoneMeshTransformModification("head", show.ToMatrix());
            }
            else
            {
                if (currentWeapon == null)
                {
                    showR = showL = show.ToMatrix();
                }
                else
                {
                    if (currentWeapon.ShowHandR)
                    {
                        showR = show.ToMatrix();
                    }
                    else
                    {
                        showR = hide.ToMatrix();
                    }

                    if (currentWeapon.ShowHandL)
                    {
                        showL = show.ToMatrix();
                    }
                    else
                    {
                        showL = hide.ToMatrix();
                    }

                }

                bodyMesh.SetBoneMeshTransformModification("upperarm_r", showR);
                bodyMesh.SetBoneMeshTransformModification("upperarm_l", showL);
                bodyMesh.SetBoneMeshTransformModification("head", hide.ToMatrix());
            }

        }

        public override void VisualUpdate()
        {
            base.VisualUpdate();


            PlayerBodyAnimator.MovementSpeed = (PhysicalVelocity).XZ().Length();


            float Dx = Vector3.Dot(velocity.XZ().Normalized(), CameraRotation.GetRightVector());
            float Dy = Vector3.Dot(velocity.XZ().Normalized(), CameraRotation.GetForwardVector().XZ().Normalized());
            Vector2 dir = new Vector2(Dx, Dy);
            PlayerBodyAnimator.MovementDirection = dir;

            PlayerBodyAnimator.MovementDirection = new Vector2(0, 1);

            PlayerBodyAnimator.Update();

            var pose = PlayerBodyAnimator.GetResultPose();

            AnimationPose resultPose = pose;

            if (currentWeapon != null && (thirdPerson || useThirdPersonAnimations))
            {
                resultPose = currentWeapon.ApplyWeaponAnimation(pose.Copy());

            }

            //bodyMesh.SetWorldPositionOverride("clavicle_l", new MathHelper.Transform { Position = new Vector3(0,3,0)}.ToMatrix());


            bodyMesh.PastePoseLocal(resultPose);

        }

        void InterpolatePos()
        {
            Vector3 oldPos = interpolatedPosition;

            Vector3 newPos = Position;

            Vector3 previousPosition = interpolatedPosition; // This should be updated each physics tick
            Vector3 currentPosition = Position; // This is updated during the physics update

            float fixedDeltaTime = Math.Max(1 / 20f, Time.DeltaTime);

            // Interpolate the position based on the elapsed time in the current frame
            float interpolationFactor = Time.DeltaTime / fixedDeltaTime;
            interpolatedPosition = Vector3.Lerp(previousPosition, currentPosition, Math.Min(interpolationFactor,1));


            cameraHeightOffset = MathHelper.Lerp(cameraHeightOffset, 0, interpolationFactor/5f);


        }

        void UpdateCamera()
        {


            if (GameMain.SkipFrames == 0)
                CameraRotation += new Vector3(Input.MouseDelta.Y, -Input.MouseDelta.X, 0) / 2f;

            CameraRotation = new Vector3(Math.Clamp(CameraRotation.X, -89, 89), CameraRotation.Y, 0);

            Camera.position = interpolatedPosition + new Vector3(0,0.7f,0) + CameraRotation.GetForwardVector().XZ().Normalized()*0.1f;

            bob = Vector3.Zero;

            bob += CameraRotation.GetForwardVector() * (float)Math.Sin(bobProgress * 1 * bobSpeed * 1) * -0.5f;
            //bob += Camera.rotation.GetUpVector() * (float)(Math.Abs(Math.Sin(bobProgress * bobSpeed * 1))) * 0.2f;
        }

        Delay stepSoundCooldown = new Delay();

        void UpdateMovement()
        {

            Vector2 input = new Vector2();

            if (Input.GetAction("moveForward").Holding())
                input += new Vector2(0, 1);

            if (Input.GetAction("moveBackward").Holding())
                input -= new Vector2(0, 1);

            if (Input.GetAction("moveRight").Holding())
                input += new Vector2(1, 0);

            if (Input.GetAction("moveLeft").Holding())
                input -= new Vector2(1, 0);

            //maxSpeed = Input.GetAction("run").Holding() ? 7 : 4;

            Vector3 motion = new Vector3();

            Vector3 right = Camera.rotation.GetRightVector().XZ();
            Vector3 forward = Camera.rotation.GetForwardVector().XZ().Normalized();

            body.Activate(true);

            // Ground movement

            body.Gravity = new System.Numerics.Vector3(0, Gravity, 0);

            velocity = body.LinearVelocity;
            body.Friction = 0.0f;
            if (input.Length() > 0.1f)
            {
                input.Normalize();

                motion += right * input.X;
                motion += forward * input.Y;

                if (onGround)
                {
                    
                    if (Math.Sin(bobProgress * bobSpeed * 2) <= 0 && Math.Sin((bobProgress + Time.DeltaTime) * bobSpeed * 2) > 0)
                    {
                        //stepSoundPlayer.Play(true);
                    }

                    bobProgress += Time.DeltaTime;


                    velocity = UpdateGroundVelocity(motion, velocity);

                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();

                    TryStep(motion.Normalized()/1.5f);

                    TryStep(MathHelper.RotateVector(motion.Normalized() * 1f / 1.6f, Vector3.UnitY, 35));
                    TryStep(MathHelper.RotateVector(motion.Normalized() * 1f / 1.6f, Vector3.UnitY, -35));

                }
                else
                {
                    /*
                    float airControlPower = Vector3.Dot(motion, ((Vector3)body.LinearVelocity).XZ());

                    airControlPower /= 100;

                    airControlPower += 1;
                    airControlPower /= 2;

                    airControlPower = 1 - airControlPower;

                    body.ApplyCentralForce(motion.ToNumerics() * 12 * airControlPower);
                    */

                    velocity = UpdateAirVelocity(motion, velocity);

                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();

                }
            }
            else
            {

                if (onGround)
                {
                    velocity = Friction(velocity);
                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();
                }
                else
                {
                    velocity = UpdateAirVelocity(new Vector3(), velocity);
                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();
                }
                // No input, apply friction
                body.Friction = 0.2f;
            }


            cameraRoll = MathHelper.Lerp(cameraRoll, input.X * 1.5f, Time.DeltaTime * 10);

            Camera.roll = cameraRoll;

            stepSoundPlayer.Position = interpolatedPosition - Vector3.Up;

        }

        void PlayStepSound()
        {
            if (stepSoundCooldown.Wait()) return;
            stepSoundCooldown.AddDelay(0.05f);

            if(onGround)
            stepSoundPlayer.Play(true);

        }
        void UpdateMovementWater()
        {

            Vector2 input = new Vector2();

            if (Input.GetAction("moveForward").Holding())
                input += new Vector2(0, 1);

            if (Input.GetAction("moveBackward").Holding())
                input -= new Vector2(0, 1);

            if (Input.GetAction("moveRight").Holding())
                input += new Vector2(1, 0);

            if (Input.GetAction("moveLeft").Holding())
                input -= new Vector2(1, 0);


            Vector3 motion = new Vector3();

            Vector3 right = Camera.rotation.GetRightVector().XZ();
            Vector3 forward = Camera.rotation.GetForwardVector();

            body.Activate(true);


            body.Gravity = new System.Numerics.Vector3(0, -2, 0);

            // Ground movement

            //velocity = body.LinearVelocity;
            body.Friction = 0.0f;
            if (input.Length() > 0.1f)
            {
                input.Normalize();

                motion += right * input.X;
                motion += forward * input.Y;

                

                    if (Math.Sin(bobProgress * bobSpeed * 2) <= 0 && Math.Sin((bobProgress + Time.DeltaTime) * bobSpeed * 2) > 0)
                    {
                        stepSoundPlayer.Play(true);
                    }

                    bobProgress += Time.DeltaTime;


                    velocity = UpdateGroundVelocity(motion, velocity);

                    body.LinearVelocity = new Vector3(velocity.X, velocity.Y, velocity.Z).ToPhysics();


                
            }
            else
            {

                if (onGround)
                {
                    velocity = Friction(velocity);
                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();
                }
                else
                {
                    velocity = UpdateAirVelocity(new Vector3(), velocity);
                    body.LinearVelocity = new Vector3(velocity.X, body.LinearVelocity.Y, velocity.Z).ToPhysics();
                }
                // No input, apply friction
                body.Friction = 0.2f;
            }


            cameraRoll = MathHelper.Lerp(cameraRoll, input.X * 1.5f, Time.DeltaTime * 10);

            Camera.roll = cameraRoll;

            stepSoundPlayer.Position = interpolatedPosition - Vector3.Up;

        }

        void CheckGround()
        {
            onGround = false;

            float radius = 0.25f;

            if (CheckGroundAtOffset(new Vector3(0, 0, 0)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(radius, 0, 0)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(-radius, 0, 0)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(0, 0, radius)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(0, 0, -radius)))
                onGround = true;



            if (CheckGroundAtOffset(new Vector3(radius * 0.77f, 0, radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(-radius * 0.77f, 0, radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(radius * 0.77f, 0, -radius * 0.77f)))
                onGround = true;

            if (CheckGroundAtOffset(new Vector3(-radius * 0.77f, 0, -radius * 0.77f)))
                onGround = true;

            if(jumpDelay.Wait())
                onGround = false;

        }

        public override void Destroy()
        {

            ExitedWater();

            Instance = null;

            base.Destroy();
        }

        bool CheckGroundAtOffset(Vector3 offset)
        {
            var hit = Physics.LineTrace(Position.ToNumerics() + offset.ToNumerics(), (Position - new Vector3(0, 1.05f, 0) + offset).ToNumerics(), new List<CollisionObject>() { body }, bodyType: BodyType.GroupCollisionTest);

            if(hit.HasHit == false)
                return false;

            RigidBody hitBody = hit.CollisionObject as RigidBody;

            if (hitBody == null) return false;

            if (hitBody.GetBodyType() == BodyType.CharacterCapsule)
            {

                //body.LinearVelocity = hit.HitNormalWorld * 8;
                //body.LinearVelocity *= new System.Numerics.Vector3(1,-2,1);
                //velocity = body.LinearVelocity;
                return false;

            }

            return hit.HitNormalWorld.Y>0.7;
        }

        Delay stepDelay = new Delay();

        void TryStep(Vector3 dir)
        {

            if (stepDelay.Wait()) return;

            Vector3 pos = Position + dir / 1.2f;

            if (pos == Vector3.Zero)
                return;

            var hit = Physics.LineTrace(pos.ToPhysics(), (pos - new Vector3(0, 0.73f, 0)).ToPhysics(), new List<CollisionObject>() { body }, BodyType.World | BodyType.MainBody);

            if (hit.HasHit == false)
                return;

            DrawDebug.Line(hit.HitPointWorld, hit.HitPointWorld + hit.HitNormalWorld, Vector3.UnitX);
            if (hit.HitNormalWorld.Y < 0.95)
                return;

            

            Vector3 hitPoint = hit.HitPointWorld;

            if (hitPoint == Vector3.Zero)
                return;



            if (hitPoint.Y > Position.Y - 1 + 0.8f)
                return;

            if (Physics.SphereTrace(hitPoint + Vector3.UnitY*0.33f, hitPoint + Vector3.UnitY, 0.3f, null, bodyType: BodyType.World).HasHit)
                return;

            if (Physics.SphereTrace(Position, Position + dir.Normalized()*0.2f, 0.3f, null, bodyType: BodyType.World).HasHit)
                return;

            if (Vector3.Distance(hitPoint, Position) > 1.4)
                return;

            hit = Physics.LineTrace(Position.ToPhysics(), Vector3.Lerp(Position, hitPoint, 1.1f).ToPhysics() + Vector3.UnitY.ToPhysics() * 0.2f, new List<CollisionObject>() { body }, body.GetCollisionMask());

            if (hit.HasHit)
            {
                //DrawDebug.Sphere(0.1f, hit.HitPointWorld, Vector3.Zero, 3);
                return;
            }




            DrawDebug.Sphere(0.1f, hitPoint, Vector3.UnitY);


            Vector3 lerpPose = Vector3.Lerp(Position, hitPoint, 0.3f);

            lerpPose.Y = hitPoint.Y + 1;

            float newOffset = Position.Y - lerpPose.Y;

            cameraHeightOffset += newOffset;
            interpolatedPosition.Y -= newOffset;

            body.SetPosition(lerpPose);

            stepDelay.AddDelay(0.1f);

        }

        Vector3 ProjectToGround(Vector3 pos)
        {
            var hit = Physics.LineTrace(pos.ToNumerics(), (pos - new Vector3(0, 100000000, 0)).ToNumerics(), new List<CollisionObject>() { body }, BodyType.World);

            if(hit.HasHit == false)
                return Vector3.Zero;

            return hit.HitPointWorld;

        }

        void FirstPersonCameraUpdate()
        {
            MathHelper.Transform t = bodyMesh.GetBoneMatrix("head").DecomposeMatrix();

            Camera.position = t.Position + Camera.rotation.GetForwardVector().XZ().Normalized() * 0.35f + Vector3.Up*0.15f;
            Camera.position.Y += cameraHeightOffset;
        }

        void FirstPersonFullBodyCameraUpdate()
        {
            MathHelper.Transform t = bodyMesh.GetBoneMatrix("head").DecomposeMatrix();

            Camera.position = t.Position + Camera.Forward * 0.003f;
            Camera.position += Camera.rotation.GetUpVector() * 0.0f;
        }

        void ThirdPersonCameraUpdate()
        {
            Vector3 forward = Camera.rotation.GetForwardVector().XZ().Normalized();

            Camera.position = interpolatedPosition;
            Camera.position += -forward * 0.4f;

            Vector3 startPos = interpolatedPosition + Vector3.Up;

            Vector3 targetCameraPos = Camera.position + new Vector3(0, 0.4f, 0);

            targetCameraPos += Camera.rotation.GetForwardVector() * -2f;
            targetCameraPos += Camera.rotation.GetUpVector() * 0.5f;
            targetCameraPos += Camera.rotation.GetRightVector() * 0.1f;

            var hit = Physics.SphereTrace(startPos.ToPhysics(), targetCameraPos.ToPhysics(), radius: 0.3f, ignoreList: bodies);
            if (hit.HasHit)
                Camera.position = hit.HitPointWorld + hit.HitNormalWorld * 0.3f;
            else
                Camera.position = targetCameraPos;
        }

        public override void LateUpdate()
        {

            if(GameMain.Instance.paused == false)
                UpdatePlayerInput();

            if (FreeCamera.active) return;
            Camera.rotation = CameraRotation;
            if(useThirdPersonAnimations)
            {
                bodyMesh.Position = interpolatedPosition - Camera.rotation.GetForwardVector().XZ().Normalized() * 0.1f - new Vector3(0, 0.90f, 0);
            }
            else
            {
                bodyMesh.Position = interpolatedPosition - Camera.rotation.GetForwardVector().XZ().Normalized() * 0.35f - new Vector3(0, 0.90f, 0);
            }
            bodyMesh.Rotation = new Vector3(0, Camera.rotation.Y, 0);


            if(thirdPerson)
            {
                ThirdPersonCameraUpdate();
                //
            }
            else
            {
                
                
                if(useThirdPersonAnimations)
                    FirstPersonFullBodyCameraUpdate();
                else
                    FirstPersonCameraUpdate();

            }

            Camera.ApplyCameraShake();

            //meleeTrail.SetTrailTransform(bodyMesh.GetBoneMatrix("hand_r").Translation, bodyMesh.GetBoneMatrix("clavicle_r").Translation);

            if (currentWeapon is not null)
            {
                currentWeapon.Position = Camera.position + bob * 0.05f * currentWeapon.BobScale + CameraRotation.GetForwardVector() * Camera.rotation.X / 2000f;
                currentWeapon.Rotation = Vector3.Lerp(Camera.rotation, CameraRotation, 0.2f) - new Vector3(0, 0, (float)Math.Sin(bobProgress * -1 * bobSpeed) * -1.5f) * currentWeapon.BobScale;
            }

            UpdatePlayerLight();

            
            if(Input.GetAction("test").Pressed() && false)
            {
                var result = Physics.MultiLineTrace(Camera.position, Camera.position + Camera.Forward * 50, new List<CollisionObject>() { body }, BodyType.GroupHitTest);

                int i = 0;

                foreach(var hit in result.Hits)
                {
                    DrawDebug.Text(hit.HitPointWorld, i.ToString(), 30);
                    i++;
                }

            }


            cylinder.Position = Position + Camera.rotation.GetForwardVector().XZ() * 3;
        }

        Delay jumpDelay = new Delay();

        void UpdatePlayerLight()
        {

            MathHelper.Transform t = bodyMesh.GetBoneMatrix("spine_03").DecomposeMatrix();

            PlayerFlashLight.Position = t.Position
                + Camera.rotation.GetForwardVector().XZ().Normalized() * 0.2f
                + Camera.rotation.GetRightVector() * -0.1f
                + Vector3.Up * 0.1f + Vector3.Up * cameraHeightOffset;
            PlayerFlashLight.Rotation = Camera.rotation;
        }

        void Jump()
        {
            
            if (onGround && isInWater() == false)
            {
                body.LinearVelocity = new Vector3(body.LinearVelocity.X, 9.5f, body.LinearVelocity.Z).ToNumerics();
                jumpDelay.AddDelay(0.1f);
            }
        }

        Vector3 UpdateGroundVelocity(Vector3 withDir, Vector3 vel)
        {
            vel = vel.XZ();
            vel = Friction(vel);

            float currentSpeed = Vector3.Dot(vel, withDir);
            float addSpeed = Math.Clamp(maxSpeed - currentSpeed, 0, acceleration*Time.DeltaTime);
    
            if(tryLimit)
                if(currentSpeed + addSpeed > maxSpeed)
                    addSpeed = maxSpeed - currentSpeed;

            return vel + addSpeed * withDir;
        }
        Vector3 UpdateAirVelocity(Vector3 wishdir, Vector3 vel)
        {
            vel = vel.XZ();

            float currentSpeed = Vector3.Dot(vel, wishdir);

            float wishspeed = maxSpeedAir;

            float addSpeed = wishspeed - currentSpeed;

            if (addSpeed <= 0f)
            {
                return vel;
            }

            float accelspeed = airAcceleration * Time.DeltaTime * wishspeed;

            if (accelspeed > addSpeed)
            {
                accelspeed = addSpeed;
            }

            return vel + accelspeed * wishdir;
        }


        Vector3 Friction(Vector3 vel, float factor = 50f)
        {


            vel = vel.XZ();

            float length = vel.Length();

            vel.Normalize();

            length -= factor * Time.DeltaTime;

            length = Math.Max(0, length);

            vel = vel * length;

            if (float.IsNaN(vel.X))
                return new Vector3();

            return vel;
        }

        void SwitchToMeleeWeapon(bool forceChange = false)
        {
            if (forceChange == false)
            {
                if(currentWeapon != null)
                    if (currentWeapon.IsMelee) return;

                if (canSwitchSlot(-1) == false) return;

            }
            if (weaponMeele != null)
            {
                SwitchWeapon(weaponMeele);
            }
        }

        void SwitchToSlot(int slot, bool forceChange = false)
        {

            if (slot < 0) return;

            if (forceChange == false)
            {

                if (canSwitchSlot(slot) == false)
                {
                    if (currentWeapon != null)
                    {
                        if (currentWeapon.IsMelee)
                        {
                            lastSlot = currentSlot;
                            currentSlot = slot;
                            
                        }

                        return;

                    }
                }

            }
            if (weapons[slot] != null)
            {

                lastSlot = currentSlot;
                currentSlot = slot;

                SwitchWeapon(weapons[slot]);
            }

        }

        void SwitchWeapon(WeaponData data)
        {
            if (currentWeapon is not null)
            {
                currentWeapon.Destroy();
                currentWeapon = null;
            }

            if (data is not null)
            {

                Weapon newWeapon = Weapon.CreateFromData(data, this);

                currentWeapon = Level.GetCurrent().AddEntity(newWeapon) as Weapon;
            }

        }

        public void AddWeapon(WeaponData weaponData)
        {

            int slot = weaponData.Slot;

            if (weapons[slot] == null)
            {
                weapons[slot] = weaponData;
                SwitchToSlot(slot, false);
            }else if (weapons[slot].Priority < weaponData.Priority)
            {
                weapons[slot] = weaponData;
                SwitchToSlot(slot, true);
            }


            

        }

        bool isInWater()
        {
            return waterBodies > 0 && underWater;
        }

        int waterBodies = 0;

        public override void OnAction(string action)
        {
            base.OnAction(action);

            if(action == "water_enter")
            {
                waterBodies++;
            }

            if(action == "water_exit")
            {
                waterBodies--;
            }


        }


        bool underWater = false;
        void CheckUnderWater()
        {

            Vector3 waterCheckPos = interpolatedPosition + Vector3.UnitY * 0.8f;

            var hit = Physics.LineTrace((waterCheckPos + Vector3.UnitY * 100).ToPhysics(), waterCheckPos.ToPhysics(), bodyType: BodyType.Liquid);

            underWater = hit.HasHit;

            underWaterAmbient.SetParameter("underWater", underWater ? 1 : 0);

        }

        public override void OnPointDamage(float damage, Vector3 point, Vector3 direction, string hitBone = "", Entity causer = null, Entity weapon = null)
        {
            base.OnPointDamage(damage, point, direction, hitBone, causer, weapon);

            GlobalParticleSystem.EmitAt("hitBlood", Vector3.Lerp(point, Camera.position, 0.4f), MathHelper.FindLookAtRotation(Vector3.Zero, -direction), new Vector3(0, 0, damage / 2f));

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            Camera.AddCameraShake(new CameraShake(duration: 1, positionAmplitude: new Vector3(0,0,0), positionFrequency: new Vector3(0,0,0), rotationAmplitude: new Vector3(9,3,0), rotationFrequency: new Vector3(9,-6.4f, 0), falloff: 1, shakeType: CameraShake.ShakeType.SingleWave));

            

        }

        protected override EntitySaveData SaveData(EntitySaveData baseData)
        {

            Rotation = Camera.rotation;

            velocity = body.LinearVelocity;

            return base.SaveData(baseData);

        }

        public override void LoadData(EntitySaveData Data)
        {
            base.LoadData(Data);

            SwitchToSlot(currentSlot, true);
            body.LinearVelocity = velocity.ToPhysics();

            body.SetPosition(Position);
            Camera.rotation = Rotation;
            CameraRotation = Camera.rotation;
            Camera.position = Position + Vector3.UnitY;
            interpolatedPosition = Position;

        }

        public float GetHealth()
        {
            return Health;
        }

        public void SetHealth(float health)
        {
            this.Health = health;
        }

        public RigidBody GetPhysicsBody()
        {
            return body;
        }

        public bool isFirstPerson()
        {
            return (thirdPerson || useThirdPersonAnimations) == false;
        }

        public SkeletalMesh GetSkeletalMesh()
        {
            return bodyMesh;
        }

    }
}
