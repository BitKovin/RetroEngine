using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using RetroEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Entities
{
    public class Box:Entity
    {

        StaticMesh mesh = new StaticMesh();

        public Box():base()
        {
            Model model = GameMain.content.Load<Model>("box");
            meshes.Add(mesh);

            mesh.model = model;
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

        }

    }
}
