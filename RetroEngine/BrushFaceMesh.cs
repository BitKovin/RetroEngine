using Engine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class BrushFaceMesh : StaticMesh
    {

        public BrushFaceMesh(Model model, Texture2D texture) 
        {
            this.model = model;
            this.texture = texture;
        }

        public static List<BrushFaceMesh> GetFacesFromPath(string filePath, string objectName, float unitSize = 32)
        {
            GraphicsDevice graphicsDevice = GameMain.inst.GraphicsDevice;

            List<BrushFaceMesh> models = new List<BrushFaceMesh>();

            var importer = new Assimp.AssimpContext();
            var scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs);

            if (scene == null)
            {
                // Error handling for failed file import
                return null;
            }

            bool operatingMesh = false;

            foreach (var mesh in scene.Meshes)
            {
                if (mesh.Name != objectName && operatingMesh == false)
                {
                    continue;
                }
                else
                {
                    if (operatingMesh == false)
                    {
                        operatingMesh = true;
                    }
                }

                if (operatingMesh && mesh.Name.Contains("entity") && mesh.Name.Contains("brush") && mesh.Name != objectName)
                    break;

                List<ModelMesh> modelMeshes = new List<ModelMesh>();
                Texture2D texture = AssetRegistry.LoadTextureFromFile(scene.Materials[mesh.MaterialIndex].Name + ".png");

                var vertices = new List<VertexPositionNormalTexture>();
                var indices = new List<int>();


                foreach (var face in mesh.Faces)
                {
                    if (face.IndexCount < 3)
                    {
                        // Skip faces with fewer than 3 vertices (not a triangle)
                        continue;
                    }

                    // Iterate through the face and create triangles
                    for (int i = 1; i < face.IndexCount - 1; i++)
                    {
                        indices.Add(face.Indices[0]);
                        indices.Add(face.Indices[i]);
                        indices.Add(face.Indices[i + 1]);
                    }
                }

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    var vertex = mesh.Vertices[i];
                    var normal = mesh.Normals[i];
                    var textureCoord = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][i] : new Assimp.Vector3D(0, 0, 0);

                    // Negate the x-coordinate to correct mirroring
                    vertices.Add(new VertexPositionNormalTexture(
                        new Vector3(-vertex.X / unitSize, vertex.Y / unitSize, vertex.Z / unitSize), // Negate x-coordinate
                        new Vector3(normal.X, normal.Y, normal.Z),
                        new Vector2(textureCoord.X , textureCoord.Y)
                    ));
                }

                var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.WriteOnly);
                vertexBuffer.SetData(vertices.ToArray());
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices.ToArray());

                var numVertices = indices.Count;
                var primitiveCount = numVertices / 3;  // Each triangle has 3 vertices

                var boundingSphere = CalculateBoundingSphere(vertices.ToArray());

                var meshPart = new ModelMeshPart
                {
                    VertexBuffer = vertexBuffer,
                    IndexBuffer = indexBuffer,
                    StartIndex = 0,
                    NumVertices = numVertices,
                    PrimitiveCount = primitiveCount
                };

                var modelMesh = new ModelMesh(graphicsDevice, new List<ModelMeshPart> { meshPart })
                {
                    BoundingSphere = boundingSphere
                };

                var defaultEffect = new BasicEffect(graphicsDevice);

                meshPart.Effect = defaultEffect;

                models.Add(new BrushFaceMesh(new Model(graphicsDevice, new List<ModelBone>(), new List<ModelMesh> { modelMesh }), texture));
            }

            return models;
        }

    }
}
