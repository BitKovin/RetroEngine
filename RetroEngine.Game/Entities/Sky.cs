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
            mesh.texture = AssetRegistry.LoadCubeTextureFromFile("textures/sky/sky.jpg");
            meshes.Add(mesh);
            mesh.Static = true;
            mesh.Shader = AssetRegistry.GetShaderFromName("CubeMapVisualizer");
        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

            mesh.Position = Camera.position;

        }

    }
}
