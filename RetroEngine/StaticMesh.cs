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
        //public VertexData[] Vertices;
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
        public bool InFrustrum;

        public float Transparency;

        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Matrix ProjectionViewmodel;

        public Matrix LightView;
        public Matrix LightViewClose;
        public Matrix LightViewVeryClose;
        public Matrix LightProjection;
        public Matrix LightProjectionClose;
        public Matrix LightProjectionVeryClose;

    }

    public class StaticMesh : IDisposable
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

        public Matrix ParrentTransform = Matrix.Identity;

        public Model model;

        public Texture texture;
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
        public bool Masked = false;

        public bool Viewmodel = false;

        internal Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public bool CastShadows = false;

        public float Transparency = 1;

        public bool UseAlternativeRotationCalculation = false;

        protected FrameStaticMeshData frameStaticMeshData = new FrameStaticMeshData();

        protected bool isParticle = false;

        protected bool _disposed = false;

        public bool Static = false;

        public bool occluded = false;
        internal bool inFrustrum = true;

        public Effect Shader;

        public bool OverrideBlend = false;
        public BlendState OverrideBlendState = BlendState.Additive;

        public bool Visible = true;

        public bool SimpleTransperent = false;

        protected Matrix WorldMatrix;

        internal bool destroyed = false;

        public int VisiblePixels = 0;


        public bool DisableOcclusionCulling = false;

        public StaticMesh()
        {

            ormTexture = AssetRegistry.LoadTextureFromFile("engine/textures/defaultORM.png");

            if(GameMain.CompatibilityMode == false)
            OcclusionQuery = new OcclusionQuery(GameMain.Instance.GraphicsDevice);

            Shader = GameMain.Instance.DefaultShader;

            if (GameMain.Instance.DefaultShader == null)
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
            effect.Parameters["EmissionPower"]?.SetValue(EmissionPower);

            if (Graphics.GlobalPointLights == false)
            {
                Vector3[] LightPos = new Vector3[LightManager.MAX_POINT_LIGHTS];
                Vector3[] LightColor = new Vector3[LightManager.MAX_POINT_LIGHTS];
                float[] LightRadius = new float[LightManager.MAX_POINT_LIGHTS];
                float[] LightRes = new float[LightManager.MAX_POINT_LIGHTS];
                Vector4[] LightDir = new Vector4[LightManager.MAX_POINT_LIGHTS];
                RenderTargetCube[] LightMaps = new RenderTargetCube[LightManager.MAX_POINT_LIGHTS];


                LightManager.FinalPointLights = LightManager.FinalPointLights.OrderBy(l => Vector3.Distance(l.Position, useAvgVertexPosition? avgVertexPosition : Position) / l.shadowData.Priority).ToList();

                List<LightManager.PointLightData> objectLights = new List<LightManager.PointLightData>();

                int filledLights = 0;

                List<int> filled = new List<int>();

                //shadows
                for (int i = 0; i < LightManager.FinalPointLights.Count && filledLights < 6; i++)
                {
                    if (LightManager.FinalPointLights[i].shadowData.CastShadows == false) continue;
                    bool intersects = IntersectsBoubndingSphere(new BoundingSphere { Radius = LightManager.FinalPointLights[i].Radius, Center = LightManager.FinalPointLights[i].Position });

                    if (intersects == false) continue;

                    filled.Add(i);

                    objectLights.Add(LightManager.FinalPointLights[i]);
                    filledLights++;

                }

                //no shadows
                for (int i = 0; i < LightManager.FinalPointLights.Count && filledLights < LightManager.MAX_POINT_LIGHTS; i++)
                {
                    //if (LightManager.FinalPointLights[i].shadowData.CastShadows == true) continue;

                    if (filled.Contains(i))
                        continue;

                    bool intersects = IntersectsBoubndingSphere(new BoundingSphere { Radius = LightManager.FinalPointLights[i].Radius, Center = LightManager.FinalPointLights[i].Position });

                    if (intersects == false) continue;

                    objectLights.Add(LightManager.FinalPointLights[i]);
                    filledLights++;

                }

                objectLights = objectLights.OrderByDescending(l => l.shadowData.CastShadows).ToList();

                for (int i = 0; i < objectLights.Count; i++)
                {
                    LightPos[i] = objectLights[i].Position;
                    LightColor[i] = objectLights[i].Color;
                    LightRadius[i] = objectLights[i].Radius;
                    LightRes[i] = objectLights[i].shadowData.resolution;

                    if (objectLights[i].shadowData.CastShadows == false)
                        LightRes[i] = 0;

                    LightDir[i] = new Vector4(objectLights[i].Direction, objectLights[i].MinDot);

                    LightMaps[i] = objectLights[i].shadowData.renderTargetCube;
                }

                effect.Parameters["LightPositions"]?.SetValue(LightPos);
                effect.Parameters["LightColors"]?.SetValue(LightColor);
                effect.Parameters["LightRadiuses"]?.SetValue(LightRadius);
                effect.Parameters["LightResolutions"]?.SetValue(LightRes);
                effect.Parameters["LightDirections"]?.SetValue(LightDir);

                effect.Parameters["PointLightCubemap1"]?.SetValue(LightMaps[0]);
                effect.Parameters["PointLightCubemap2"]?.SetValue(LightMaps[1]);
                effect.Parameters["PointLightCubemap3"]?.SetValue(LightMaps[2]);
                effect.Parameters["PointLightCubemap4"]?.SetValue(LightMaps[3]);
                effect.Parameters["PointLightCubemap5"]?.SetValue(LightMaps[4]);
                effect.Parameters["PointLightCubemap6"]?.SetValue(LightMaps[5]);
                effect.Parameters["PointLightCubemap7"]?.SetValue(LightMaps[6]);
                effect.Parameters["PointLightCubemap8"]?.SetValue(LightMaps[7]);
                effect.Parameters["PointLightCubemap9"]?.SetValue(LightMaps[8]);
                effect.Parameters["PointLightCubemap10"]?.SetValue(LightMaps[9]);

            }

        }

        public virtual bool IntersectsBoubndingSphere(BoundingSphere sphere)
        {
            if (frameStaticMeshData.model is not null)
            {

                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    
                    if (mesh.BoundingSphere.Transform(WorldMatrix).Intersects(sphere))
                    {
                        return true;
                    }


                }
            }

            return false;
        }


        protected virtual void SetupBlending()
        {

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;

            if (OverrideBlend)
            {
                graphicsDevice.BlendState = OverrideBlendState;
                graphicsDevice.DepthStencilState = DepthStencilState.Default;
                return;
            }

            if (Transperent || Graphics.OpaqueBlending == false)
            {
                if (graphicsDevice.BlendState != BlendState.NonPremultiplied)
                {
                    graphicsDevice.BlendState = BlendState.NonPremultiplied;


                }

            }
            else if (graphicsDevice.BlendState != BlendState.Opaque)
            {
                graphicsDevice.BlendState = BlendState.Opaque;

            }

            if (SimpleTransperent && Transperent)
            {
                graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            }
            else
            {
                graphicsDevice.DepthStencilState = DepthStencilState.Default;
                graphicsDevice.RasterizerState = RasterizerState.CullNone;
            }

        }

        public virtual void DrawUnified()
        {
            if ((frameStaticMeshData.IsRendered == false) && frameStaticMeshData.Viewmodel == false || occluded) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = Shader;

            SetupBlending();

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

        protected void UpdateTextureParamIfNeeded(Effect effect, string name, Texture value)
        {

            effect.Parameters[name]?.SetValue(value);

        }

        public virtual void DrawShadow(bool closeShadow = false, bool veryClose = false)
        {
            if (!CastShadows || !isRenderedShadow) return;

            if (!CastShadows) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.ShadowMapEffect;

            if (Masked || Transperent)
            {
                effect = GameMain.Instance.render.ShadowMapMaskedEffect;

            }

            if (frameStaticMeshData.model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {

                    if(closeShadow)
                        if (Graphics.DirectionalLightFrustrumClose.Contains(mesh.BoundingSphere.Transform(WorldMatrix)) == ContainmentType.Disjoint) continue;

                    if (veryClose)
                        if (Graphics.DirectionalLightFrustrumVeryClose.Contains(mesh.BoundingSphere.Transform(WorldMatrix)) == ContainmentType.Disjoint) continue;

                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;


                        if (closeShadow)
                            Graphics.LightViewProjectionClose = frameStaticMeshData.LightViewClose * frameStaticMeshData.LightProjectionClose;
                        else if (veryClose)
                            Graphics.LightViewProjectionVeryClose = frameStaticMeshData.LightViewVeryClose * frameStaticMeshData.LightProjectionVeryClose;
                        else
                            Graphics.LightViewProjection = frameStaticMeshData.LightView * frameStaticMeshData.LightProjection;

                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                        if(Masked||Transperent)
                            UpdateTextureParamIfNeeded(effect, "Texture", FindTexture(meshPartData.textureName));

                        // Set effect parameters
                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);

                        if (closeShadow)
                        {
                            effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjectionClose);
                            effect.Parameters["View"].SetValue(frameStaticMeshData.LightViewClose);
                        } else if(veryClose)
                        {
                            effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjectionVeryClose);
                            effect.Parameters["View"].SetValue(frameStaticMeshData.LightViewVeryClose);
                        }
                        else
                        {
                            effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjection);
                            effect.Parameters["View"].SetValue(frameStaticMeshData.LightView);
                        }

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

        public virtual void DrawDepth()
        {

            if (Viewmodel) return;

            if(Render.IgnoreFrustrumCheck == false)
            if (frameStaticMeshData.InFrustrum == false) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.OcclusionStaticEffect;


            if (Transperent)
                Masked = true;

            if (GameMain.Instance.render.BoundingSphere.Radius == 0 || IntersectsBoubndingSphere(GameMain.Instance.render.BoundingSphere))

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


            if(DisableOcclusionCulling)
            {
                DrawDepth();
                return;
            }


            oclusionCulling = true;


            OcclusionQuery?.Begin();

            DrawDepth();

            OcclusionQuery?.End();
        }

        public void EndOcclusionTest()
        {

            if (destroyed) return;

            if (oclusionCulling == false) return;

            if(DisableOcclusionCulling == true)
            {
                occluded = false;
                return;
            }

            if (OcclusionQuery == null) return;

            if (OcclusionQuery.GraphicsDevice == null) return;
            while (OcclusionQuery.IsComplete == false)
            {

            }

            occluded = OcclusionQuery.PixelCount < 2;

            VisiblePixels = OcclusionQuery.PixelCount;

            oclusionCulling = false;
        }

        protected Texture FindTexture(string name)
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
                        textures.TryAdd(item + name, output);
                        return output;
                    }
                }

            }

            //Console.WriteLine($"failed to find texture {name}");

            return texture;

        }

        protected Texture2D FindTextureWithSufix(string name, string sufix = "_em", Texture2D def = null)
        {

            if (name == null)
                if (emisssiveTexture == null)
                {
                    return GameMain.Instance.render.black;
                }
                else
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
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.Viewmodel ? frameStaticMeshData.ProjectionViewmodel : frameStaticMeshData.Projection);

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
                                Matrix.CreateTranslation(Position) * ParrentTransform;
                return worldMatrix;
            }
        }

        public virtual void LoadFromFile(string filePath)
        {
            model = GetModelFromPath(filePath);

            avgVertexPosition = CalculateAvgVertexLocation();
        }

        internal static Dictionary<string, Assimp.Scene> loadedScenes = new Dictionary<string, Assimp.Scene>();
        protected static Dictionary<string, Model> loadedModels = new Dictionary<string, Model>();

        protected virtual Model GetModelFromPath(string filePath, bool dynamicBuffer = false)
        {
            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            filePath = AssetRegistry.FindPathForFile(filePath);

            if (loadedModels.ContainsKey(filePath))
            {
                return loadedModels[filePath];
            }

            Console.WriteLine("loading model: " + filePath);

            Assimp.Scene scene;
            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            if (loadedScenes.ContainsKey(filePath))
            {
                scene = loadedScenes[filePath];
            }
            else
            {

                string hint = "";
                if (filePath.EndsWith(".obj"))
                    hint = "obj";

                scene = importer.ImportFileFromStream(AssetRegistry.GetFileStreamFromPath(filePath), Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.CalculateTangentSpace | Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.FindDegenerates, formatHint: hint);
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



                if (mesh.Name.Contains("op_"))
                {
                    string name = mesh.Name;
                    name = name.Replace("op_", "");
                    name = name.Replace("_Mesh", "");
                    points.Add(name, new Vector3(-mesh.Vertices[0].X, mesh.Vertices[0].Y, mesh.Vertices[0].Z));
                }

                var vertices = new List<VertexData>();
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
                    var tangent = mesh.Tangents[i];
                    var BiTangent = mesh.BiTangents[i];


                    var textureCoord = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0][i] : new Assimp.Vector3D(0, 0, 0);

                    // Negate the x-coordinate to correct mirroring
                    vertices.Add(new VertexData
                    {
                        Position = new Vector3(vertex.X, vertex.Y, vertex.Z), // Negate x-coordinate
                        Normal = new Vector3(normal.X, normal.Y, normal.Z),
                        TextureCoordinate = new Vector2(textureCoord.X, textureCoord.Y),
                        Tangent = new Vector3(tangent.X, tangent.Y, tangent.Z),
                        BiTangent = new Vector3(BiTangent.X, BiTangent.Y, BiTangent.Z)
                    });
                }


                VertexBuffer vertexBuffer;

                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexData), vertices.Count, BufferUsage.None);

                vertexBuffer.SetData(vertices.ToArray());
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None);
                indexBuffer.SetData(indices.ToArray());

                int numFaces = mesh.FaceCount;
                int primitiveCount = numFaces * 3;  // Each face is a triangle with 3 vertices


                boundingSphere = CalculateBoundingSphere(vertices.ToArray());


                meshParts.Add(new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, StartIndex = 0, NumVertices = indices.Count, PrimitiveCount = primitiveCount, Tag = new MeshPartData { textureName = Path.GetFileName(scene.Materials[mesh.MaterialIndex].TextureDiffuse.FilePath), Points = points } });
            }


            modelMesh.Add(new ModelMesh(graphicsDevice, meshParts) { BoundingSphere = boundingSphere });

            Model model = new Model(graphicsDevice, new List<ModelBone>(), modelMesh);

            loadedModels.TryAdd(filePath, model);

            Console.WriteLine($"loaded model {filePath}");

            return model;
        }


        public static void ClearCache()
        {
            foreach (var model in loadedModels.Values)
            {
                foreach(var mesh in model.Meshes)
                {
                    foreach (var meshPart in mesh.MeshParts)
                    {
                        meshPart.VertexBuffer.Dispose();
                        meshPart.IndexBuffer.Dispose();
                    }
                }
            }
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
            return Camera.frustum.Contains(sphere.Transform(WorldMatrix)) != ContainmentType.Disjoint;
        }

        protected bool IsBoundingSphereInShadowFrustum(BoundingSphere sphere)
        {
            return Graphics.DirectionalLightFrustrum.Contains(sphere.Transform(WorldMatrix)) != ContainmentType.Disjoint;
        }

        public virtual void RenderPreparation()
        {

            WorldMatrix = GetWorldMatrix();

            frameStaticMeshData.Projection = Camera.projection;
            frameStaticMeshData.ProjectionViewmodel = Camera.projectionViewmodel;
            frameStaticMeshData.model = model;
            frameStaticMeshData.Transperent = Transperent;
            frameStaticMeshData.EmissionPower = EmissionPower;
            frameStaticMeshData.View = Camera.finalizedView;
            frameStaticMeshData.World = WorldMatrix;
            frameStaticMeshData.Viewmodel = Viewmodel;
            frameStaticMeshData.LightView = Graphics.LightView;
            frameStaticMeshData.LightViewClose = Graphics.LightCloseView;
            frameStaticMeshData.LightViewVeryClose = Graphics.LightVeryCloseView;
            frameStaticMeshData.LightProjection = Graphics.LightProjection;
            frameStaticMeshData.LightProjectionVeryClose = Graphics.LightVeryCloseProjection;
            frameStaticMeshData.Transparency = Transparency;
            frameStaticMeshData.LightProjectionClose = Graphics.LightCloseProjection;
            frameStaticMeshData.IsRendered = isRendered;
            frameStaticMeshData.IsRenderedShadow = isRenderedShadow;
            frameStaticMeshData.InFrustrum = inFrustrum;
        }

        public virtual void UpdateCulling()
        {
            isRendered = false;
            isRenderedShadow = false;
            inFrustrum = false;

            if (Visible == false) return;

            if (model is null) return;

            //WorldMatrix = GetWorldMatrix();

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
            isRendered = inFrustrum && !occluded || GameMain.SkipFrames>0;
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
            return vector / n;
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
            return Vector3.Transform(GetOffsetPoint(name), WorldMatrix);
        }

        public virtual Vector3 GetOffsetPoint(string name)
        {
            Vector3 point = new Vector3();

            if (model is not null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        MeshPartData data = part.Tag as MeshPartData;
                        if (data is null) continue;

                        if (data.Points.TryGetValue(name, out point))
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

        public virtual void Destroyed()
        {
            //model = null;

            destroyed = true;
            //GameMain.pendingDispose.Add(this);
            Dispose();
            
        }

        public virtual Vector3 GetClosestToCameraPosition()
        {
            return useAvgVertexPosition ? avgVertexPosition : Position;
        }

        public void Dispose()
        {
            Unload();

            GC.SuppressFinalize(this);
            texture = null;
            _disposed = true;
            //OcclusionQuery?.Dispose();
        }

        public static void UnloadModel(Model model)
        {
            foreach (var mesh in model.Meshes)
                foreach (var part in mesh.MeshParts)
                {
                    part.VertexBuffer?.Dispose();
                    part.IndexBuffer?.Dispose();
                    part.Effect?.Dispose();
                    part.Tag = null;
                }
        }

    }
}
