using BulletSharp;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    public class NPCBase : Entity
    {

        Vector3 MoveDirection = Vector3.Zero;

        SkeletalMesh mesh = new SkeletalMesh();

        SkeletalMesh mesh2 = new SkeletalMesh();

        float speed = 2f;

        static Delay updateDelay = new Delay();

        static List<NPCBase> npcList = new List<NPCBase>();
        static List<NPCBase> currentUpdateNPCs = new List<NPCBase>();
        static int currentUpdateIndex = 0;

        RigidBody body;

        StaticMesh sm = new StaticMesh();

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 1);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            bodies.Add(body);

            
            //mesh.CastShadows = false;

            npcList.Add(this);




        }


        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile("models/skeletal_test2.fbx");

            mesh.SetInterpolationEnabled(false);

            mesh.texture = AssetRegistry.LoadTextureFromFile("textures/brushes/__TB_empty.png");


            sm.LoadFromFile("models/cube.obj");
            sm.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            mesh2.LoadFromFile("models/skeletal_test.fbx");

            mesh2.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            mesh.PlayAnimation(0);

            meshes.Add(mesh);
            meshes.Add(mesh2);

            mesh.SetInterpolationEnabled(false);

            sm.Transperent = false;

            sm.Scale =new Vector3(0.2f);


        }

        public override void Update()
        {
            UpdateNPCList();

            if(loadedAssets)
                mesh.Update(Time.deltaTime);

            //sm.Position = mesh.GetBoneMatrix("hand_r").DecomposeMatrix().Position;
            //sm.Rotation = mesh.GetBoneMatrix("hand_r").DecomposeMatrix().Rotation;


            if(currentUpdateNPCs.Contains(this))
                UpdateMovementDirection();
        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            Destroy();

        }

        public override void AsyncUpdate()
        {
            body.LinearVelocity = new System.Numerics.Vector3(MoveDirection.X * speed, body.LinearVelocity.Y, MoveDirection.Z * speed);

            mesh.Position = Position - new Vector3(0, 1.1f, 0);

            mesh.Rotation = new Vector3(0,MathHelper.FindLookAtRotation(Vector3.Zero, MoveDirection).Y, 0);

            mesh2.Position = mesh.Position;
            mesh2.Rotation = mesh.Rotation;

            mesh2.PastePose(mesh.CopyPose());
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
            List<Vector3> path = Navigation.FindPath(Position, Camera.position);

            Vector3 targetLocation = new Vector3();

            if (path.Count > 0)
                targetLocation = path[0];

            MoveDirection = targetLocation - Position;
            MoveDirection.Normalize();
        }

        static void UpdateNPCList()
        {

            if (updateDelay.Wait())
                return;

            updateDelay.AddDelay(0.001f);

            currentUpdateNPCs.Clear();

            for (int i = 0; i < 1; i++)
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

    }
}
