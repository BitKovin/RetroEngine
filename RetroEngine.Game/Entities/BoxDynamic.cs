using BulletSharp;
using Engine;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{
    internal class BoxDynamic : Entity
    {
        StaticMesh mesh = new StaticMesh();

        RigidBody body;

        public BoxDynamic() : base()
        {
            Model model = GameMain.content.Load<Model>("box");
            meshes.Add(mesh);

            mesh.model = model;

            body = Physics.Physics.CreateBox(this, new BulletSharp.Math.Vector3(1,1,1));
        }


        public override void Start()
        {
            base.Start();

            body.SetPosition(new BulletSharp.Math.Vector3(Position.X, Position.Y, Position.Z));

        }

        public override void Update()
        {
            base.Update();

            UpdateCollision();

            mesh.Position = Position;
            mesh.Rotation = Rotation;
            
        }
    }
}
