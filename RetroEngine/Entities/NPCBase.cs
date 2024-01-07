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

        float speed = 5;

        static Delay updateDelay = new Delay();

        static List<NPCBase> npcList = new List<NPCBase>();
        static List<NPCBase> currentUpdateNPCs = new List<NPCBase>();
        static int currentUpdateIndex = 0;

        RigidBody body;

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 1);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            bodies.Add(body);

            
            mesh.CastShadows = false;

            npcList.Add(this);

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile("models/skeletal_test.fbx");

            mesh.texture = AssetRegistry.LoadTextureFromFile("textures/foil.png");
            mesh.normalTexture = AssetRegistry.LoadTextureFromFile("textures/foil_n.png");
            mesh.ormTexture = AssetRegistry.LoadTextureFromFile("textures/foil_orm.png");


            meshes.Add(mesh);
        }

        public override void Update()
        {
            UpdateNPCList();

            if(loadedAssets)
                mesh.Update(Time.deltaTime);

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

            mesh.Position = Position - new Vector3(0, 1, 0);

            mesh.Rotation = new Vector3(0,MathHelper.FindLookAtRotation(Vector3.Zero, MoveDirection).Y, 0);

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
