using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetroEngine;

namespace RetroEngine
{
    public class SkeletalMesh : StaticMesh
    {
        //AnimationPlayer animationPlayer;

        public override void Draw()
        {

            Matrix[] bones = null;


            // Render the skinned mesh.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    if (bones is not null)
                        effect.SetBoneTransforms(bones);

                    effect.World = GetWorldMatrix();
                    effect.View = Camera.view;
                    effect.Projection = Camera.projection;
                }

                mesh.Draw();
            }
        }

        public virtual void DrawNormals()
        {
            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.content.Load<Effect>("NormalOutput");

            Matrix[] bones = null;


            if (model is not null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {

                    foreach (SkinnedEffect skinnedEffect in mesh.Effects)
                        if (bones is not null)
                            skinnedEffect.SetBoneTransforms(bones);


                        foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        // Set effect parameters
                        effect.Parameters["World"].SetValue(GetWorldMatrix());
                        effect.Parameters["View"].SetValue(Camera.view);
                        effect.Parameters["Projection"].SetValue(Camera.projection);

                        // Draw the primitives using the custom effect
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                0,
                                meshPart.NumVertices,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }
    }
}
