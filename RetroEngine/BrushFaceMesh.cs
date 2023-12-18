using RetroEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetroEngine.Map;
using Assimp;

namespace RetroEngine
{
    public class BrushFaceMesh : StaticMesh
    {

        public string textureName = "";

        public BrushFaceMesh(Model model, Texture2D texture, string textureName = "")
        {
            this.model = model;
            this.texture = texture;
            this.textureName = textureName;
        }

        public static List<BrushFaceMesh> GetFacesFromPath(string filePath, string objectName, float unitSize = 32)
        {

            if (GameMain.CanLoadAssetsOnThisThread() == false)
            {
                Logger.Log($"THREAD ERROR:  attempted to load merged faces from not render thread. Model path: {filePath}  object name: {objectName}");
                return null;
            }

            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            List<BrushFaceMesh> models = new List<BrushFaceMesh>();
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            Assimp.Scene scene;
            if (loadedScenes.ContainsKey(filePath))
                scene = loadedScenes[filePath];
            else
            {
                scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs);
                loadedScenes.Add(filePath, scene);
            }

            if (scene == null)
            {
                // Error handling for failed file import
                return null;
            }

            bool operatingMesh = false;

            foreach (var mesh in scene.Meshes)
            {

                bool transperent = false;

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

                if (scene.Materials[mesh.MaterialIndex].Name.Contains("_tranperent"))
                    transperent = true;

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
                        new Vector2(textureCoord.X, textureCoord.Y)
                    ));
                }

                var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.None);
                vertexBuffer.SetData(vertices.ToArray());
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None);
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

                models.Add(new BrushFaceMesh(new Model(graphicsDevice, new List<ModelBone>(), new List<ModelMesh> { modelMesh }), texture, scene.Materials[mesh.MaterialIndex].Name) {Transperent = transperent });
            }

            return models;
        }


        public static List<BrushFaceMesh> GetMergedFacesFromPath(string filePath, string objectName, float unitSize = 32)
        {
            if (GameMain.CanLoadAssetsOnThisThread() == false)
            {
                Logger.Log($"THREAD ERROR:  attempted to load merged model from not render thread. Model path: {filePath}  object name: {objectName}");
                return null;
            }

            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            List<BrushFaceMesh> models = new List<BrushFaceMesh>();

            var importer = new Assimp.AssimpContext();

            Assimp.Scene scene;
            if (loadedScenes.ContainsKey(filePath))
                scene = loadedScenes[filePath];
            else
            {
                scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs);
                loadedScenes.Add(filePath, scene);
            }

            if (scene == null)
            {
                // Error handling for failed file import
                return null;
            }

            bool operatingMesh = false;

            foreach (var mesh in scene.Meshes)
            {

                bool transperent = false;

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

                if (scene.Materials[mesh.MaterialIndex].Name.Contains("_tranperent"))
                    transperent = true;

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
                        new Vector3(-normal.X, normal.Y, normal.Z),
                        new Vector2(textureCoord.X, textureCoord.Y)
                    ));
                }

                var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.None);
                vertexBuffer.SetData(vertices.ToArray());
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None);
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
                    PrimitiveCount = primitiveCount,
                    Tag = new MeshPartData { textureName = scene.Materials[mesh.MaterialIndex].Name + ".png" }
                };

                var modelMesh = new ModelMesh(graphicsDevice, new List<ModelMeshPart> { meshPart })
                {
                    BoundingSphere = boundingSphere
                };

                var defaultEffect = new BasicEffect(graphicsDevice);

                meshPart.Effect = defaultEffect;

                models.Add(new BrushFaceMesh(new Model(graphicsDevice, new List<ModelBone>(), new List<ModelMesh> { modelMesh }), texture) { Transperent = transperent });
            }

            return models;
        }


        public static List<BrushFaceMesh> MergeFaceMeshes(List<BrushFaceMesh> meshes)
        {
            List<BrushFaceMesh> merged = new List<BrushFaceMesh>();

            Dictionary<Texture2D, List<BrushFaceMesh>> keyValuePairs = new Dictionary<Texture2D, List<BrushFaceMesh>>();

            foreach (BrushFaceMesh mesh in meshes)
            {
                if(keyValuePairs.ContainsKey(mesh.texture) == false)
                {
                    keyValuePairs.Add(mesh.texture, new List<BrushFaceMesh>());
                }

                keyValuePairs[mesh.texture].Add(mesh);

            }

            List<Model> models = new List<Model>();

            foreach(Texture2D texture in keyValuePairs.Keys)
            {
                models.Clear();

                foreach(BrushFaceMesh mesh in keyValuePairs[texture])
                {
                    models.Add(mesh.model);
                }

                merged.Add(new BrushFaceMesh (MergeModels(models), texture));

            }

            return merged;
        }

        static Model MergeModelsO(List<Model> models)
        {
            List<VertexPositionNormalTexture> Vertices = new List<VertexPositionNormalTexture>();
            List<int> Indices = new List<int>();

            foreach (Model model in models)
            {
                foreach(ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        VertexPositionNormalTexture[] newVertices = new VertexPositionNormalTexture[part.VertexBuffer.VertexCount];
                        part.VertexBuffer.GetData(newVertices);

                        int[] newIndeces = new int[part.VertexBuffer.VertexCount];
                        part.IndexBuffer.GetData(newIndeces);

                        Vertices.AddRange(newVertices);
                        Indices.AddRange(newIndeces);

                    }
                }
            }

            var numVertices = Indices.Count;
            var primitiveCount = numVertices / 3;  // Each triangle has 3 vertices

            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), Vertices.Count, BufferUsage.None);
            vertexBuffer.SetData(Vertices.ToArray());
            var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, Indices.Count, BufferUsage.None);
            indexBuffer.SetData(Indices.ToArray());

            var meshPart = new ModelMeshPart
            {
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                StartIndex = 0,
                NumVertices = numVertices,
                PrimitiveCount = primitiveCount,
                Tag = new MeshPartData { textureName = ((MeshPartData)models[0].Meshes[0].MeshParts[0].Tag).textureName }
            };

            var modelMesh = new ModelMesh(graphicsDevice, new List<ModelMeshPart> { meshPart });

            return new Model(graphicsDevice, new List<ModelBone>(), new List<ModelMesh> { modelMesh });
        }

        public static Model MergeModels(List<Model> models)
        {

            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            // Create lists to store combined vertex and index data
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<int> indices = new List<int>();

            // Offset to keep track of the current vertex index
            int vertexOffset = 0;

            // Loop through each model
            foreach (var model in models)
            {
                // Loop through each mesh in the model
                foreach (var mesh in model.Meshes)
                {
                    // Combine mesh parts into one vertex buffer
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        // Get the vertex and index data
                        VertexPositionNormalTexture[] meshVertices = new VertexPositionNormalTexture[meshPart.VertexBuffer.VertexCount];
                        int[] meshIndices = new int[meshPart.PrimitiveCount * 3];

                        meshPart.VertexBuffer.GetData(meshVertices);
                        meshPart.IndexBuffer.GetData(meshIndices);

                        // Add the vertices to the combined list
                        vertices.AddRange(meshVertices);

                        // Adjust indices to account for the combined vertex data
                        for (int i = 0; i < meshIndices.Length; i++)
                        {
                            meshIndices[i] += vertexOffset;
                        }

                        // Add the indices to the combined list
                        indices.AddRange(meshIndices);
                    }

                    // Update the vertex offset for the next mesh
                    vertexOffset = vertices.Count;
                }
            }

            // Create a new vertex buffer and index buffer for the merged model
            VertexBuffer mergedVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.None);
            mergedVertexBuffer.SetData(vertices.ToArray());

            IndexBuffer mergedIndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None);
            mergedIndexBuffer.SetData(indices.ToArray());

            

            var mergedPart = new ModelMeshPart
            {
                VertexBuffer = mergedVertexBuffer,
                IndexBuffer = mergedIndexBuffer,
                StartIndex = 0,
                NumVertices = vertices.Count,
                PrimitiveCount = indices.Count / 3,
                Tag = new MeshPartData { textureName = ((MeshPartData)models[0].Meshes[0].MeshParts[0].Tag).textureName }
            };

            var modelMesh = new ModelMesh(graphicsDevice, new List<ModelMeshPart> { mergedPart });
            modelMesh.BoundingSphere = CalculateBoundingSphere(vertices.ToArray());

            return new Model(graphicsDevice, new List<ModelBone>(), new List<ModelMesh> { modelMesh });
        }

        public static Model GetCollisionModel(string filePath, string objectName, float unitSize = 32)
        {
            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;


            var importer = new Assimp.AssimpContext();
            var scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.Triangulate);

            if (scene == null)
            {
                // Error handling for failed file import
                return null;
            }


            var meshParts = new List<ModelMeshPart>();

            List<ModelMesh> modelMesh = new List<ModelMesh>();
            BoundingSphere boundingSphere = new BoundingSphere(Vector3.Zero, 100);

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


                var vertices = new VertexPositionNormalTexture[mesh.VertexCount];
                var indices = new int[mesh.FaceCount * 3];
                int vertexIndex = 0;

                foreach (var face in mesh.Faces)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var vertex = mesh.Vertices[face.Indices[2 - i]];
                        var normal = mesh.Normals[face.Indices[2 - i]];
                        var textureCoord = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][face.Indices[2 - i]] : new Assimp.Vector3D(0, 0, 0);

                        if (vertexIndex >= vertices.Length)
                            break;

                        // Negate the x-coordinate to correct mirroring
                        vertices[vertexIndex] = new VertexPositionNormalTexture(
                            new Vector3(-vertex.X/MapData.UnitSize, vertex.Y / MapData.UnitSize, vertex.Z / MapData.UnitSize), // Negate x-coordinate
                            new Vector3(normal.X, normal.Y, normal.Z),
                            new Vector2(textureCoord.X, textureCoord.Y)
                        );

                        indices[vertexIndex] = vertexIndex;
                        vertexIndex++;
                    }
                }


                var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
                vertexBuffer.SetData(vertices);
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
                indexBuffer.SetData(indices);

                int numFaces = mesh.FaceCount;
                int primitiveCount = numFaces * 3;  // Each face is a triangle with 3 vertices


                boundingSphere = CalculateBoundingSphere(vertices);


                meshParts.Add(new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, StartIndex = 0, NumVertices = indices.Length, PrimitiveCount = primitiveCount });
            }


            modelMesh.Add(new ModelMesh(graphicsDevice, meshParts) { BoundingSphere = boundingSphere });

            var defaultEffect = new BasicEffect(graphicsDevice);

            foreach (var meshPart in meshParts)
            {
                meshPart.Effect = defaultEffect;
            }

            return new Model(graphicsDevice, new List<ModelBone>(), modelMesh);

        }
    }
}
