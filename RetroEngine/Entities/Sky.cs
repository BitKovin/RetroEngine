using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    public class Sky : Entity
    {

        StaticMesh mesh = new SkyMesh();

        public string texturePath = "textures/sky/sky2.png";

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile("models/sky.obj");
            mesh.texture = AssetRegistry.LoadCubeTextureFromFile(texturePath);
            mesh.Static = true;
            mesh.Shader = AssetRegistry.GetShaderFromName("CubeMapVisualizer");

            meshes.Add(mesh);
        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

            mesh.Position = Camera.position;

        }

    }

    internal class SkyMesh : StaticMesh
    {

        internal SkyMesh()
        {
            TwoSided = true;
        }

        public override void UpdateCulling()
        {
            isRendered = true;
            occluded = false;
            frameStaticMeshData.IsRendered = true;
            frameStaticMeshData.IsRenderedShadow = false;
        }

        public override void DrawDepth(bool pointLight = false, bool drawTrans = false)
        {

            Position = Camera.finalizedPosition;
            frameStaticMeshData.World = GetWorldMatrix();

            base.DrawDepth();
        }

        public override void DrawUnified()
        {


            Position = Camera.finalizedPosition;
            frameStaticMeshData.World = GetWorldMatrix();

            base.DrawUnified();
        }

    }

}
