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

        float speed = 3;

        public override void Start()
        {
            base.Start();

            body = Physics.CreateCharacterCapsule(this, 1, 0.5f, 1);
            body.Gravity = new Vector3(0, -35, 0).ToNumerics();
            body.SetPosition(Position.ToPhysics());

            mesh.LoadFromFile("models/npc_base.obj");
            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");
            meshes.Add(mesh);
        }

        public override void Update()
        {
            UpdateMovementDirection();

            body.LinearVelocity = new System.Numerics.Vector3(MoveDirection.X * speed, body.LinearVelocity.Y, MoveDirection.Z * speed);
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            mesh.Position = Position - new Vector3(0, 1, 0);

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
