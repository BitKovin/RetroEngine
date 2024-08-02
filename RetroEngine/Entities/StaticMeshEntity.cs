using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Map;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("static_mesh")]
    public class StaticMeshEntity : Entity
    {
        string modelPath = "";
        string texturePath = "";
        StaticMesh mesh = new StaticMesh();

        RigidBody body;

        float scale = 1;

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            modelPath = data.GetPropertyString("path");
            meshes.Add(mesh);
            texturePath = data.GetPropertyString("texture");
            mesh.Position = Position;
            mesh.Scale = new Vector3(data.GetPropertyFloat("scale", 1)) * new Vector3(1,1,1);


            Vector3 importRot = data.GetPropertyVector("angles", Vector3.Zero);


            Vector3 rotation = EntityData.ConvertRotation(importRot);

            Rotation = rotation;

            //DrawDebug.Line(Position, Position + Rotation.GetForwardVector() * 4, Vector3.One, 40);


            mesh.Rotation = rotation;
            
        }

        public override void Update()
        {
            base.Update();

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile(modelPath);
            var tex = AssetRegistry.LoadTextureFromFile(texturePath);

            if (tex != null)
            {
                mesh.texture = tex;

                var normal = AssetRegistry.LoadTextureFromFile(texturePath.Replace(".png", "_n.png"));

                if (normal != null)
                    mesh.normalTexture = normal;

                var orm = AssetRegistry.LoadTextureFromFile(texturePath.Replace(".png", "_orm.png"));

                if (orm != null)
                    mesh.ormTexture = orm;

            }
            else
            {
                mesh.textureSearchPaths.Add(texturePath);
                
                mesh.PreloadTextures();
            }

            //mesh.BackFaceShadows = true;



            body = Physics.CreateFromShape(this, mesh.Scale.ToPhysics(), Physics.CreateCollisionShapeFromModel(mesh.model, complex: true), 0);

            body.SetPosition(mesh.Position.ToPhysics());
            body.SetRotation(mesh.Rotation);

            mesh.CastShadows = true;

            bodies.Add(body);

        }

    }
}
