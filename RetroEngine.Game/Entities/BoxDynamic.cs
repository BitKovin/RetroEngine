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

        SkeletalMesh skeletalMesh = new SkeletalMesh();

        public Vector3 scale = new Vector3(1);


        RigidBody body;

        public BoxDynamic() : base()
        {
            

            mesh.LoadFromFile("models/cube.obj");

            
            skeletalMesh.LoadFromFile("models/skeletal_test.fbx");

            mesh = skeletalMesh;

            mesh.texture = AssetRegistry.LoadTextureFromFile("textures/foil.png");
            mesh.normalTexture = AssetRegistry.LoadTextureFromFile("textures/foil_n.png");
            mesh.ormTexture = AssetRegistry.LoadTextureFromFile("textures/foil_orm.png");
            //mesh.Transperent = true;
            //mesh.Transparency = 1f;

            meshes.Add(mesh);
        }


        public override void Start()
        {
            base.Start();


            body = Physics.CreateBox(this, scale.ToNumerics());
            body.SetPosition(Position.ToNumerics());
            body.SetMassProps(scale.Length(), body.CollisionShape.CalculateLocalInertia(scale.Length()));
            mesh.Scale = scale;

            bodies.Add(body);

        }

        Delay delay = new Delay();

        public override void Update()
        {
            base.Update();

            UpdateCollision();

            //skeletalMesh.Update(Time.deltaTime);

            mesh.Position = Position - new Vector3(0,1,0);
            mesh.Rotation = Rotation;

        }

    }
}
