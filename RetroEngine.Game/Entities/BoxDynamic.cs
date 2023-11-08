using BulletSharp;
using Engine;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Physics;
using RetroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using RetroEngine.Audio;

namespace RetroEngine.Game.Entities
{

    [LevelObject("ent_cat_point")]
    public class BoxDynamic : Entity
    {
        StaticMesh mesh = new StaticMesh();

        SoundEffectInstance soundEffectInstance;

        public BoxDynamic() : base()
        {
            Model model = GameMain.content.Load<Model>("box");
            meshes.Add(mesh);

            mesh.model = model;

            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            body = Physics.Physics.CreateBox(this, new BulletSharp.Math.Vector3(1,1,1));

            soundEffectInstance = AssetRegistry.LoadSoundFromFile("Sounds/test.wav").CreateInstance();

            soundEffectInstance.Play();
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

            soundEffectInstance.ApplyPosition(Position);


        }
    }
}
