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
using BulletSharp;

namespace RetroEngine
{
    public class SkeletalMesh : StaticMesh
    {

        Dictionary<string, int> boneNamesToId = new Dictionary<string, int>();

        public SkeletalBone rootBone = null;

        List<SkeletalBone> bones = new List<SkeletalBone>();


        //loads model. Has a lot of junk code
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
            rootBone.UpdateRefMatrix(new BoneTransform());
        }

        SkeletalBone ProcessBoneNode(Node node)
        {
            SkeletalBone bone = new SkeletalBone();
            bone.name = node.Name;
            bone.relativeTransform = BoneTransformFromAssimpMatrix(node.Transform);
            

            if (boneNamesToId.ContainsKey(node.Name))
            {
                bone.id = boneNamesToId[node.Name];
                bone.name = node.Name;
            }

            foreach (var child in node.Children)
            {

                SkeletalBone childBone = ProcessBoneNode(child);
                bone.child.Add(childBone);
                bones.Add(childBone);
            }
            return bone;
        }

        public struct BoneTransform
        {
            public Vector3 position = new Vector3();
            public Microsoft.Xna.Framework.Quaternion rotation = new Microsoft.Xna.Framework.Quaternion();
            public Vector3 scale = new Vector3(1f);

            public BoneTransform()
            {
            }

            public static BoneTransform operator +(BoneTransform v1, BoneTransform v2) // should add process v2 transforms as it's v1's child bone
            {
                BoneTransform result = new BoneTransform();

                result.rotation = v1.rotation * v2.rotation;
                result.scale = v1.scale * v2.scale;
                result.position = v1.position + Vector3.Transform(v2.position * result.scale, Matrix.CreateFromQuaternion(result.rotation));
                

                return result;
            }
        }

        public class SkeletalBone
        {
            public int id = -1;
            public string name;
            public List<SkeletalBone> child = new List<SkeletalBone>();

            public BoneTransform relativeTransform; //relative bone tranform

            public Matrix referenceMatrix; //mesh space matrix of reference pose

            public Matrix meshTransform; //mesh space matrix of bone. Should be applied to vertices


            public void UpdateTransforms(BoneTransform parrent)
            {

                BoneTransform resultTransform;

                resultTransform = parrent + relativeTransform;

                foreach(var bone in child)
                {
                    bone.UpdateTransforms(resultTransform);
                }
                
                meshTransform = Matrix.CreateScale(resultTransform.scale) *
                            Matrix.CreateFromQuaternion(resultTransform.rotation)*
                            Matrix.CreateTranslation(resultTransform.position);

            }

            //should create reference matrix, but reference matrix isn't used yet. Copy of UpdateTransforms 
            public void UpdateRefMatrix(BoneTransform parrent)
            {

                BoneTransform resultTransform = relativeTransform + parrent;

                foreach (var bone in child)
                {
                    bone.UpdateRefMatrix(resultTransform);
                }

                referenceMatrix = Matrix.CreateScale(resultTransform.scale) *
                            Matrix.CreateFromQuaternion(resultTransform.rotation) *
                            Matrix.CreateTranslation(resultTransform.position);

            }
            public override string ToString()
            {
                return name;
            }

        }

        
        BoneTransform BoneTransformFromAssimpMatrix(Matrix4x4 matrix)
        {
            BoneTransform boneTransform = new BoneTransform();

            Vector3D scaling = new Vector3D();
            Vector3D position = new Vector3D();
            Assimp.Quaternion rotation = new Assimp.Quaternion();

            matrix.Decompose(out scaling, out rotation, out position);

            boneTransform.position = new Vector3(position.X, position.Y, position.Z);
            boneTransform.scale = new Vector3(scaling.X, scaling.Y, scaling.Z);
            boneTransform.rotation = new Microsoft.Xna.Framework.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);

            return boneTransform;
        }

        //translating from assimp to XNA
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

        //forward rendering
        public override void DrawUnified()
        {

            rootBone.UpdateTransforms(new BoneTransform());

            Effect effect = GameMain.Instance.render.UnifiedEffect;

            Matrix[] boneTransforms = new Matrix[128];

            foreach(SkeletalBone bone in bones) 
            {
                if (bone.id < 0) continue;

                boneTransforms[bone.id] = bone.meshTransform;
            }

            effect.Parameters["BoneTransforms"].SetValue(boneTransforms);

            base.DrawUnified();
        }

    }
}
