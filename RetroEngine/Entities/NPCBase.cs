using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Skeletal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RetroEngine.MathHelper;

namespace RetroEngine.Entities
{
    public class NPCBase : Entity
    {

        Vector3 MoveDirection = Vector3.Zero;
        Vector3 DesiredMoveDirection = Vector3.Zero;

        SkeletalMesh mesh = new SkeletalMesh();

        TestAnimator animator = new TestAnimator();

        float speed = 5f;

        static Delay updateDelay = new Delay();

        static List<NPCBase> npcList = new List<NPCBase>();
        static List<NPCBase> currentUpdateNPCs = new List<NPCBase>();
        static int currentUpdateIndex = 0;

        Vector3 targetLocation = Vector3.Zero;

        RigidBody body;

        StaticMesh sm = new StaticMesh();

        static List<Vector3> directionsLUT = new List<Vector3>();

        PathfindingQuery pathfindingQuery = new PathfindingQuery();

        SoundPlayer deathSoundPlayer;
        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 1);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            bodies.Add(body);


            deathSoundPlayer = (SoundPlayer)Level.GetCurrent().AddEntity(new SoundPlayer());

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

            mesh.LoadFromFile("models/skeletal_test.fbx");


            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");


            sm.LoadFromFile("models/cube.obj");
            sm.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            meshes.Add(mesh);
            mesh.CastShadows = true;
            //meshes.Add(sm);


            sm.Transperent = false;

            sm.Scale =new Vector3(0.7f);

            deathSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/mew.wav"));
            deathSoundPlayer.Volume = 1f;

            animator.Load();

        }

        public override void Update()
        {
            //UpdateNPCList();

            if(Input.GetAction("test").Pressed())
            {
                Destroy();
            }
            targetLocation = Camera.position;

            float angleDif = MathHelper.FindLookAtRotation(Position, targetLocation).Y - mesh.Rotation.Y;

            MathHelper.Transform transform = new MathHelper.Transform();

            transform.Rotation = new Vector3(angleDif, 0, 0);

            mesh.SetBoneMeshTransformModification("spine_02", transform.ToMatrix());


            animator.Update();

            mesh.PastePoseLocal(animator.GetResultPose());

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            Destroy();

            deathSoundPlayer.Position = Position;
            deathSoundPlayer.Play();
            deathSoundPlayer.Destroy(2);

        }

        public override void AsyncUpdate()
        {
            body.Activate();

            body.LinearVelocity = new System.Numerics.Vector3(MoveDirection.X * speed, body.LinearVelocity.Y, MoveDirection.Z * speed);

            MoveDirection = Vector3.Lerp(MoveDirection, DesiredMoveDirection, Time.deltaTime*3);

            if (loadedAssets)
                mesh.Update(Time.deltaTime);

            mesh.Position = Position - new Vector3(0, 1.1f, 0);

            mesh.Rotation = new Vector3(0,MathHelper.FindLookAtRotation(Vector3.Zero, MoveDirection).Y, 0);


            RequestNewTargetLocation();

        }
        public override void LateUpdate()
        {
            base.LateUpdate();

        }

        public override void Destroy()
        {
            base.Destroy();

            npcList.Remove(this);

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

            if(locations.Count > 0)
            {
                moveLocation = locations[0];
            }

            Vector3 newMoveDirection = moveLocation - Position;

            

            DesiredMoveDirection = newMoveDirection;

            DesiredMoveDirection.Normalize();
        }

        void RequestNewTargetLocation()
        {
            if(pathfindingQuery.Processing ==false)
                pathfindingQuery.Start(Position, targetLocation);
        }

        private void PathfindingQuery_OnPathFound(List<Vector3> points)
        {
            Vector3 moveLocation = new Vector3();

            if (points.Count > 0)
            {
                moveLocation = points[0];
            }else
            {
                return;
            }

            sm.Position = moveLocation;

            Vector3 newMoveDirection = moveLocation - Position;

            DesiredMoveDirection = newMoveDirection;

            DesiredMoveDirection.Normalize();
        }

        static void UpdateNPCList()
        {

            if (updateDelay.Wait())
                return;

            updateDelay.AddDelay(0.01f);

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

        public static void ResetStaticData()
        {
            npcList.Clear();
            currentUpdateNPCs.Clear();
            currentUpdateIndex = 0;
            updateDelay = new Delay();
        }

        internal class TestAnimator : Animator
        {

            Animation idleAnimation;
            Animation runFAnimation = new Animation();

            bool loaded = false;

            public override void Load()
            {
                base.Load();

                idleAnimation = AddAnimation("Animations/human/idle.fbx");

                runFAnimation = AddAnimation("Animations/human/run_f.fbx");

                loaded = true;
            }

            public override Dictionary<string, Matrix> GetResultPose()
            {
                if(loaded == false)
                    return new Dictionary<string, Matrix>();



                return Animation.LerpPose(idleAnimation.GetPoseLocal(), runFAnimation.GetPoseLocal(),0.5f);

            }

        }

    }
}
