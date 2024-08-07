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

namespace RetroEngine.Game.Entities.Player
{

    [LevelObject("info_player_start")]
    public class PlayerCharacter : Entity, ICharacter
    {


        StaticMesh cylinder = new StaticMesh();

        public RigidBody body;

        float maxSpeed = 8;
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
        public List<WeaponData> weapons = new List<WeaponData>();

        public Weapon currentWeapon;
        [JsonInclude]
        public int currentSlot = -1;
        [JsonInclude]
        public int lastSlot = -1;

        public bool flashlightEnabled = false;

        float bobSpeed = 8;

        PlayerUI PlayerUI;

        Vector3 interpolatedPosition = new Vector3();

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

        FmodEventInstance underWaterSound;

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

            

            SaveGame = true;

        }

        private void ButtonRotate_onClicked()
        {

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



        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Camera.position = Position = OldCameraPos = data.GetPropertyVectorPosition("origin");

            name = "player";
            Camera.rotation = new Vector3(0, data.GetPropertyFloat("angle") + 90, 0);

        }

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1.8f, 0.35f, 2);
            //body.UserIndex = (int)BodyType.HitTest;
            //body.UserIndex2 = (int)BodyType.CharacterCapsule;
            body.Gravity = new Vector3(0, -27, 0).ToNumerics();

            underWaterSound = FmodEventInstance.Create("snapshot:/UnderWater");

            body.SetPosition(Position.ToPhysics());

            Input.MouseDelta = new Vector2();

            body.CcdMotionThreshold = 0.000001f;
            body.CcdSweptSphereRadius = 0.3f;
            body.Friction = 1f;

            bodies.Add(body);

            stepSoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            stepSound = FmodEventInstance.Create("event:/Character/Player Footsteps");
            stepSoundPlayer.SetSound(stepSound);
            stepSoundPlayer.Volume = 0.1f;

            stepSound.SetParameter("Surface", 1);

            //weapons.Add(new WeaponData { weaponType = typeof(weapon_hammer), ammo = 1 });
            //weapons.Add(new WeaponData { weaponType = typeof(weapon_shotgunNew), ammo = 50 });
            //weapons.Add(new WeaponData { weaponType = typeof(weapon_pistol_double), ammo = 50 });
            SwitchToSlot(0, true);

            interpolatedPosition = Position;

            PlayerFlashLight = Level.GetCurrent().AddEntity(new PointLight()) as PointLight;
            PlayerFlashLight.CastShadows = true;


            PlayerFlashLight.enabled = true;

            PlayerFlashLight.SetAngle(25);
            PlayerFlashLight.radius = 30;
            PlayerFlashLight.Intensity = 1;

            PlayerFlashLight.Start();


            if(Level.GetCurrent().FindEntityByName("PlayerGlobal") == null)
                Level.GetCurrent().AddEntity(new Entities.Player.PlayerGlobal());

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

            UpdateMovement();

            UpdateCamera();

            if (Input.GetAction("test").Holding())
                PlayerBodyAnimator.FireAction();

            if (Input.GetAction("jump").Holding())
                Jump();

            if (Input.GetAction("slot0").Pressed())
                SwitchToSlot(-1);

            if (Input.GetAction("slot1").Pressed())
                SwitchToSlot(0);

            if (Input.GetAction("slot2").Pressed())
                SwitchToSlot(1);

            if (Input.GetAction("slot3").Pressed())
                SwitchToSlot(2);

            if (Input.GetAction("lastSlot").Pressed())
                SwitchToSlot(lastSlot);

            if (Input.GetAction("view").Pressed())
                thirdPerson = ! thirdPerson;

            if(Input.GetAction("test2").Pressed())
                flashlightEnabled = ! flashlightEnabled;

        }

        void EnteredWater()
        {
            PostProcessStep.StepsBefore.Add(waterPP);
            underWaterSound.StartEvent();
        }

        void ExitedWater()
        {
            PostProcessStep.StepsBefore.Remove(waterPP);
            underWaterSound.Stop();
        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            CheckGround();

            InterpolatePos();


            Camera.velocity = body.LinearVelocity;

            PlayerBodyAnimator.Update();

            PlayerBodyAnimator.MovementSpeed = ((Vector3)body.LinearVelocity).XZ().Length();

            float Dx = Vector3.Dot(((Vector3)body.LinearVelocity).XZ().Normalized(), Camera.rotation.GetRightVector());
            float Dy = Vector3.Dot(((Vector3)body.LinearVelocity).XZ().Normalized(), Camera.rotation.GetForwardVector().XZ().Normalized());

            Vector2 dir = new Vector2(Dx, Dy);

            PlayerFlashLight.enabled = flashlightEnabled;

            PlayerBodyAnimator.MovementDirection = dir;
            

            MathHelper.Transform hide = new MathHelper.Transform();
            hide.Scale = Vector3.Zero;
            MathHelper.Transform show = new MathHelper.Transform();

            Matrix showR = new Matrix();
            Matrix showL = new Matrix();

            if (currentWeapon == null)
            {
                showR = showL = show.ToMatrix();
            }else
            {
                if(currentWeapon.ShowHandR)
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

            bodyMesh.SetBoneMeshTransformModification("upperarm_r",showR);
            bodyMesh.SetBoneMeshTransformModification("upperarm_l", showL);
            bodyMesh.SetBoneMeshTransformModification("head", hide.ToMatrix());

            if(thirdPerson)
            {
                bodyMesh.SetBoneMeshTransformModification("upperarm_r", show.ToMatrix());
                bodyMesh.SetBoneMeshTransformModification("upperarm_l", show.ToMatrix());
                bodyMesh.SetBoneMeshTransformModification("head", show.ToMatrix());
            }

        }

        public override void VisualUpdate()
        {
            base.VisualUpdate();

            var pose = PlayerBodyAnimator.GetResultPose();

            if (currentWeapon != null && thirdPerson)
            {
                pose = currentWeapon.ApplyWeaponAnimation(pose);

            }

            //bodyMesh.SetWorldPositionOverride("clavicle_l", new MathHelper.Transform { Position = new Vector3(0,3,0)}.ToMatrix());

            bodyMesh.PastePoseLocal(pose);

        }

        void InterpolatePos()
        {
            Vector3 oldPos = interpolatedPosition;

            Vector3 newPos = Position;

            Vector3 previousPosition = interpolatedPosition; // This should be updated each physics tick
            Vector3 currentPosition = Position; // This is updated during the physics update

            float fixedDeltaTime = Math.Max(1 / 30f, Time.DeltaTime);

            // Interpolate the position based on the elapsed time in the current frame
            float interpolationFactor = Time.DeltaTime / fixedDeltaTime;
            interpolatedPosition = Vector3.Lerp(previousPosition, currentPosition, interpolationFactor);


        }

        void UpdateCamera()
        {
            if (GameMain.SkipFrames == 0)
                Camera.rotation += new Vector3(Input.MouseDelta.Y, -Input.MouseDelta.X, 10) / 2f;

            Camera.rotation = new Vector3(Math.Clamp(Camera.rotation.X, -89, 89), Camera.rotation.Y, 0);


            Camera.position = interpolatedPosition + new Vector3(0,0.7f,0) + Camera.rotation.GetForwardVector().XZ().Normalized()*0.1f;

            bob = Vector3.Zero;

            bob += Camera.rotation.GetForwardVector() * (float)Math.Sin(bobProgress * 1 * bobSpeed * 1) * -0.5f;
            //bob += Camera.rotation.GetUpVector() * (float)(Math.Abs(Math.Sin(bobProgress * bobSpeed * 1))) * 0.2f;
        }

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
                        stepSoundPlayer.Play(true);
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

            float radius = 0.4f;

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

            base.Destroy();
        }

        bool CheckGroundAtOffset(Vector3 offset)
        {
            var hit = Physics.LineTrace(Position.ToNumerics() + offset.ToNumerics(), (Position - new Vector3(0, 1.05f, 0) + offset).ToNumerics(), new List<CollisionObject>() { body }, bodyType: BodyType.GroupCollisionTest);

            if(hit.HasHit == false)
                return false;

            return hit.HitNormalWorld.Y>0.7;
        }

        Delay stepDelay = new Delay();

        void TryStep(Vector3 dir)
        {

            if (stepDelay.Wait()) return;

            Vector3 pos = Position + dir/1.2f;

            if (pos == Vector3.Zero)
                return;

            var hit = Physics.LineTrace(pos.ToPhysics(), (pos - new Vector3(0, 0.73f, 0)).ToPhysics(), new List<CollisionObject>() { body }, BodyType.World & BodyType.MainBody);

            if (hit.HasHit == false)
                return;

            DrawDebug.Line(hit.HitPointWorld, hit.HitPointWorld + hit.HitNormalWorld, Vector3.UnitX);
            if (hit.HitNormalWorld.Y < 0.95)
                return;

            

            Vector3 hitPoint = hit.HitPointWorld;

            if (hitPoint == Vector3.Zero)
                return;



            if (hitPoint.Y > Position.Y - 1 + 1)
                return;

            if (Vector3.Distance(hitPoint, Position) > 1.4)
                return;

            hit = Physics.LineTrace(Position.ToPhysics(), Vector3.Lerp(Position, hitPoint, 1.1f).ToPhysics() + Vector3.UnitY.ToPhysics() * 0.2f, new List<CollisionObject>() { body }, body.GetCollisionMask());

            if (hit.HasHit)
            {
                //DrawDebug.Sphere(0.1f, hit.HitPointWorld, Vector3.Zero, 3);
                return;
            }


            hitPoint.Y += 1.5f;

            DrawDebug.Sphere(0.5f, hitPoint, Vector3.UnitY);

            Vector3 lerpPose = Vector3.Lerp(Position, hitPoint, 0.4f);

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
        }

        void FirstPersonFullBodyCameraUpdate()
        {
            MathHelper.Transform t = bodyMesh.GetBoneMatrix("head").DecomposeMatrix();

            Camera.position = t.Position + Camera.rotation.GetForwardVector() * 0.2f;
            Camera.position += Camera.rotation.GetUpVector() * 0.15f;
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

            var hit = Physics.SphereTrace(startPos.ToPhysics(), targetCameraPos.ToPhysics(), radius: 0.3f, ignoreList: new List<CollisionObject> { body });
            if (hit.HasHit)
                Camera.position = hit.HitPointWorld + hit.HitNormalWorld * 0.3f;
            else
                Camera.position = targetCameraPos;
        }

        public override void LateUpdate()
        {

            if (FreeCamera.active)
                return;

            UpdatePlayerInput();

            bodyMesh.Position = interpolatedPosition - Camera.rotation.GetForwardVector().XZ().Normalized() * 0.25f - new Vector3(0, 0.93f, 0);
            bodyMesh.Rotation = new Vector3(0, Camera.rotation.Y, 0);


            if(thirdPerson)
            {
                ThirdPersonCameraUpdate();
                //FirstPersonFullBodyCameraUpdate();
            }
            else
            {
                FirstPersonCameraUpdate();
            }

            if (currentWeapon is not null)
            {
                currentWeapon.Position = Camera.position + bob * 0.05f * currentWeapon.BobScale + Camera.rotation.GetForwardVector() * Camera.rotation.X / 2000f;
                currentWeapon.Rotation = Camera.rotation + new Vector3(0, 0, (float)Math.Sin(bobProgress * -1 * bobSpeed) * -1.5f) * currentWeapon.BobScale;
            }

            UpdatePlayerLight();

            
            if(Input.GetAction("test").Pressed())
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
                + Vector3.Up * 0.1f;
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

        void SwitchToSlot(int slot, bool forceChange = false)
        {
            if(forceChange == false)
                if (slot == currentSlot) return;

            if (weapons.Count > slot && slot >= 0)
            {

                lastSlot = currentSlot;
                currentSlot = slot;

                SwitchWeapon(weapons[slot]);
            }
            else
            {
                currentSlot = slot;
                SwitchWeapon(null);
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
                currentWeapon = Level.GetCurrent().AddEntity(Weapon.CreateFromData(data, this)) as Weapon;
            }

        }

        public void AddWeapon(WeaponData weaponData)
        {
            weapons.Add(weaponData);
                SwitchToSlot(weapons.Count - 1,true);
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

            var hit = Physics.LineTrace(waterCheckPos + Vector3.UnitY * 100, waterCheckPos, bodyType: BodyType.Liquid);

            underWater = hit.HasHit;

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
            return thirdPerson == false;
        }

        public SkeletalMesh GetSkeletalMesh()
        {
            return bodyMesh;
        }

    }
}
