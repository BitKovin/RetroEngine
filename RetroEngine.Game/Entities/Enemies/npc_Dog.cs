using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using DotRecast.Detour.Crowd;
using Microsoft.Xna.Framework;
using RetroEngine.Audio;
using RetroEngine.Entities;
using RetroEngine.Game.Entities.Weapons;
using RetroEngine.Map;
using RetroEngine.NavigationSystem;
using RetroEngine.ParticleSystem;
using RetroEngine.PhysicsSystem;
using RetroEngine.SaveSystem;
using RetroEngine.Skeletal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static RetroEngine.MathHelper;

namespace RetroEngine.Game.Entities.Enemies
{
    [LevelObject("npc_dog")]
    public class npc_dog : Entity
    {
        [JsonInclude]
        public Vector3 MoveDirection = Vector3.Zero;
        [JsonInclude]
        public Vector3 DesiredMoveDirection = Vector3.Zero;

        SkeletalMesh mesh = new SkeletalMesh();

        [JsonInclude]
        public float speed = 5f;

        float maxSpeed = 8.5f;

        Delay updateDelay = new Delay();

        static List<NPCBase> npcList = new List<NPCBase>();
        static List<NPCBase> currentUpdateNPCs = new List<NPCBase>();
        static int currentUpdateIndex = 0;

        [JsonInclude]
        public Vector3 targetLocation = Vector3.Zero;

        RigidBody body;

        StaticMesh sm = new StaticMesh();

        static List<Vector3> directionsLUT = new List<Vector3>();

        PathfindingQuery pathfindingQuery = new PathfindingQuery();

        SoundPlayer deathSoundPlayer;

        protected float AnimationInterpolationDistance = 10;
        protected float AnimationComplexDistance = 30;
        protected float AnimationDistance = 60;
        protected float ShadowDistance = 40;
        protected bool AnimationAlwaysUpdateTime = true;

        [JsonInclude]
        public bool dead = false;

        Entity target;


        bool onGround = true;

        [JsonInclude]
        public bool attacking = false;

        [JsonInclude]
        public Delay attackCooldown = new Delay();

        [JsonInclude]
        public bool stunned = false;

        [JsonInclude]
        public float rootMotionScale = 1;

        SoundPlayer SoundPlayer;

        FmodEventInstance soundStun;
        FmodEventInstance soundAttack;
        FmodEventInstance soundAttackStart;
        FmodEventInstance soundDeath;

        public npc_dog()
        {
            SaveGame = true;

            mesh.OnAnimationEvent += Mesh_OnAnimationEvent;

            Health = 40;

            attackCooldown.AddDelay(1);

        }

        private void Mesh_OnAnimationEvent(AnimationEvent animationEvent)
        {
            if(animationEvent.Name == "attackStart")
            {

            }
            else if(animationEvent.Name == "attackEnd")
            {
                attacking = false;
                mesh.PlayAnimation("run");

            }
            else if(animationEvent.Name == "stunEnd")
            {
                stunned = false;
                mesh.PlayAnimation("run");
                attacking = false;
                speed = 5;
            }
            else if(animationEvent.Name == "lookAtTarget")
            {
                mesh.Rotation = new Vector3(0, MathHelper.FindLookAtRotation(Position, targetLocation).Y, 0);
            }
                
            
        }

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 2f, 0.7f, 10, CollisionFlags.CharacterObject);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            body.CcdMotionThreshold = 0.5f;
            body.CcdSweptSphereRadius = 0.7f;

            bodies.Add(body);

            target = Level.GetCurrent().FindEntityByName("player");


            //npcList.Add(this);

            pathfindingQuery.OnPathFound += PathfindingQuery_OnPathFound;


            body.ActivationState = ActivationState.DisableDeactivation;

            InitDirectionsLUT();

            mesh.Rotation = Rotation;

            MoveDirection = Rotation.GetForwardVector();

            SoundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;
            SoundPlayer.Position = Position;

            

        }

        void InitDirectionsLUT()
        {

            if (directionsLUT.Count > 0) return;

            directionsLUT.Add(new Vector3(0, 0, 0));

            directionsLUT.Add(new Vector3(1, 0, 0));
            directionsLUT.Add(new Vector3(0, 0, 1));
            directionsLUT.Add(new Vector3(-1, 0, 0));
            directionsLUT.Add(new Vector3(0, 0, -1));

            float diagonalL = 0.7777f;

            directionsLUT.Add(new Vector3(diagonalL, 0, diagonalL));
            directionsLUT.Add(new Vector3(-diagonalL, 0, diagonalL));
            directionsLUT.Add(new Vector3(diagonalL, 0, -diagonalL));
            directionsLUT.Add(new Vector3(-diagonalL, 0, -diagonalL));
        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            new HeartPickup().LoadAssetsIfNeeded();

            mesh.LoadFromFile("models/enemies/dog.FBX");

            ParticleSystemEnt.Preload("hitBlood");

            if(dead == false)
                mesh.PlayAnimation("run");

            mesh.ReloadHitboxes(this);

            mesh.CastGeometricShadow = true;

            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");


            sm.LoadFromFile("models/cube.obj");
            sm.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            meshes.Add(mesh);
            mesh.CastShadows = true;
            //meshes.Add(sm);

            mesh.PreloadTextures();

            sm.Transperent = false;

            sm.Scale = new Vector3(0.7f);


            deathSoundPlayer = (SoundPlayer)Level.GetCurrent().AddEntity(new SoundPlayer());
            deathSoundPlayer.SetSound(AssetRegistry.LoadSoundFmodFromFile("sounds/mew2.wav"));
            deathSoundPlayer.Volume = 1f;

            soundStun = FmodEventInstance.Create("event:/NPC/Dog/DogStun");
            soundAttack = FmodEventInstance.Create("event:/NPC/Dog/DogAttack");
            soundAttackStart = FmodEventInstance.Create("event:/NPC/Dog/DogAttackStart");
            soundDeath = FmodEventInstance.Create("event:/NPC/Dog/DogDeath");

        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            new Vector3(0, data.GetPropertyFloat("angle") + 90, 0);

            mesh.Rotation = Rotation;

            MoveDirection = Rotation.GetForwardVector();

        }


        void CheckGround()
        {

            onGround = false;

            var hit = Physics.LineTrace(Position, Position - Vector3.UnitY * 1.3f, new List<CollisionObject>{body}, BodyType.GroupCollisionTest);

            if(hit.HasHit)
            {
                RigidBody hitBody = hit.CollisionObject as RigidBody;

                if (hitBody == null) return;

                if (hitBody.GetBodyType() == BodyType.CharacterCapsule)
                {

                    body.LinearVelocity = hit.HitNormalWorld*8;
                    return;

                }

                onGround = true;

            }


        }

        public override void OnPointDamage(float damage, Vector3 point, Vector3 direction, string hitBone = "", Entity causer = null, Entity weapon = null)
        {

            if (attacking)
            {
                if (weapon != null)
                {

                    Weapon weaponPlayer = weapon as Weapon;

                    if (weaponPlayer != null)
                    {
                        if (hitedEntities.Contains(target) == false)
                            if (weaponPlayer.IsMelee)
                            {
                                Time.AddTimeScaleEffect(new TimeScaleEffect(0.1f, 0.05f));
                                weaponPlayer.Blocked();
                            }

                    }
                }
            }

            base.OnPointDamage(damage, point, direction, hitBone, causer, weapon);

            damage = MathF.Max(15, damage);

            GlobalParticleSystem.EmitAt("hitBlood", point, MathHelper.FindLookAtRotation(Vector3.Zero, -direction), new Vector3(0, 0, damage / 10f));

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            Stun((weapon.Position - Position).Normalized());


            if (Health <= 0)
            {
                Death();
            }
        }

        void Stun(Vector3 attackDirection)
        {

            if(stunned) return;

            attacking = false;

            stunned = true;
            mesh.PlayAnimation("stun", false, 0.2f, true);
            mesh.Rotation = MathHelper.FindLookAtRotation(Vector3.Zero, attackDirection.XZ());
            MoveDirection = attackDirection.XZ();

            SoundPlayer.SetSound(soundStun);
            SoundPlayer.Play(true);

        }

        [JsonInclude]
        public Delay meshStopUpdateDelay = new Delay();

        void Death()
        {

            meshStopUpdateDelay.AddDelay(3);

            mesh.PlayAnimation("death", false);

            mesh.MaxRenderDistance = 40;

            Physics.Remove(body);

            Position = mesh.Position;

            //body.Friction = 0.5f;

            mesh.ClearRagdollBodies();

            if(dead) return;

            GetOwner()?.OnAction("despawned");

            SoundPlayer.SetSound(soundDeath);
            SoundPlayer.Play();

            var hit = Physics.SphereTrace(Position + Vector3.UnitY * 0.2f, Position - Vector3.Up * 100, 0.3f, bodyType: BodyType.World);

            Vector3 spawnPos = Position;

            if(hit.HasHit)
                spawnPos = hit.HitShapeLocation + Vector3.UnitY*0.2f;

            Entity healthPickup = new HeartPickup();
            healthPickup.Position = spawnPos;
            healthPickup.Start();
            Level.GetCurrent().AddEntity(healthPickup);

            dead = true;

        }


        public override void Update()
        {
            base.Update();


            float distance = Vector3.Distance(Position, targetLocation);

            Vector3 toTarget = (target.Position - Position).XZ().Normalized();

            if (distance < 6f && attacking == false && Vector3.Dot(mesh.Rotation.GetForwardVector(), toTarget) > 0.95f && attackCooldown.Wait() == false && stunned == false)
            {

                rootMotionScale = Lerp(0.1f, 1f, Saturate(distance - 1 / 4f));

                PerformAttack(toTarget);

            }

        }

        List<Entity> hitedEntities = new List<Entity>();

        [JsonInclude]
        public bool reachedFloor = false;
        public override void AsyncUpdate()
        {

            SoundPlayer.Position = Position;

            if (dead)
            {

                if (reachedFloor) return;

                Vector3 prevPos = Position + Vector3.UnitY / 5;

                Position -= Vector3.UnitY * Time.DeltaTime * 2;

                var hit = Physics.LineTraceForStatic(prevPos.ToPhysics(), Position.ToPhysics());

                if (hit.HasHit)
                {
                    Position = hit.HitPointWorld;
                    reachedFloor = true;
                }
                mesh.Position = Position;

                return;
            }



            if (target != null)
                targetLocation = target.Position;

            //MoveDirection = crowdAgent.vel.FromRc().XZ().FastNormalize();


            //crowdAgent.npos = (Position + Vector3.UnitY * heightDif).ToRc();

            if (loadedAssets == false) return;


            float desiredMaxSpeed = (Vector3.Distance(targetLocation, Position) < 2) ? 2 : maxSpeed;

            if(speed < desiredMaxSpeed)
            speed += Time.DeltaTime * (attackCooldown.Wait() ? 5: 7);


            


            if(attackCooldown.Wait() == false)
                attacking = false;



            speed = Math.Clamp(speed, 0, desiredMaxSpeed);


            //CheckGround();


            if (onGround && attacking == false && stunned == false)
            {

                body.LinearVelocity = new System.Numerics.Vector3(MoveDirection.X * speed, body.LinearVelocity.Y, MoveDirection.Z * speed);

                MoveDirection = Vector3.Lerp(MoveDirection, DesiredMoveDirection, Time.DeltaTime  * MathHelper.Lerp(1f,6, MathF.Pow(Vector3.Dot(MoveDirection, DesiredMoveDirection)/2 + 0.5f,1.5f)));

            }

            var rootTrans = mesh.PullRootMotion();

            Vector3 motion = rootTrans.Position * rootMotionScale;

            motion = Vector3.Transform(motion, mesh.Rotation.GetRotationMatrix());

            if (attacking || stunned)
            {
                //body.Translate(MoveDirection.ToPhysics() * Time.DeltaTime * 8);


                body.LinearVelocity = new System.Numerics.Vector3(0, body.LinearVelocity.Y, 0);

                body.TranslateSweep(motion.ToPhysics(), 0.4f);
                Position = body.WorldTransform.Translation;


                mesh.Rotation += rootTrans.Rotation;

                MoveDirection = mesh.Rotation.GetForwardVector();

                //Console.WriteLine(rootTrans.Rotation);

                if(attacking && target != null)
                {
                    ProcessEnemyHit();
                }

                speed = 2;

            }

            mesh.Position = Position - new Vector3(0, 1f, 0);

            if(stunned == false && attacking == false)
                mesh.Rotation = new Vector3(0, MathHelper.FindLookAtRotation(Vector3.Zero, MoveDirection).Y, 0);

            mesh.UpdateHitboxes();

            //crowdAgent.SetAgentTargetPosition(targetLocation);

            //return;
            if (updateDelay.Wait()) return;
            RequestNewTargetLocation();
            updateDelay.AddDelay(Math.Min(Vector3.Distance(Position, targetLocation) / 30, 0.1f) + Random.Shared.NextSingle() / 8f);

            //if(attacking == false)
            //TryStep(MoveDirection / 1.5f);

                
        }

        void ProcessEnemyHit()
        {

            if(hitedEntities.Contains(target)) return;

            Vector3 attackPos = mesh.Position + Vector3.UnitY;

            Vector3 attackDir = targetLocation - attackPos;
            attackDir = attackDir.Normalized();

            if(Vector3.Dot(new Vector3(attackDir.X, attackDir.Y/4f, attackDir.Z).Normalized(), mesh.Rotation.GetForwardVector())<0.6f)
            {
                return;
            }

            var hit = Physics.SphereTrace(attackPos, attackPos + attackDir * 0.6f * new Vector3(1,3,1), 0.1f, bodies, BodyType.CharacterCapsule);

            if(hit.HasHit&& hit.entity == target)
            {
                target.OnPointDamage(10, hit.HitShapeLocation, attackDir,"", this, this);

                SoundPlayer.SetSound(soundAttack);
                SoundPlayer.Play();

                hitedEntities.Add(target);

            }

        }


        void PerformAttack(Vector3 toTarget)
        {

            List<RigidBody> ignore = new List<RigidBody>();

            ignore.Add(body);
            ignore.AddRange(target.bodies);

            Physics.SphereTrace(Position, targetLocation, 0.1f, ignore, BodyType.GroupCollisionTest);

            attacking = true;
            mesh.PlayAnimation("attack", false, rootMotion: true);

            MoveDirection = toTarget.XZ().Normalized();

            attackCooldown.AddDelay(2);
            hitedEntities.Clear();

            SoundPlayer.SetSound(soundAttackStart);
            SoundPlayer.Play();

        }

        public override void VisualUpdate()
        {
            base.VisualUpdate();

            if (dead && meshStopUpdateDelay.Wait() == false)
                return;

            mesh.Update(Time.DeltaTime);

            if (dead) return;

            float cameraDistance = Vector3.Distance(Position, Camera.position);



        }

        public override void LateUpdate()
        {
            base.LateUpdate();

        }

        public override void Destroy()
        {
            base.Destroy();



        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

        }

        void UpdateMovementDirection()
        {
            body.Activate();

            List<Vector3> locations = new List<Vector3>();

            Vector3 moveLocation = new Vector3();

            foreach (Vector3 dir in directionsLUT)
            {
                List<Vector3> path = Navigation.FindPath(Position, targetLocation + dir * 0.5f);

                if (path.Count > 0)
                    locations.Add(path[0]);
            }
            locations = locations.OrderBy(x => Vector3.Distance(targetLocation, x)).ToList();

            if (locations.Count > 0)
            {
                moveLocation = locations[0];
            }

            Vector3 newMoveDirection = moveLocation - Position;



            DesiredMoveDirection = newMoveDirection;

            DesiredMoveDirection.Normalize();
        }

        void RequestNewTargetLocation()
        {
            if (pathfindingQuery.Processing == false)
                pathfindingQuery.Start(Position, targetLocation);
        }

        private void PathfindingQuery_OnPathFound(List<Vector3> points)
        {
            Vector3 moveLocation = new Vector3();


            if (points.Count > 0)
            {
                moveLocation = points[0];
            }
            else
            {
                moveLocation = targetLocation;
            }

            sm.Position = moveLocation;

            Vector3 newMoveDirection = moveLocation - Position;

            DesiredMoveDirection = newMoveDirection.XZ().Normalized();

        }

        static void UpdateNPCList()
        {


            currentUpdateNPCs.Clear();

            for (int i = 0; i < 100; i++)
            {
                currentUpdateIndex++;

                if (npcList.Count > 0)
                    while (currentUpdateIndex >= npcList.Count)
                    {
                        currentUpdateIndex -= npcList.Count;
                    }
                currentUpdateNPCs.Add(npcList[currentUpdateIndex]);
            }
        }

        Delay stepDelay = new Delay();

        void TryStep(Vector3 dir)
        {

            if (stepDelay.Wait()) return;

            Vector3 pos = Position + dir / 1.2f;

            if (pos == Vector3.Zero)
                return;

            var hit = Physics.LineTrace(pos.ToPhysics(), (pos - new Vector3(0, 0.23f, 0)).ToPhysics(), new List<CollisionObject>() { body }, BodyType.World);

            if (hit.HasHit == false)
                return;

            //DrawDebug.Line(hit.HitPointWorld, hit.HitPointWorld + hit.HitNormalWorld, Vector3.UnitX);
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

        [JsonInclude]
        public SkeletalMesh.AnimationState animationState;

        protected override EntitySaveData SaveData(EntitySaveData baseData)
        {

            Rotation = mesh.Rotation;
            animationState = mesh.GetAnimationState();

            return base.SaveData(baseData);

        }

        public override void LoadData(EntitySaveData Data)
        {
            base.LoadData(Data);


            body.SetPosition(Position);

            mesh.Rotation = Rotation;
            mesh.Position = Position;

            if (dead)
            {
                Death();
                mesh.Update(10);
            }

            mesh.SetAnimationState(animationState);

        }

        public static void ResetStaticData()
        {
            npcList.Clear();
            currentUpdateNPCs.Clear();
            currentUpdateIndex = 0;
        }

    }
}
