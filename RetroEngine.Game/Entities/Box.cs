using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using RetroEngine;
using RetroEngine.Physics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Entities
{
    public class Box:Entity
    {

        StaticMesh mesh = new StaticMesh();

        RigidBody body;

        public Box():base()
        {
            Model model = GameMain.content.Load<Model>("box");
            meshes.Add(mesh);

            mesh.model = model;

            body = Physics.CreateBox(this, new BulletSharp.Math.Vector3(5, 1, 5), collisionFlags: CollisionFlags.StaticObject);

            mesh.Scale = new Microsoft.Xna.Framework.Vector3(5, 1, 5);

            body.SetMassProps(0, new BulletSharp.Math.Vector3(0,0,0));

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
