using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetroEngine;
using System.IO;
using Assimp;

namespace RetroEngine
{
    public class SkeletalMesh : StaticMesh
    {

        Dictionary<string, int> boneNamesToId = new Dictionary<string, int>();

        SkeletalBone rootBone = null;

        protected override Model GetModelFromPath(string filePath, bool dynamicBuffer = false)
        {
            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            filePath = AssetRegistry.FindPathForFile(filePath);


            Assimp.Scene scene;
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            if (loadedScenes.ContainsKey(filePath))
            {
                scene = loadedScenes[filePath];
            }
            else
            {
                scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.CalculateTangentSpace | Assimp.PostProcessSteps.Triangulate);
                //loadedScenes.Add(filePath, scene);
            }

            while (loadedScenes.Keys.Count > 2)
            {
                loadedScenes.Remove(loadedScenes.Keys.First());
            }

            if (scene == null)
            {
                // Error handling for failed file import
                return null;
            }

            Dictionary<string, Vector3> points = new Dictionary<string, Vector3>();

            var meshParts = new List<ModelMeshPart>();


            List<ModelMesh> modelMesh = new List<ModelMesh>();
            BoundingSphere boundingSphere = new BoundingSphere(Vector3.Zero, 100);
            foreach (var mesh in scene.Meshes)
            {

                var vertices = new VertexData[mesh.VertexCount];
                var indices = new int[mesh.FaceCount * 3];
                int vertexIndex = 0;

                if (mesh.Name.Contains("op_"))
                {
                    string name = mesh.Name;
                    name = name.Replace("op_", "");
                    name = name.Replace("_Mesh", "");
                    points.Add(name, new Vector3(-mesh.Vertices[0].X, mesh.Vertices[0].Y, mesh.Vertices[0].Z));
                }

                foreach (var face in mesh.Faces)
                {
                    if (face.Indices.Count != 3) continue;

                    for (int i = 0; i < 3; i++)
                    {
                        var vertex = mesh.Vertices[face.Indices[i]];
                        var normal = mesh.Normals[face.Indices[i]];
                        var tangent = mesh.Tangents[face.Indices[i]];
                        var textureCoord = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][face.Indices[i]] : new Assimp.Vector3D(0, 0, 0);

                        List<VertexWeight> weights = new List<VertexWeight>();
                        List<Bone> bones = new List<Bone>();

                        foreach(var bone in mesh.Bones)
                        {
                            boneNamesToId.TryAdd(bone.Name, boneNamesToId.Count);

                            foreach (VertexWeight weight in bone.VertexWeights)
                            {
                                if (weight.VertexID == vertexIndex)
                                {
                                    weights.Add(weight);
                                    bones.Add(bone);
                                }
                            }
                        }

                        // Negate the x-coordinate to correct mirroring
                        vertices[vertexIndex] = new VertexData(
                            new Vector3(-vertex.X, vertex.Y, vertex.Z), // Negate x-coordinate
                            new Vector3(-normal.X, normal.Y, normal.Z),
                            new Vector2(textureCoord.X, textureCoord.Y),
                            new Vector3(-tangent.X, tangent.Y, tangent.Z)
                        );

                        for (int b = 0; b < bones.Count; b++)
                        {
                            int boneId = boneNamesToId[bones[b].Name];
                            float weight = weights[b].Weight;

                            if(weight > 0.9999f)
                            {
                                weight = 0.9999f;
                            }

                            switch (b)
                            {

                                case 0:
                                    vertices[vertexIndex].bone1 = new Vector2(boneId, weight); break;
                                case 1:
                                    vertices[vertexIndex].bone2 = new Vector2(boneId, weight); break;
                                case 2:
                                    vertices[vertexIndex].bone3 = new Vector2(boneId, weight); break;
                                case 3:
                                    vertices[vertexIndex].bone4 = new Vector2(boneId, weight); break;
                            }


                        }

                        indices[vertexIndex] = vertexIndex;
                        vertexIndex++;
                    }
                }


                VertexBuffer vertexBuffer;
                if (dynamicBuffer)
                {
                    vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(VertexData), vertices.Length, BufferUsage.None);
                }
                else
                {
                    vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexData), vertices.Length, BufferUsage.None);
                }
                vertexBuffer.SetData(vertices);
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
                indexBuffer.SetData(indices);

                int numFaces = mesh.FaceCount;
                int primitiveCount = numFaces * 3;  // Each face is a triangle with 3 vertices


                boundingSphere = CalculateBoundingSphere(vertices);


                meshParts.Add(new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, StartIndex = 0, NumVertices = indices.Length, PrimitiveCount = primitiveCount, Tag = new MeshPartData { textureName = Path.GetFileName(scene.Materials[mesh.MaterialIndex].TextureDiffuse.FilePath), Points = points, Vertices = vertices } });
            }


            modelMesh.Add(new ModelMesh(graphicsDevice, meshParts) { BoundingSphere = boundingSphere });

            Model model = new Model(graphicsDevice, new List<ModelBone>(), modelMesh);

            ConstructSkeletonTree(scene);

            return model;
        }

        void ConstructSkeletonTree(Scene scene)
        {
            rootBone = ProcessBoneNode(scene.RootNode);
        }

        SkeletalBone ProcessBoneNode(Node node)
        {
            SkeletalBone bone = new SkeletalBone();
            bone.name = node.Name;
            bone.relativeTransform = FromAssimpMatrix(node.Transform);

            if(boneNamesToId.ContainsKey(node.Name))
            {
                bone.id = boneNamesToId[node.Name];
                bone.name = node.Name;
            }

            foreach (var child in node.Children)
            {
                 bone.child.Add(ProcessBoneNode(child));
            }
            return bone;
        }

        class SkeletalBone
        {
            public int id = -1;
            public string name;
            public List<SkeletalBone> child = new List<SkeletalBone>();
            public Matrix relativeTransform;

        }

        Matrix FromAssimpMatrix(Matrix4x4 matrix)
        {
            Matrix output = Matrix.Identity;

            output.M11 = matrix.A1;
            output.M12 = matrix.A2;
            output.M13 = matrix.A3;
            output.M14 = matrix.A4;

            output.M21 = matrix.B1;
            output.M22 = matrix.B2;
            output.M23 = matrix.B3;
            output.M24 = matrix.B4;

            output.M31 = matrix.C1;
            output.M32 = matrix.C2;
            output.M33 = matrix.C3;
            output.M34 = matrix.C4;

            output.M41 = matrix.D1;
            output.M42 = matrix.D2;
            output.M43 = matrix.D3;
            output.M44 = matrix.D4;

            return output;
        }

    }
}
