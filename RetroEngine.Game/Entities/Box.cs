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

        public Vector3 size = Vector3.One;

        public Box():base()
        {
            Model model = GameMain.content.Load<Model>("box");
            meshes.Add(mesh);

            mesh.model = model;



        }


        public override void Start()
        {
            base.Start();

            body = Physics.CreateBox(this, new BulletSharp.Math.Vector3(size.X, size.Y, size.Z), collisionFlags: CollisionFlags.StaticObject);

            mesh.Scale = size;

            body.SetMassProps(0, new BulletSharp.Math.Vector3(0, 0, 0));

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
