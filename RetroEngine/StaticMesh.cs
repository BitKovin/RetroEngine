using RetroEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using System.IO;

namespace RetroEngine
{

    class MeshPartData
    {
        public string textureName = "";
        public Dictionary<string, Vector3> Points = new Dictionary<string, Vector3>();
    }

    public struct FrameStaticMeshData
    {
        public Model model;

        public float EmissionPower;
        public bool Transperent;
        public bool Viewmodel;

        public float Transparency;

        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Matrix ProjectionViewmodel;

        public Matrix LightView;
        public Matrix LightProjection;

    }

    public class StaticMesh : IDisposable
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = new Vector3(1,1,1);

        public Model model;

        public Texture2D texture;
        public Texture2D emisssiveTexture;
        public List<string> textureSearchPaths = new List<string>();
        public float EmissionPower = 1;

        public float CalculatedCameraDistance = 0;

        public bool isRendered = true;

        public bool useAvgVertexPosition;

        protected Vector3 avgVertexPosition;

        public bool Transperent = false;

        public bool Viewmodel = false;

        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public bool CastShadows = true;

        public float Transparency = 1;

        protected FrameStaticMeshData frameStaticMeshData = new FrameStaticMeshData();

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

        protected void LoadCurrentTextures()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                    if(meshPartData != null)
                        FindTexture(meshPartData.textureName);
                }
            }
        }

        public virtual void DrawUnified()
        {
            GraphicsDevice graphicsDevice = GameMain.inst._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.inst.render.UnifiedEffect;


            if (frameStaticMeshData.model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        // Set effect parameters
                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);
                        effect.Parameters["View"].SetValue(frameStaticMeshData.View);
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.Viewmodel? frameStaticMeshData.ProjectionViewmodel : frameStaticMeshData.Projection);

                        effect.Parameters["DirectBrightness"].SetValue(Graphics.DirectLighting);
                        effect.Parameters["GlobalBrightness"].SetValue(Graphics.GlobalLighting);
                        effect.Parameters["LightDirection"].SetValue(Graphics.LightDirection);

                        effect.Parameters["ShadowMapViewProjection"].SetValue(Graphics.LightViewProjection);
                        effect.Parameters["ShadowMap"].SetValue(GameMain.inst.render.shadowMap);
                        effect.Parameters["ShadowBias"].SetValue(Graphics.ShadowBias);

                        effect.Parameters["Transparency"].SetValue(frameStaticMeshData.Transparency);

                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                        if (meshPartData is not null)
                        {
                            effect.Parameters["Texture"].SetValue(FindTexture(meshPartData.textureName));
                        }
                        else
                        {
                            effect.Parameters["Texture"].SetValue(texture);
                        }

                        if (meshPartData is not null)
                        {
                            effect.Parameters["EmissiveTexture"].SetValue(FindEmissiveTexture(meshPartData.textureName));
                        }
                        else
                        {
                            effect.Parameters["EmissiveTexture"].SetValue(GameMain.inst.render.black);
                        }
                        effect.Parameters["EmissionPower"].SetValue(EmissionPower);

                        Vector3[] LightPos = new Vector3[LightManager.MAX_POINT_LIGHTS];
                        Vector3[] LightColor = new Vector3[LightManager.MAX_POINT_LIGHTS]; 
                        float[] LightRadius = new float[LightManager.MAX_POINT_LIGHTS]; 

                        for (int i = 0; i < LightManager.MAX_POINT_LIGHTS; i++)
                        {
                            LightPos[i] = LightManager.FinalPointLights[i].Position;
                            LightColor[i] = LightManager.FinalPointLights[i].Color;
                            LightRadius[i] = LightManager.FinalPointLights[i].Radius;
                        }

                        effect.Parameters["LightPositions"].SetValue(LightPos);
                        effect.Parameters["LightColors"].SetValue(LightColor);
                        effect.Parameters["LightRadiuses"].SetValue(LightRadius);

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

        public virtual void DrawShadow()
        {
            if (!CastShadows) return;

            GraphicsDevice graphicsDevice = GameMain.inst._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.inst.render.ShadowMapEffect;


            if (model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        // Set effect parameters
                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);
                        effect.Parameters["View"].SetValue(frameStaticMeshData.LightView);
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjection);

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

        Texture2D FindTexture(string name)
        {
            if (name == null)
                return texture;

            if (textures.ContainsKey(name))
                return textures[name];

            Texture2D output;
            if (textureSearchPaths.Count > 0)
            {
                foreach (string item in textureSearchPaths)
                {
                    output = AssetRegistry.LoadTextureFromFile(item + name, true);
                    if (output != null)
                    {
                        textures.Add(name, output);
                        return output;
                    }
                }
                
            }
            return texture;
            
        }

        Texture2D FindEmissiveTexture(string name)
        {

            if (name == null)
                if (emisssiveTexture == null)
                {
                    return GameMain.inst.render.black;
                }else
                {
                    return emisssiveTexture;
                }

            name = name.ToLower();
            name = name.Replace(".png", "_em.png");

            if (textures.ContainsKey(name))
                return textures[name];

            Texture2D output;
            if (textureSearchPaths.Count > 0)
            {
                foreach (string item in textureSearchPaths)
                {
                    output = AssetRegistry.LoadTextureFromFile(item + name, true);
                    if (output != null)
                    {
                        textures.Add(name, output);
                        return output;
                    }
                }

            }
            if (emisssiveTexture == null)
            {
                return GameMain.inst.render.black;
            }
            else
            {
                return emisssiveTexture;
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

        public virtual void DrawMisc()
        {
            GraphicsDevice graphicsDevice = GameMain.inst._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.inst.render.MiscEffect;


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
                        effect.Parameters["CameraPosition"].SetValue(Camera.position);

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
            model = GetModelFromPath(filePath);

            avgVertexPosition = CalculateAvgVertexLocation();
        }

        static Dictionary<string, Assimp.Scene> loadedScenes = new Dictionary<string, Assimp.Scene>();
        static Dictionary<string, Model> loadedModels = new Dictionary<string, Model>();
        static Assimp.AssimpContext importer = new Assimp.AssimpContext();

        protected Model GetModelFromPath(string filePath)
        {
            GraphicsDevice graphicsDevice = GameMain.inst.GraphicsDevice;

            filePath = AssetRegistry.FindPathForFile(filePath);

            if(loadedModels.ContainsKey(filePath))
            {
                return loadedModels[filePath];
            }

            Assimp.Scene scene;

            if (loadedScenes.ContainsKey(filePath))
            {
                scene = loadedScenes[filePath];
            }
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

            Dictionary<string, Vector3> points = new Dictionary<string, Vector3>();

            var meshParts = new List<ModelMeshPart>();

            List<ModelMesh> modelMesh = new List<ModelMesh>();
            BoundingSphere boundingSphere = new BoundingSphere(Vector3.Zero, 100);
            foreach (var mesh in scene.Meshes)
            {
                var vertices = new VertexPositionNormalTexture[mesh.VertexCount];
                var indices = new int[mesh.FaceCount * 3];
                int vertexIndex = 0;

                if(mesh.Name.Contains("op_"))
                {
                    string name = mesh.Name;
                    name = name.Replace("op_","");
                    name = name.Replace("_Mesh", "");
                    points.Add(name, new Vector3(-mesh.Vertices[0].X, mesh.Vertices[0].Y, mesh.Vertices[0].Z));
                }

                foreach (var face in mesh.Faces)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var vertex = mesh.Vertices[face.Indices[2 - i]];
                        var normal = mesh.Normals[face.Indices[2 - i]];
                        var textureCoord = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][face.Indices[2 - i]] : new Assimp.Vector3D(0, 0, 0);

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


                var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
                vertexBuffer.SetData(vertices);
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
                indexBuffer.SetData(indices);

                int numFaces = mesh.FaceCount;
                int primitiveCount = numFaces * 3;  // Each face is a triangle with 3 vertices


                boundingSphere = CalculateBoundingSphere(vertices);


                meshParts.Add(new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, StartIndex = 0, NumVertices = indices.Length, PrimitiveCount = primitiveCount, Tag= new MeshPartData {textureName = Path.GetFileName(scene.Materials[mesh.MaterialIndex].TextureDiffuse.FilePath), Points = points} });
            }


            modelMesh.Add(new ModelMesh(graphicsDevice, meshParts) { BoundingSphere = boundingSphere });

            Model model = new Model(graphicsDevice, new List<ModelBone>(), modelMesh);

            loadedModels.TryAdd(filePath, model);

            return model;
        }

        public virtual void PreloadTextures()
        {
            LoadCurrentTextures();
        }

        public static Model CreateModelFromVertices(List<Vector3> vertexPositions)
        {

            if (GameMain.IsOnRenderThread() == false)
            {
                Logger.Log($"THREAD ERROR:  attempted to create model from not render thread.");
                return null;
            }

            GraphicsDevice graphicsDevice = GameMain.inst.GraphicsDevice;

            List<ModelMeshPart> meshParts = new List<ModelMeshPart>();

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[vertexPositions.Count];
            int[] indices = new int[vertexPositions.Count];

            for (int i = 0; i < vertexPositions.Count; i++)
            {
                vertices[i] = new VertexPositionNormalTexture(vertexPositions[i], Vector3.Zero, Vector2.Zero);
                indices[i] = i;
            }

            // Ensure there are at least 3 vertices to form a triangle
            if (vertices.Length >= 3)
            {
                var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.None);
                vertexBuffer.SetData(vertices);

                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
                indexBuffer.SetData(indices);

                meshParts.Add(new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, StartIndex = 0, NumVertices = vertices.Length, PrimitiveCount = indices.Length / 3 });
            }

            ModelMesh mesh = new ModelMesh(graphicsDevice, meshParts);

            List<ModelMesh> meshes = new List<ModelMesh>();
            meshes.Add(mesh);

            return new Model(graphicsDevice, new List<ModelBone>(), meshes);
        }

        protected bool IsBoundingSphereInFrustum(BoundingSphere sphere)
        {

            // Calculate the combined view-projection matrix
            Matrix viewProjection = Camera.view * Camera.projection;

            // Transform the sphere position into view space
            Vector3 sphereCenterView = Vector3.Transform(sphere.Center, Camera.view);

            // Calculate the squared radius of the bounding sphere
            float sphereRadiusSquared = sphere.Radius * sphere.Radius;

            // Check if the sphere is inside the camera frustum
            if (sphereCenterView.Z + sphere.Radius < -sphereRadiusSquared
                || sphereCenterView.Z - sphere.Radius > sphereRadiusSquared
                || sphereCenterView.X + sphere.Radius < -sphereRadiusSquared
                || sphereCenterView.X - sphere.Radius > sphereRadiusSquared
                || sphereCenterView.Y + sphere.Radius < -sphereRadiusSquared
                || sphereCenterView.Y - sphere.Radius > sphereRadiusSquared)
            {
                return false;
            }

            return true;
        }

        public virtual void RenderPreparation()
        {

            isRendered = true;

            frameStaticMeshData.Projection = Camera.projection;
            frameStaticMeshData.ProjectionViewmodel = Camera.projectionViewmodel;
            frameStaticMeshData.model = model;
            frameStaticMeshData.Transperent = Transperent;
            frameStaticMeshData.EmissionPower = EmissionPower;
            frameStaticMeshData.View = Camera.view;
            frameStaticMeshData.World = GetWorldMatrix();
            frameStaticMeshData.Viewmodel = Viewmodel;
            frameStaticMeshData.LightView = Graphics.GetLightView();
            frameStaticMeshData.LightProjection = Graphics.GetLightProjection();
            frameStaticMeshData.Transparency = Transparency;
        }

        private Vector3 CalculateAvgVertexLocation()
        {
            float n = 0;

            Vector3 vector = new Vector3();

            foreach (ModelMesh mesh in model.Meshes)
            {

                // Get the vertices from the model's mesh part
                VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[mesh.MeshParts[0].VertexBuffer.VertexCount];
                mesh.MeshParts[0].VertexBuffer.GetData(vertices);
                // Extract the positions and scale them if necessary
                Vector3[] positions = new Vector3[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vector += vertices[i].Position;
                    n++;
                }

            }
            return vector/n;
        }

        protected static BoundingSphere CalculateBoundingSphere(VertexPositionNormalTexture[] vertices)
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


        public virtual Vector3 GetOffsetPointWorldSpace(string name)
        {
            return Vector3.Transform(GetOffsetPoint(name), GetWorldMatrix());
        }

        public virtual Vector3 GetOffsetPoint(string name)
        {
            Vector3 point = new Vector3();

            if(model is not null)
            {
                foreach(ModelMesh mesh in  model.Meshes)
                {
                    foreach(ModelMeshPart part in mesh.MeshParts)
                    {
                        MeshPartData data = part.Tag as MeshPartData;
                        if (data is null) continue;

                        if(data.Points.TryGetValue(name, out point))
                        {
                            return point;
                        }

                    }
                }
            }

            return point;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            texture = null;
            model = null;
        }
    }
}
