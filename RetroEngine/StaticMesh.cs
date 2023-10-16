using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace RetroEngine
{
    public class StaticMesh
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = new Vector3(1,1,1);

        public Model model;


        public void Draw()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = GetWorldMatrix();
                    effect.View = Camera.view;
                    effect.Projection = Camera.projection;
                }

                mesh.Draw();
            }
        }

        public void DrawNormals()
        {
            GraphicsDevice graphicsDevice = GameMain.inst._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.content.Load<Effect>("NormalOutput");


            if (model is not null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
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



        Matrix GetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateScale(Scale) *
                                Matrix.CreateRotationX(Rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(Rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(Rotation.Z / 180 * (float)Math.PI) *
                                Matrix.CreateTranslation(Position);

            return worldMatrix;
        }

    }
}
