using BulletSharp;
using RetroEngine;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using RetroEngine.Audio;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;

namespace RetroEngine.Game.Entities
{

    [LevelObject("ent_cat_point")]
    public class BoxDynamic : Entity
    {
        StaticMesh mesh = new StaticMesh();

        public Vector3 scale = new Vector3(1);

        SoundPlayer soundPlayer;

        RigidBody body;

        public BoxDynamic() : base()
        {
            Model model = GameMain.content.Load<Model>("box");
            meshes.Add(mesh);

            mesh.model = model;

            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");
        }


        public override void Start()
        {
            base.Start();

            body = Physics.CreateBox(this, scale.ToPhysics());
            body.SetPosition(Position.ToNumerics());
            body.SetMassProps(scale.Length(), body.CollisionShape.CalculateLocalInertia(scale.Length()));
            mesh.Scale = scale;

            bodies.Add(body);

            soundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;

            soundPlayer.SetSound(AssetRegistry.LoadSoundFromFile("Sounds/test.wav"));
            soundPlayer.IsLooped = true;
            //soundPlayer.Play();

        }

        public override void Update()
        {
            base.Update();

            UpdateCollision();

            mesh.Position = Position;
            mesh.Rotation = Rotation;

            soundPlayer.Position = Position;
        }
    }
}
