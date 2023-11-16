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

        StaticMesh mesh = new StaticMesh();

        float speed = 5;

        static Delay updateDelay = new Delay();

        static List<NPCBase> npcList = new List<NPCBase>();
        static NPCBase currentUpdateNPC = null;
        static int currentUpdateIndex = 0;

        static float lastUpdateTime = 0;

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 1);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            mesh.LoadFromFile("models/npc_base.obj");
            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");
            meshes.Add(mesh);
            mesh.CastShadows = false;

            npcList.Add(this);

        }

        public override void Update()
        {

            UpdateNPCList();

            if(currentUpdateNPC == this)
            try
            {
                UpdateMovementDirection();
            }
            catch (Exception ex) { Logger.Log("error while pathfinding"); }

        }

        public override void AsyncUpdate()
        {
            body.LinearVelocity = new System.Numerics.Vector3(MoveDirection.X * speed, body.LinearVelocity.Y, MoveDirection.Z * speed);

            mesh.Position = Position - new Vector3(0, 1, 0);
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

            updateDelay.AddDelay(0.002f);

            currentUpdateIndex++;

            if(npcList.Count>0)
                while(currentUpdateIndex>= npcList.Count)
                {
                    currentUpdateIndex -= npcList.Count;
                }
            currentUpdateNPC = npcList[currentUpdateIndex];

        }

    }
}
