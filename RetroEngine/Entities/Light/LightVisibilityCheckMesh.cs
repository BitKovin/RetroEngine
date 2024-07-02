using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Light
{
    internal class LightVisibilityCheckMesh : StaticMesh
    {

        internal LightVisibilityCheckMesh() 
        {
            //Transperent = true;
        }

        public bool IsVisible()
        {
            return occluded == false && inFrustrum;
        }

        public override void DrawShadow(bool closeShadow = false, bool veryClose = false, bool viewmodel = false)
        {

        }

        public override Vector3 GetClosestToCameraPosition()
        {
            return Camera.rotation.GetForwardVector()*10000000 + Camera.position;
        }

        public override void DrawDepth(bool pointLight)
        {
            var def = GameMain.Instance.GraphicsDevice.DepthStencilState;
            var defBlend = GameMain.Instance.GraphicsDevice.BlendState;
            GameMain.Instance.GraphicsDevice.DepthStencilState = Microsoft.Xna.Framework.Graphics.DepthStencilState.DepthRead;
            GameMain.Instance.GraphicsDevice.BlendState = Microsoft.Xna.Framework.Graphics.BlendState.Additive;

            DrawBlack();

            GameMain.Instance.GraphicsDevice.DepthStencilState = def;
            GameMain.Instance.GraphicsDevice.BlendState = defBlend;
        }

        void DrawBlack()
        {
            if (Render.IgnoreFrustrumCheck == false)
                if (frameStaticMeshData.InFrustrum == false) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.OcclusionStaticEffect;


            if (Transperent)
                Masked = true;

            if (GameMain.Instance.render.BoundingSphere.Radius == 0 || IntersectsBoundingSphere(GameMain.Instance.render.BoundingSphere))

                if (frameStaticMeshData.model is not null)
                {

                    effect.Parameters["black"].SetValue(true);

                    if (frameStaticMeshData.model.Meshes is not null)
                        foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                        {

                            foreach (ModelMeshPart meshPart in mesh.MeshParts)
                            {

                                // Set the vertex buffer and index buffer for this mesh part
                                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                                graphicsDevice.Indices = meshPart.IndexBuffer;

                                effect.Parameters["World"].SetValue(frameStaticMeshData.World);
                                effect.Parameters["Masked"]?.SetValue(Masked);
                                if (Masked)
                                {
                                    MeshPartData meshPartData = meshPart.Tag as MeshPartData;
                                    ApplyShaderParams(effect, meshPartData);


                                    if (texture.GetType() == typeof(RenderTargetCube))
                                        effect.Parameters["Texture"].SetValue(AssetRegistry.LoadTextureFromFile("engine/textures/white.png"));
                                }
                                effect.Techniques[0].Passes[0].Apply();


                                graphicsDevice.DrawIndexedPrimitives(
                                    PrimitiveType.TriangleList,
                                    meshPart.VertexOffset,
                                    meshPart.StartIndex,
                                    meshPart.PrimitiveCount);

                            }
                        }
                }

            effect.Parameters["black"].SetValue(false);
        }

        public override void DrawUnified()
        {

        }

    }
}
