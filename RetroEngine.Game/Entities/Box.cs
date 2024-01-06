using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using RetroEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetroEngine.Entities
{
    public class Box:Entity
    {

        StaticMesh mesh = new StaticMesh();


        public Vector3 size = Vector3.One;

        RigidBody body;

        public Box():base()
        {

            mesh.LoadFromFile("models/cube.obj");


            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            mesh.Viewmodel = true;

            meshes.Add(mesh);

        }


        public override void Start()
        {
            base.Start();

        }

        public override void Update()
        {
            base.Update();

            UpdateCollision();

            mesh.Position = Position;
            mesh.Rotation = Rotation;

            mesh.Scale = size;
        }

    }
}
