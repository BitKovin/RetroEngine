using BulletSharp;
using BulletSharp.SoftBody;
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
    [LevelObject("npc_humanAxe")]
    public class npc_HumanAxe : Entity
    {
        [JsonInclude]
        public Vector3 MoveDirection = Vector3.Zero;
        [JsonInclude]
        public Vector3 DesiredMoveDirection = Vector3.Zero;

        SkeletalMesh mesh = new SkeletalMesh();

        [JsonInclude]
        public float speed = 5f;

        float maxSpeed = 7.5f;

        Delay updateDelay = new Delay();

        static List<NPCBase> npcList = new List<NPCBase>();
        static List<NPCBase> currentUpdateNPCs = new List<NPCBase>();
        static int currentUpdateIndex = 0;

        [JsonInclude]
        public Vector3 targetLocation = Vector3.Zero;

        RigidBody body;


        static List<Vector3> directionsLUT = new List<Vector3>();

        PathfindingQuery pathfindingQuery = new PathfindingQuery();


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
        FmodEventInstance soundDamage;
        FmodEventInstance soundAttack;
        FmodEventInstance soundAttackStart;
        FmodEventInstance soundDeath;

        CharacterAnimator animator = new CharacterAnimator();

        DtCrowdAgent crowdAgent;

        public npc_HumanAxe()
        {
            SaveGame = true;

            //mesh.OnAnimationEvent += Mesh_OnAnimationEvent;
            animator.OnAnimationEvent += Mesh_OnAnimationEvent;

            Health = 90;

            attackCooldown.AddDelay(1);


        }

        private void Mesh_OnAnimationEvent(AnimationEvent animationEvent)
        {


            if(animationEvent.Name == "attack")
            {
                ProcessEnemyHit();
            }
            else if(animationEvent.Name == "attackEnd")
            {
                attacking = false;

            }
            else if(animationEvent.Name == "stunEnd")
            {
                stunned = false;
                attacking = false;
                speed = 1;
            }
            else if(animationEvent.Name == "lookAtTarget")
            {
                mesh.Rotation = new Vector3(0, MathHelper.FindLookAtRotation(Position, targetLocation).Y, 0);
            }
                
            
        }


        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 2f, 0.5f, 1, CollisionFlags.CharacterObject);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            body.CcdMotionThreshold = 0.001f;
            body.CcdSweptSphereRadius = 0.5f;

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

            mesh.LoadFromFile("models/enemies/enemy1.FBX");

            animator.LoadAssets();

            ParticleSystemEnt.Preload("hitBlood");

            mesh.ReloadHitboxes(this);

            mesh.CastGeometricShadow = true;

            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");


            meshes.Add(mesh);
            mesh.CastShadows = true;
            //meshes.Add(sm);

            mesh.PreloadTextures();



            soundStun = FmodEventInstance.Create("event:/NPC/Enemy1/Enemy1Stun");
            soundAttack = FmodEventInstance.Create("event:/NPC/Enemy1/Enemy1Attack");
            soundAttackStart = FmodEventInstance.Create("event:/NPC/Enemy1/Enemy1AttackStart");
            soundDeath = FmodEventInstance.Create("event:/NPC/Enemy1/Enemy1Death");
            soundDamage = FmodEventInstance.Create("event:/NPC/Enemy1/Enemy1Damage");

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

            if (hitBone == "head")
                damage *= 1.5f;

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

                                Stun((weapon.Position - Position).Normalized());

                            }

                    }
                }
            }

            base.OnPointDamage(damage, point, direction,hitBone, causer, weapon);

            damage = MathF.Max(15, damage);

            SoundPlayer.PlayAtLocation(FmodEventInstance.Create("event:/NPC/General/FleshHit"), point, Volume: damage / 20f);

            GlobalParticleSystem.EmitAt("hitBlood", point, MathHelper.FindLookAtRotation(Vector3.Zero, -direction), new Vector3(0, 0, damage / 10f));
            GlobalParticleSystem.EmitAt("hitBlood", point, MathHelper.FindLookAtRotation(Vector3.Zero, direction), new Vector3(0, 0, damage / 20f));

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            SoundPlayer.SetSound(soundDamage);
            SoundPlayer.Play(true);

            speed = float.Clamp(speed - damage / 3, 0, 10);

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
            
            mesh.Rotation = MathHelper.FindLookAtRotation(Vector3.Zero, attackDirection.XZ());
            MoveDirection = attackDirection.XZ();

            animator.stunAnimation.Play();
            animator.attackAnimation.Stop();

            SoundPlayer.SetSound(soundStun);
            SoundPlayer.Play(true);

        }

        void Death()
        {

            CrowdSystem.RemoveAgent(crowdAgent);

            mesh.PlayAnimation("death", false);

            mesh.MaxRenderDistance = 50;

            meshStopUpdateDelay.AddDelay(3);

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

            if (distance < 2f && attacking == false && Vector3.Dot(mesh.Rotation.GetForwardVector(), toTarget) > 0.7f && attackCooldown.Wait() == false && stunned == false)
            {

                rootMotionScale = Lerp(0.1f, 1f, Saturate(distance - 1 / 4f));

                PerformAttack(toTarget);

            }

        }

        MathHelper.Transform rootTrans = new MathHelper.Transform();

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

            if (crowdAgent != null)
            {
                MoveDirection = crowdAgent.vel.FromRc().XZ().FastNormalize();
                crowdAgent.npos = (Position + Vector3.UnitY * 1).ToRc();
            }

            if (loadedAssets == false) return;


            float desiredMaxSpeed = (Vector3.Distance(targetLocation, Position) < 1.3f) ? 2 : maxSpeed;

            if (speed < desiredMaxSpeed)
                speed += Time.DeltaTime * 10;

            if (speed > desiredMaxSpeed)
                speed = desiredMaxSpeed;


            speed = Math.Clamp(speed, 0, desiredMaxSpeed);


            //CheckGround();


            if (onGround && stunned == false)
            {

                body.LinearVelocity = new System.Numerics.Vector3(MoveDirection.X * speed, body.LinearVelocity.Y, MoveDirection.Z * speed);

                MoveDirection = Vector3.Lerp(MoveDirection, DesiredMoveDirection, Time.DeltaTime * MathHelper.Lerp(1f, 6, MathF.Pow(Vector3.Dot(MoveDirection, DesiredMoveDirection) / 2 + 0.5f, 1.5f)));

                if(crowdAgent != null)
                {
                    body.LinearVelocity = new System.Numerics.Vector3(crowdAgent.vel.X, body.LinearVelocity.Y, crowdAgent.vel.Z);
                }

            }

            Vector3 motion = rootTrans.Position * rootMotionScale;

            motion = Vector3.Transform(motion, mesh.Rotation.GetRotationMatrix());

            if (stunned)
            {
                //body.Translate(MoveDirection.ToPhysics() * Time.DeltaTime * 8);


                body.LinearVelocity = new System.Numerics.Vector3(0, body.LinearVelocity.Y, 0);

                body.TranslateSweep(motion.ToPhysics(), 0.4f);
                Position = body.WorldTransform.Translation;


                mesh.Rotation += rootTrans.Rotation;

                MoveDirection = mesh.Rotation.GetForwardVector();

                //Console.WriteLine(rootTrans.Rotation);


                speed = 2;

            }

            mesh.Position = Position - new Vector3(0, 1f, 0);

            if (stunned == false)
                mesh.Rotation = new Vector3(0, MathHelper.FindLookAtRotation(Vector3.Zero, MoveDirection).Y, 0);

            if (crowdAgent != null)
                crowdAgent.SetAgentTargetPosition(targetLocation);

            //return;
            if (updateDelay.Wait()) return;
            updateDelay.AddDelay(Math.Min(Vector3.Distance(Position, targetLocation) / 30, 0.1f) + Random.Shared.NextSingle() / 8f);



            if (crowdAgent == null && Vector3.Distance(Position, targetLocation) < 20 && false)
            {
                crowdAgent = CrowdSystem.CreateAgent(this, Position);
                crowdAgent.option.separationWeight = 1;
                crowdAgent.option.pathOptimizationRange = 4;
            }

            if(crowdAgent == null)
                RequestNewTargetLocation();

            //if(attacking == false)
            //TryStep(MoveDirection / 1.5f);


        }

        void ProcessEnemyHit()
        {

            if(hitedEntities.Contains(target)) return;

            Vector3 attackPos = mesh.Position + Vector3.UnitY;

            Vector3 attackDir = mesh.Rotation.GetForwardVector();
            attackDir = attackDir.Normalized();

            var hit = Physics.SphereTrace(attackPos, attackPos + attackDir * 1.2f * new Vector3(1,3,1), 0.6f, bodies, BodyType.CharacterCapsule);

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

            if (dead) return;

            attacking = true;

            animator.attackAnimation.Play();

            //mesh.PlayAnimation("attack", false,0.4f, rootMotion: true);


            MoveDirection = toTarget.XZ().Normalized();

            attackCooldown.AddDelay(1);
            hitedEntities.Clear();

            SoundPlayer.SetSound(soundAttackStart);
            SoundPlayer.Play();

        }

        [JsonInclude]
        public Delay meshStopUpdateDelay = new Delay();

        public override void VisualUpdate()
        {
            base.VisualUpdate();
            if (dead)
            {
                if(meshStopUpdateDelay.Wait())
                mesh.Update(Time.DeltaTime * 1.2f);
                return;
            }

            animator.MovementSpeed = ((Vector3)body.LinearVelocity).XZ().Length();

            animator.Update();

            var pose = animator.GetResultPose();

            rootTrans = pose.RootMotion;

            mesh.PastePoseLocal(pose);
            mesh.UpdateHitboxes();


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

            CrowdSystem.RemoveAgent(crowdAgent);

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
        public Animator.AnimatorSaveState animationState;

        protected override EntitySaveData SaveData(EntitySaveData baseData)
        {

            animationState = animator.SaveState();

            Rotation = mesh.Rotation;

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

            animator.LoadState(animationState);

        }

        public static void ResetStaticData()
        {
            npcList.Clear();
            currentUpdateNPCs.Clear();
            currentUpdateIndex = 0;
        }

        class CharacterAnimator : Animator
        {

            Animation idleAnimation;
            Animation runFAnimation;

            public ActionAnimation attackAnimation;
            public ActionAnimation stunAnimation;

            public float MovementSpeed = 0;

            SkeletalMesh proxy = new SkeletalMesh();

            protected override void Load()
            {
                base.Load();

                idleAnimation = AddAnimation("models/enemies/enemy1.fbx", true, "idle", interpolation: false);
                runFAnimation = AddAnimation("models/enemies/enemy1.fbx", true, "run", interpolation: true);

                attackAnimation = AddActionAnimation("models/enemies/enemy1.fbx", "attack",0.3f);
                stunAnimation = AddActionAnimation("models/enemies/enemy1.fbx", "stun", 0.3f, 0.4f);


                proxy.LoadFromFile("models/enemies/enemy1.fbx");

            }

            protected override AnimationPose ProcessResultPose()
            {

                float blendFactor = MovementSpeed / 5;
                blendFactor = Math.Clamp(blendFactor, 0, 1);

                var locomotionPose = Animation.LerpPose(idleAnimation.GetPoseLocal(), runFAnimation.GetPoseLocal(), blendFactor);


                if(attackAnimation.GetBlendFactor() > 0)
                    locomotionPose.LayeredBlend(attackAnimation.GetBoneByName("spine_01"), attackAnimation.GetPoseLocal(), attackAnimation.GetBlendFactor(), 0.7f);

                locomotionPose = Animation.LerpPose(locomotionPose, stunAnimation.GetPoseLocal(), stunAnimation.GetBlendFactor());

                return locomotionPose;

            }

            protected override AnimationPose ProcessSimpleResultPose()
            {
                if (MovementSpeed > 1)
                {
                    return runFAnimation.GetPoseLocal();
                }

                return idleAnimation.GetPoseLocal();
            }


        }

    }
}
