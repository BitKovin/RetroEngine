using BulletSharp;
using BulletSharp.SoftBody;
using DotRecast.Detour.Crowd;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.NavigationSystem;
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

        float maxSpeed = 5;

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

        Entity target;


        public npc_dog()
        {
            SaveGame = true;

        }

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 2f, 0.5f);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            body.CcdMotionThreshold = 0.2f;
            body.CcdSweptSphereRadius = 0.4f;

            bodies.Add(body);

            target = Level.GetCurrent().FindEntityByName("player");


            //npcList.Add(this);

            pathfindingQuery.OnPathFound += PathfindingQuery_OnPathFound;


            InitDirectionsLUT();
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

            mesh.LoadFromFile("models/enemies/dog.FBX");
            mesh.LoadMeshMetaFromFile("models/enemies/dog.fbx");

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


        }

        public override void Update()
        {
            //UpdateNPCList();



        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

 
            if (dead) return;

            dead = true;

            mesh.PlayAnimation("death", false);

            Physics.Remove(body);

            Position = mesh.Position;

            //body.Friction = 0.5f;

            deathSoundPlayer.Position = Position;
            deathSoundPlayer.Play();
            deathSoundPlayer.Destroy(2);

            AsyncUpdate();

        }

        bool reachedFloor = false;

        public override void AsyncUpdate()
        {

            if (dead)
            {


                Vector3 prevPos = Position + Vector3.UnitY/5;

                Position -= Vector3.UnitY * Time.DeltaTime*2;

                var hit = Physics.LineTrace(prevPos.ToPhysics(), Position.ToPhysics(), bodyType: BodyType.World);

                if(hit.HasHit)
                {
                    Position = hit.HitPointWorld;
                    reachedFloor = true;
                }
                mesh.Position = Position;
                mesh.Update(Time.DeltaTime);

                return;
            }



            if (target != null)

                targetLocation = target.Position;

            //MoveDirection = crowdAgent.vel.FromRc().XZ().FastNormalize();


            //crowdAgent.npos = (Position + Vector3.UnitY * heightDif).ToRc();

            float angleDif = MathHelper.FindLookAtRotation(Position, targetLocation).Y - mesh.Rotation.Y;

            MathHelper.Transform transform = new MathHelper.Transform();

            transform.Rotation = new Vector3(0, angleDif, 0);


            float distance = Vector3.Distance(Position, targetLocation);


            if (distance > 3)
            {
                speed += Time.DeltaTime * 10;

            }
            else
            {
                speed -= Time.DeltaTime * 15;
            }

            speed = Math.Clamp(speed, 0, maxSpeed);

            body.Activate();



            body.LinearVelocity = new System.Numerics.Vector3(MoveDirection.X * speed, body.LinearVelocity.Y, MoveDirection.Z * speed);

            MoveDirection = Vector3.Lerp(MoveDirection, DesiredMoveDirection, Time.DeltaTime * 3);


            mesh.Position = Position - new Vector3(0, 1f, 0);

            mesh.Rotation = new Vector3(0, MathHelper.FindLookAtRotation(Vector3.Zero, MoveDirection).Y, 0);

            mesh.UpdateHitboxes();

            //crowdAgent.SetAgentTargetPosition(targetLocation);

            //return;
            if (updateDelay.Wait()) return;
            RequestNewTargetLocation();
            updateDelay.AddDelay(Math.Min(Vector3.Distance(Position, targetLocation) / 30, 0.1f) + Random.Shared.NextSingle() / 10f);
            //TryStep(MoveDirection / 1.5f);


        }

        [JsonInclude]
        public bool dead = false;

        public override void VisualUpdate()
        {
            base.VisualUpdate();

            if (dead) return;

            float cameraDistance = Vector3.Distance(Position, Camera.position);


            mesh.Update(Time.DeltaTime);

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



        protected override EntitySaveData SaveData(EntitySaveData baseData)
        {


            return base.SaveData(baseData);



        }

        public override void LoadData(EntitySaveData Data)
        {
            base.LoadData(Data);

            body.SetPosition(Position);

            if (dead)
            {
                mesh.PlayAnimation("death", false, 0);
                mesh.Update(2);
                Physics.Remove(body);
            }

        }

        public static void ResetStaticData()
        {
            npcList.Clear();
            currentUpdateNPCs.Clear();
            currentUpdateIndex = 0;
        }

    }
}
