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

        float maxSpeed = 5;

        Delay updateDelay = new Delay();

        static List<NPCBase> npcList = new List<NPCBase>();
        static List<NPCBase> currentUpdateNPCs = new List<NPCBase>();
        static int currentUpdateIndex = 0;

        Vector3 targetLocation = Vector3.Zero;

        RigidBody body;

        StaticMesh sm = new StaticMesh();

        static List<Vector3> directionsLUT = new List<Vector3>();

        PathfindingQuery pathfindingQuery = new PathfindingQuery();

         SoundPlayer deathSoundPlayer;

        protected float AnimationInterpolationDistance = 10;
        protected float AnimationComplexDistance = 30;
        protected float AnimationDistance = 60;
        protected float ShadowDistance = 20;
        protected bool AnimationAlwaysUpdateTime = true;

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 1);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            bodies.Add(body);


            

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

            deathSoundPlayer = (SoundPlayer)Level.GetCurrent().AddEntity(new SoundPlayer());
            deathSoundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("sounds/mew.wav"));
            deathSoundPlayer.Volume = 1f;

            animator.LoadAssets();

        }

        public override void Update()
        {
            //UpdateNPCList();

            

            

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

            targetLocation = Camera.position;

            float angleDif = MathHelper.FindLookAtRotation(Position, targetLocation).Y - mesh.Rotation.Y;

            MathHelper.Transform transform = new MathHelper.Transform();

            transform.Rotation = new Vector3(angleDif, 0, 0);

            mesh.SetBoneMeshTransformModification("spine_02", transform.ToMatrix());



            animator.Speed = ((Vector3)body.LinearVelocity).XZ().Length();

            
            float distance = Vector3.Distance(Position, targetLocation);

            float cameraDistance = Vector3.Distance(Position, Camera.position);

            if (mesh.isRendered && cameraDistance<=AnimationDistance)
            {
                animator.UpdateVisual = true;
                animator.Update();
                animator.Simple = cameraDistance > AnimationComplexDistance;
                animator.InterpolateAnimations = cameraDistance < AnimationInterpolationDistance;
                mesh.PastePoseLocal(animator.GetResultPose());
                mesh.CastShadows = cameraDistance < ShadowDistance;
            }
            else if(AnimationAlwaysUpdateTime)
            {
                animator.UpdateVisual = false;
                animator.Update();
            }


            if(distance > 3) 
            {
                speed += Time.deltaTime * 10;
                
            }else
            {
                speed -= Time.deltaTime * 15;
            }

            speed = Math.Clamp(speed, 0, maxSpeed);

            body.Activate();

            body.LinearVelocity = new System.Numerics.Vector3(MoveDirection.X * speed, body.LinearVelocity.Y, MoveDirection.Z * speed);

            MoveDirection = Vector3.Lerp(MoveDirection, DesiredMoveDirection, Time.deltaTime * 3);


            mesh.Position = Position - new Vector3(0, 1.1f, 0);

            mesh.Rotation = new Vector3(0, MathHelper.FindLookAtRotation(Vector3.Zero, MoveDirection).Y, 0);

            if (updateDelay.Wait()) return;
            RequestNewTargetLocation();
            updateDelay.AddDelay(Math.Min(Vector3.Distance(Position, targetLocation)/60,1));

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

            updateDelay.AddDelay(0.3f);

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
        }

        internal class TestAnimator : Animator
        {

            Animation idleAnimation;
            Animation runFAnimation = new Animation();


            public float Speed = 0;

            protected override void Load()
            {
                base.Load();

                idleAnimation = AddAnimation("Animations/human/idle.fbx");

                runFAnimation = AddAnimation("Animations/human/run_f.fbx", interpolation: true);

                runFAnimation.Speed = 0.9f;

                MathHelper.Transform t = new MathHelper.Transform();

                t.Rotation = new Vector3(0,0,-45);

                //idleAnimation.SetBoneMeshTransformModification("spine_03", t.ToMatrix());

            }

            protected override AnimationPose ProcessResultPose()
            {

                float blendFactor = Speed / 5;
                blendFactor = Math.Clamp(blendFactor, 0, 1);

                return Animation.LerpPose(idleAnimation.GetPoseLocal(), runFAnimation.GetPoseLocal(), blendFactor);

            }

            protected override AnimationPose ProcessSimpleResultPose()
            {
                if (Speed > 1)
                {
                    return runFAnimation.GetPoseLocal();
                }

                return idleAnimation.GetPoseLocal();
            }


        }

    }
}
