using BulletSharp;
using RetroEngine.Map;
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
            mesh.Scale = new Microsoft.Xna.Framework.Vector3(data.GetPropertyFloat("scale", 1));

            mesh.Rotation = new Microsoft.Xna.Framework.Vector3(0, 180+data.GetPropertyFloat("angle"),0);
        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile(modelPath);
            mesh.texture = AssetRegistry.LoadTextureFromFile(texturePath);

            var normal = AssetRegistry.LoadTextureFromFile(texturePath.Replace(".png", "_n.png"));

            if(normal !=null)
            mesh.normalTexture = normal;

            var orm = AssetRegistry.LoadTextureFromFile(texturePath.Replace(".png", "_orm.png"));

            if (orm != null)
            mesh.ormTexture = orm;

            body = Physics.CreateFromShape(this, mesh.Scale.ToPhysics(), Physics.CreateCollisionShapeFromModel(mesh.model, complex: true), 0);

            body.SetPosition(mesh.Position.ToPhysics());
            body.SetRotation(mesh.Rotation);

            mesh.CastShadows = true;

            bodies.Add(body);

        }

    }
}
