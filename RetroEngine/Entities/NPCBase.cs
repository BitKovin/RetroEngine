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

        Delay updateDelay = new Delay();

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
        }

        public override void Update()
        {
            if (!updateDelay.Wait())
            {
                try
                {
                    UpdateMovementDirection();
                }
                catch (Exception ex) { Logger.Log("error while pathfinding"); }

                updateDelay.AddDelay(Vector3.Distance(Position, Camera.position) / 30f);
            }
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

        void UpdateMovementDirection()
        {

            List<Vector3> path = Navigation.FindPath(Position, Camera.position);

            

            Vector3 targetLocation = new Vector3();

            if (path.Count > 0)
                targetLocation = path[0];

            MoveDirection = targetLocation - Position;
            MoveDirection.Normalize();
        }

    }
}
