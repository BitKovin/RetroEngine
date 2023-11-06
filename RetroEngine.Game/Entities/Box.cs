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


        public Vector3 size = Vector3.One;

        public Box():base()
        {
            Model model = GameMain.content.Load<Model>("box");
            meshes.Add(mesh);

            mesh.model = model;

            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

        }


        public override void Start()
        {
            base.Start();

            body = Physics.CreateFromShape(this,Vector3.One.ToPhysics(), Physics.CreateCollisionShapeFromModel(mesh.model), collisionFlags: CollisionFlags.StaticObject);

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
