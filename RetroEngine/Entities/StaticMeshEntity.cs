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

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            modelPath = data.GetPropertyString("path");
            meshes.Add(mesh);
            texturePath = data.GetPropertyString("texture");
            mesh.Position = Position;

            mesh.Rotation = data.GetPropertyVectorRotation("angles");
            mesh.UseAlternativeRotationCalculation = true;
        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile(modelPath);
            mesh.texture = AssetRegistry.LoadTextureFromFile(texturePath);

        }

    }
}
