using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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

        public Texture2D texture;

        public virtual void Draw()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = GetWorldMatrix();
                    effect.View = Camera.view;
                    effect.Projection = Camera.projection;
                    effect.Texture = texture;
                    effect.TextureEnabled = texture is not null;
                }

                mesh.Draw();
            }
        }

        public virtual void DrawNormals()
        {
            GraphicsDevice graphicsDevice = GameMain.inst._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.inst.render.NormalEffect;


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

        protected Matrix GetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateScale(Scale) *
                                Matrix.CreateRotationX(Rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(Rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(Rotation.Z / 180 * (float)Math.PI) *
                                Matrix.CreateTranslation(Position);

            return worldMatrix;
        }

        public void LoadFromFile(string filePath)
        {

            GraphicsDevice graphicsDevice = GameMain.inst.GraphicsDevice;


            var importer = new Assimp.AssimpContext();
            var scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs);

            if (scene == null)
            {
                // Error handling for failed file import
                return;
            }

           
            var meshParts = new List<ModelMeshPart>();

            List<ModelMesh> modelMesh = new List<ModelMesh>();
            BoundingSphere boundingSphere = new BoundingSphere(Vector3.Zero,100);
            foreach (var mesh in scene.Meshes)
            {
                var vertices = new VertexPositionNormalTexture[mesh.VertexCount];
                var indices = new int[mesh.FaceCount * 3];
                int vertexIndex = 0;

                foreach (var face in mesh.Faces)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var vertex = mesh.Vertices[face.Indices[2-i]];
                        var normal = mesh.Normals[face.Indices[2-i]];
                        var textureCoord = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][face.Indices[2-i]] : new Assimp.Vector3D(0, 0, 0);

                        // Negate the x-coordinate to correct mirroring
                        vertices[vertexIndex] = new VertexPositionNormalTexture(
                            new Vector3(-vertex.X, vertex.Y, vertex.Z), // Negate x-coordinate
                            new Vector3(normal.X, normal.Y, normal.Z),
                            new Vector2(textureCoord.X, textureCoord.Y)
                        );

                        indices[vertexIndex] = vertexIndex;
                        vertexIndex++;
                    }
                }


                var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(vertices);
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices);

                int numFaces = mesh.FaceCount;
                int primitiveCount = numFaces * 3;  // Each face is a triangle with 3 vertices


                boundingSphere = CalculateBoundingSphere(vertices);


                meshParts.Add(new ModelMeshPart {VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer,StartIndex = 0, NumVertices = indices.Length, PrimitiveCount = primitiveCount});
            }


            modelMesh.Add(new ModelMesh(graphicsDevice, meshParts) {BoundingSphere = boundingSphere});

            var defaultEffect = new BasicEffect(graphicsDevice);

            foreach (var meshPart in meshParts)
            {
                meshPart.Effect = defaultEffect;
            }

            model = new Model(graphicsDevice, new List<ModelBone>(), modelMesh);
            //model.Meshes.Add(new ModelMesh(graphicsDevice, meshParts));

            
        }

        private BoundingSphere CalculateBoundingSphere(VertexPositionNormalTexture[] vertices)
        {
            Vector3 center = Vector3.Zero;
            foreach (var vertex in vertices)
            {
                center += vertex.Position;
            }
            center /= vertices.Length;

            float radius = 0f;
            foreach (var vertex in vertices)
            {
                float distance = Vector3.Distance(center, vertex.Position);
                radius = Math.Max(radius, distance);
            }

            return new BoundingSphere(center, radius);
        }

    }
}
