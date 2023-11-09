using BulletSharp;
using RetroEngine;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using RetroEngine.Audio;
using Microsoft.Xna.Framework;

namespace RetroEngine.Game.Entities
{

    [LevelObject("ent_cat_point")]
    public class BoxDynamic : Entity
    {
        StaticMesh mesh = new StaticMesh();

        SoundEffectInstance soundEffectInstance;

        public Vector3 scale = new Vector3(1);

        public BoxDynamic() : base()
        {
            Model model = GameMain.content.Load<Model>("box");
            meshes.Add(mesh);

            mesh.model = model;

            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            soundEffectInstance = AssetRegistry.LoadSoundFromFile("Sounds/test.wav").CreateInstance();

            soundEffectInstance.Play();
            soundEffectInstance.IsLooped = true;
        }


        public override void Start()
        {
            base.Start();

            body = Physics.CreateBox(this, scale.ToPhysics());
            body.SetPosition(new BulletSharp.Math.Vector3(Position.X, Position.Y, Position.Z));
            body.SetMassProps(scale.Length(), body.CollisionShape.CalculateLocalInertia(scale.Length()));
            mesh.Scale = scale;
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
