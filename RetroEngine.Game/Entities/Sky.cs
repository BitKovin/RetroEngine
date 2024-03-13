using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{
    internal class Sky : Entity
    {

        StaticMesh mesh = new StaticMesh();

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile("models/sky.obj");
            mesh.emisssiveTexture = AssetRegistry.LoadTextureFromFile("textures/sky/sky.png");
            mesh.texture = AssetRegistry.LoadTextureFromFile("textures/sky/sky.png");
            meshes.Add(mesh);
            mesh.Position = Position;
            mesh.Shader = AssetRegistry.GetShaderFromName("Unlit");
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            mesh.Position = Camera.position;

        }

    }
}
