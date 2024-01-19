using RetroEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RetroEngine
{

    public class MeshPartData
    {
        public string textureName = "";
        public Dictionary<string, Vector3> Points = new Dictionary<string, Vector3>();
        public VertexData[] Vertices;
    }

    public struct FrameStaticMeshData
    {
        public Model model;
        public Model model2;

        public float EmissionPower;
        public bool Transperent;
        public bool Viewmodel;
        public bool IsRendered;
        public bool IsRenderedShadow;

        public float Transparency;

        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Matrix ProjectionViewmodel;

        public Matrix LightView;
        public Matrix LightProjection;
        public Matrix LightProjectionClose;

    }

    public class StaticMesh : IDisposable
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = new Vector3(1,1,1);

        public Model model;

        public Texture2D texture;
        public Texture2D emisssiveTexture;
        public Texture2D normalTexture;
        public Texture2D ormTexture;
        public List<string> textureSearchPaths = new List<string>();
        public float EmissionPower = 1;

        public float CalculatedCameraDistance = 0;

        public bool isRendered = true;

        public bool isRenderedShadow = true;

        public bool useAvgVertexPosition;

        public Vector3 avgVertexPosition;

        public bool Transperent = false;

        public bool Viewmodel = false;

        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public bool CastShadows = false;

        public float Transparency = 1;

        public bool UseAlternativeRotationCalculation = false;

        protected FrameStaticMeshData frameStaticMeshData = new FrameStaticMeshData();

        protected bool isParticle = false;

        protected bool _disposed = false;

        public bool Static = false;

        protected bool occluded = false;
        protected bool inFrustrum = false;

        public Effect Shader;

        public StaticMesh()
        {

            ormTexture = AssetRegistry.LoadTextureFromFile("engine/textures/defaultORM.png");

            OcclusionQuery = new OcclusionQuery(GameMain.Instance.GraphicsDevice);

            Shader = AssetRegistry.GetShaderFromName("UnifiedOutput");

        }

        public virtual void Draw()
        {
            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.ColorEffect;

            if (model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;
                        if (meshPartData is not null)
                        {
                            effect.Parameters["Texture"].SetValue(FindTexture(meshPartData.textureName));
                        }
                        else
                        {
                            effect.Parameters["Texture"].SetValue(texture);
                        }

                        effect.Parameters["DepthScale"].SetValue(frameStaticMeshData.Viewmodel ? 0.1f : 1);

                        Matrix projection;

                        if (frameStaticMeshData.Viewmodel)
                            projection = frameStaticMeshData.ProjectionViewmodel;
                        else
                            projection = frameStaticMeshData.Projection;

                        effect.Parameters["WorldViewProjection"].SetValue(frameStaticMeshData.World * frameStaticMeshData.View * projection);

                        

                        // Draw the primitives using the custom effect
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }

        protected void LoadCurrentTextures()
        {

            if (model == null) return;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                    if (meshPartData == null) continue;

                    FindTexture(meshPartData.textureName);
                    FindTextureWithSufix(meshPartData.textureName);
                    FindTextureWithSufix(meshPartData.textureName, "_n");
                    FindTextureWithSufix(meshPartData.textureName, "_orm");
                }
            }
        }

        protected void ApplyShaderParams(Effect effect, MeshPartData meshPartData)
        {
            effect.Parameters["World"]?.SetValue(frameStaticMeshData.World);

            effect.Parameters["Transparency"]?.SetValue(frameStaticMeshData.Transparency);

            effect.Parameters["isParticle"]?.SetValue(isParticle);
            effect.Parameters["Viewmodel"]?.SetValue(frameStaticMeshData.Viewmodel);

            

            if (meshPartData is not null && textureSearchPaths.Count > 0)
            {

                UpdateTextureParamIfNeeded(effect, "Texture", FindTexture(meshPartData.textureName));
                UpdateTextureParamIfNeeded(effect, "EmissiveTexture", FindTextureWithSufix(meshPartData.textureName, def: emisssiveTexture));
                UpdateTextureParamIfNeeded(effect, "NormalTexture", FindTextureWithSufix(meshPartData.textureName, "_n", normalTexture));
                UpdateTextureParamIfNeeded(effect, "ORMTexture", FindTextureWithSufix(meshPartData.textureName, "_orm", ormTexture));
            }
            else
            {
                UpdateTextureParamIfNeeded(effect, "Texture", texture);
                UpdateTextureParamIfNeeded(effect, "EmissiveTexture", emisssiveTexture);
                UpdateTextureParamIfNeeded(effect, "NormalTexture", normalTexture);
                UpdateTextureParamIfNeeded(effect, "ORMTexture", ormTexture);
            }
            effect.Parameters["EmissionPower"].SetValue(EmissionPower);
        }

        public virtual void DrawUnified()
        {
            if (frameStaticMeshData.IsRendered == false && frameStaticMeshData.Viewmodel == false) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = Shader;


            if (frameStaticMeshData.model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;


                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;
                        
                        ApplyShaderParams(effect, meshPartData);

                        Stats.RenderedMehses++;

                        // Draw the primitives using the custom effect
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }

        protected void UpdateTextureParamIfNeeded(Effect effect,string name, Texture2D value)
        {
            Texture2D current = effect.Parameters[name]?.GetValueTexture2D();

            if (current == value) 
            { 
                return; 
            }

            effect.Parameters[name]?.SetValue(value);

        }

        public virtual void DrawShadow(bool closeShadow = false)
        {
            if (!CastShadows || !isRenderedShadow) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.ShadowMapEffect;


            if (frameStaticMeshData.model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        
                        if (closeShadow)
                            Graphics.LightViewProjectionClose = frameStaticMeshData.LightView * frameStaticMeshData.LightProjectionClose;
                        else
                            Graphics.LightViewProjection = frameStaticMeshData.LightView * frameStaticMeshData.LightProjection;

                        // Set effect parameters
                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);
                        effect.Parameters["View"].SetValue(frameStaticMeshData.LightView);
                        if(closeShadow)
                            effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjectionClose);
                        else
                            effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjection);

                        // Draw the primitives using the custom effect
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }

        public virtual void DrawOcclusion()
        {

            if (Viewmodel) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.OcclusionEffect;

            if (frameStaticMeshData.model is not null)
            {
                if (frameStaticMeshData.model.Meshes is not null)
                    foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                    {

                        foreach (ModelMeshPart meshPart in mesh.MeshParts)
                        {

                            // Set the vertex buffer and index buffer for this mesh part
                            graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                            graphicsDevice.Indices = meshPart.IndexBuffer;

                            effect.Parameters["World"].SetValue(frameStaticMeshData.World);

                            effect.Techniques[0].Passes[0].Apply();

                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);

                        }
                    }
            }
        }

        public virtual void DrawPathes()
        {
            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.BuffersEffect;

            if (model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;
                        if (meshPartData is not null)
                        {
                            effect.Parameters["ColorTexture"].SetValue(FindTexture(meshPartData.textureName));
                            effect.Parameters["EmissiveTexture"].SetValue(FindTextureWithSufix(meshPartData.textureName));
                        }
                        else
                        {
                            effect.Parameters["ColorTexture"].SetValue(texture);
                            effect.Parameters["EmissiveTexture"].SetValue(GameMain.Instance.render.black);
                        }


                        Matrix projection;

                        if (frameStaticMeshData.Viewmodel)
                            projection = frameStaticMeshData.ProjectionViewmodel;
                        else
                            projection = frameStaticMeshData.Projection;

                        effect.Parameters["WorldViewProjection"].SetValue(frameStaticMeshData.World * frameStaticMeshData.View * projection);
                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);


                        // Draw the primitives using the custom effect
                        
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);
                        
                    }
                }
            }
        }

        OcclusionQuery OcclusionQuery;

        bool oclusionCulling = false;

        public void StartOcclusionTest()
        {

            if (inFrustrum == false) return;

            oclusionCulling = true;

            OcclusionQuery.Begin();

            DrawOcclusion();

            OcclusionQuery.End();
        }

        public void EndOcclusionTest() 
        {

            if (oclusionCulling == false) return;

            while (OcclusionQuery.IsComplete == false)
            {

            }

            occluded = OcclusionQuery.PixelCount < 2;
            oclusionCulling = false;
        }

        protected Texture2D FindTexture(string name)
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
                    output = AssetRegistry.LoadTextureFromFile(item + name, false);
                    if (output != null)
                    {
                        textures.TryAdd(name, output);
                        return output;
                    }
                }
                
            }

            Console.WriteLine($"failed to find texture {name}");

            return texture;
            
        }

        protected Texture2D FindTextureWithSufix(string name, string sufix = "_em", Texture2D def = null)
        {

            if (name == null)
                if (emisssiveTexture == null)
                {
                    return GameMain.Instance.render.black;
                }else
                {
                    return emisssiveTexture;
                }

            name = name.ToLower();
            name = name.Replace(".png", $"{sufix}.png");

            if (textures.ContainsKey(name))
                return textures[name];

            
            if (textureSearchPaths.Count > 0)
            {

                Texture2D output;

                foreach (string item in textureSearchPaths)
                {
                    output = AssetRegistry.LoadTextureFromFile(item + name, true);
                    if (output != null)
                    {
                        textures.TryAdd(name, output);
                        return output;
                    }
                }

            }
            if (def == null)
            {
                return GameMain.Instance.render.black;
            }
            else
            {
                return def;
            }

        }

        public virtual void DrawNormals()
        {

            if (Transperent) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.NormalEffect;


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
                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);
                        effect.Parameters["View"].SetValue(frameStaticMeshData.View);
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.Viewmodel? frameStaticMeshData.ProjectionViewmodel: frameStaticMeshData.Projection);

                        effect.Parameters["DepthScale"].SetValue(frameStaticMeshData.Viewmodel ? 0.01f : 1);

                        // Draw the primitives using the custom effect
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }

        public virtual void DrawMisc()
        {
            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.MiscEffect;


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
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }

        protected virtual Matrix GetWorldMatrix()
        {

            if (UseAlternativeRotationCalculation)
            {
                Matrix worldMatrix = Matrix.CreateScale(Scale) *
                            Matrix.CreateRotationZ(Rotation.Z / 180 * (float)Math.PI) *
                            Matrix.CreateRotationX(Rotation.X / 180 * (float)Math.PI) *
                            Matrix.CreateRotationY(Rotation.Y / 180 * (float)Math.PI) *

                            Matrix.CreateTranslation(Position);
                return worldMatrix;
            }
            else
            {

                Matrix worldMatrix = Matrix.CreateScale(Scale) *
                                Matrix.CreateRotationX(Rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(Rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(Rotation.Z / 180 * (float)Math.PI) *
                                Matrix.CreateTranslation(Position);
                return worldMatrix;
            }
        }

        public virtual void LoadFromFile(string filePath)
        {
            model = GetModelFromPath(filePath);

            avgVertexPosition = CalculateAvgVertexLocation();
        }

        protected static Dictionary<string, Assimp.Scene> loadedScenes = new Dictionary<string, Assimp.Scene>();
        protected static Dictionary<string, Model> loadedModels = new Dictionary<string, Model>();

        protected virtual Model GetModelFromPath(string filePath,bool dynamicBuffer = false)
        {
            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            filePath = AssetRegistry.FindPathForFile(filePath);

            if(loadedModels.ContainsKey(filePath))
            {
                return loadedModels[filePath];
            }

            Assimp.Scene scene;
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            if (loadedScenes.ContainsKey(filePath))
            {
                scene = loadedScenes[filePath];
            }
            else
            {
                scene = importer.ImportFile(filePath, Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.CalculateTangentSpace);
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

                if(mesh.Name.Contains("op_"))
                {
                    string name = mesh.Name;
                    name = name.Replace("op_","");
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

                        // Negate the x-coordinate to correct mirroring
                        vertices[vertexIndex] = new VertexData
                        {
                            Position = new Vector3(-vertex.X, vertex.Y, vertex.Z), // Negate x-coordinate
                            Normal = new Vector3(-normal.X, normal.Y, normal.Z),
                            TextureCoordinate = new Vector2(textureCoord.X, textureCoord.Y),
                            Tangent = new Vector3(-tangent.X, tangent.Y, tangent.Z)
                        };

                        indices[vertexIndex] = vertexIndex;
                        vertexIndex++;
                    }
                }


                VertexBuffer vertexBuffer;

                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexData), vertices.Length, BufferUsage.None);
                
                vertexBuffer.SetData(vertices);
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.None);
                indexBuffer.SetData(indices);

                int numFaces = mesh.FaceCount;
                int primitiveCount = numFaces * 3;  // Each face is a triangle with 3 vertices


                boundingSphere = CalculateBoundingSphere(vertices);


                meshParts.Add(new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, StartIndex = 0, NumVertices = indices.Length, PrimitiveCount = primitiveCount, Tag= new MeshPartData {textureName = Path.GetFileName(scene.Materials[mesh.MaterialIndex].TextureDiffuse.FilePath), Points = points, Vertices = dynamicBuffer? vertices : null} });
            }


            modelMesh.Add(new ModelMesh(graphicsDevice, meshParts) { BoundingSphere = boundingSphere });

            Model model = new Model(graphicsDevice, new List<ModelBone>(), modelMesh);

            loadedModels.TryAdd(filePath, model);

            return model;
        }


        public static void ClearCache()
        {
            loadedModels.Clear();
            loadedScenes.Clear();
        }
        public virtual void PreloadTextures()
        {
            LoadCurrentTextures();
        }

        public static Model CreateModelFromVertices(List<Vector3> vertexPositions)
        {

            if (GameMain.CanLoadAssetsOnThisThread() == false)
            {
                Logger.Log($"THREAD ERROR:  attempted to create model from not render thread.");
                return null;
            }

            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            List<ModelMeshPart> meshParts = new List<ModelMeshPart>();

            VertexData[] vertices = new VertexData[vertexPositions.Count];
            int[] indices = new int[vertexPositions.Count];

            for (int i = 0; i < vertexPositions.Count; i++)
            {
                vertices[i] = new VertexData();
                indices[i] = i;
            }

            // Ensure there are at least 3 vertices to form a triangle
            if (vertices.Length >= 3)
            {
                var vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexData), vertices.Length, BufferUsage.None);
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
            return Camera.frustum.Contains(sphere.Transform(GetWorldMatrix())) != ContainmentType.Disjoint;
        }

        protected bool IsBoundingSphereInShadowFrustum(BoundingSphere sphere)
        {
            return Graphics.DirectionalLightFrustrum.Contains(sphere.Transform(GetWorldMatrix())) != ContainmentType.Disjoint;
        }

        public virtual void RenderPreparation()
        {

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
            frameStaticMeshData.LightProjectionClose = Graphics.GetCloseLightProjection();
            frameStaticMeshData.IsRendered = isRendered;
            frameStaticMeshData.IsRenderedShadow = isRenderedShadow;
        }

        public virtual void UpdateCulling()
        {
            isRendered = false;
            isRenderedShadow = false;

            inFrustrum = false;

            if (model is null) return;
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (IsBoundingSphereInFrustum(mesh.BoundingSphere) || Viewmodel)
                {
                    inFrustrum = true;
                }

                if (IsBoundingSphereInShadowFrustum(mesh.BoundingSphere))
                {
                    isRenderedShadow = true;

                }

            }
            isRendered = inFrustrum && !occluded;
            frameStaticMeshData.IsRendered = isRendered;
        }


        protected Vector3 CalculateAvgVertexLocation()
        {
            float n = 0;

            Vector3 vector = new Vector3();

            foreach (ModelMesh mesh in model.Meshes)
            {

                // Get the vertices from the model's mesh part
                VertexData[] vertices = new VertexData[mesh.MeshParts[0].VertexBuffer.VertexCount];
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

        protected static BoundingSphere CalculateBoundingSphere(VertexData[] vertices)
        {

            List<Vector3> points = new List<Vector3>();

            foreach (VertexData vertex in vertices)
            {
                points.Add(vertex.Position);
            }

            return BoundingSphere.CreateFromPoints(points);
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

        protected virtual void Unload()
        {

        }

        public void Dispose()
        {
            Unload();

            GC.SuppressFinalize(this);
            texture = null;
            _disposed = true;
        }

        public static void UnloadModel(Model model)
        {
            foreach(var mesh in model.Meshes)
                foreach(var part in mesh.MeshParts)
                {
                    part.VertexBuffer?.Dispose();
                    part.IndexBuffer?.Dispose();
                    part.Effect?.Dispose();
                    part.Tag = null;
                }
        }

    }
}
