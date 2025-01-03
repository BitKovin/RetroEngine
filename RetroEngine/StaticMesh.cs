﻿using RetroEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RetroEngine.PhysicsSystem;
using RetroEngine.Graphic;

namespace RetroEngine
{

    public class MeshPartData
    {
        public string Name = "";
        public string textureName = "";
        public Dictionary<string, Vector3> Points = new Dictionary<string, Vector3>();

        public int MeshIndex = -1; //used during skeletal mesh loading

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
        public Matrix LightViewmodelView;
        public Matrix LightViewClose;
        public Matrix LightViewVeryClose;

        public Matrix LightViewmodelProjection;
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
        public float EmissionPower = 2;


        public bool isRendered = true;

        public bool isRenderedShadow = true;

        public bool useAvgVertexPosition;

        public Vector3 avgVertexPosition;

        public bool Transperent = false;
        public bool Masked = false;

        public bool Viewmodel = false;

        public bool CastViewModelShadows = true;

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

        public SurfaceShaderInstance Shader;

        public bool OverrideBlend = false;
        public BlendState OverrideBlendState = BlendState.Additive;

        public bool Visible = true;

        public bool SimpleTransperent = false;

        protected Matrix WorldMatrix;

        internal bool destroyed = false;

        public int VisiblePixels = 0;

        public bool TwoSided = false;

        public bool DisableOcclusionCulling = false;

        public float DitherDisolve = 0;

        public bool CastGeometricShadow = false;

        public bool DepthTestEqual = false; //pixel gets discarded if depths does not equal to prepath(not closer or farther. Only equal)

        public bool BackFaceShadows = false;

        public float NormalBiasScale = 1;

        public bool LargeObject = false;

        internal bool partialTransparency = false;

        public List<string> MeshHideList = new List<string>();
        protected string[] finalizedMeshHide = new string[0];

        public Entity Owner;

        public StaticMesh()
        {

            if(Render.UsesOpenGL)
            {
                if(Level.ChangingLevel == false)
                {
                    DisableOcclusionCulling = true;
                }
            }

            ormTexture = AssetRegistry.LoadTextureFromFile("engine/textures/defaultORM.png");

            if(GameMain.CompatibilityMode == false)
            OcclusionQuery = new OcclusionQuery(GameMain.Instance.GraphicsDevice);

            Shader = new SurfaceShaderInstance(GameMain.Instance.DefaultShader);

            if (GameMain.Instance.DefaultShader == "")
                Shader = new SurfaceShaderInstance("UnifiedOutput");

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

        public struct MeshData
        {
            public List<Vector3> vertices;
            public List<int> indices;
        }

        public virtual List<MeshData> GetMeshData()
        {

            List<MeshData> meshData = new List<MeshData>();

            foreach(ModelMesh mesh in model.Meshes)
                foreach(ModelMeshPart meshPart in mesh.MeshParts)
                {

                    VertexData[] vertexData = new VertexData[meshPart.VertexBuffer.VertexCount];
                    int[] indices = new int[meshPart.IndexBuffer.IndexCount];

                    meshPart.VertexBuffer.GetData(vertexData);
                    meshPart.IndexBuffer.GetData(indices);

                    List<Vector3> positions = new List<Vector3>();

                    if (Position != Vector3.Zero || Rotation != Vector3.Zero || Scale != Vector3.Zero)
                    {

                        foreach (VertexData data in vertexData)
                        {
                            positions.Add(Vector3.Transform(data.Position, GetWorldMatrix()));
                        }
                    }
                    else
                    {
                        foreach (VertexData data in vertexData)
                        {
                            positions.Add(data.Position);   
                        }
                    }


                    meshData.Add(new MeshData { indices = indices.ToList(), vertices = positions });

                }

            return meshData;

        }

        List<Vector3> vertexPosCache;

        public virtual List<Vector3> GetMeshVertices()
        {

            List<Vector3> positions = new List<Vector3>();

            if (vertexPosCache == null)
            {

                vertexPosCache = new List<Vector3>();

                foreach (ModelMesh mesh in model.Meshes)
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        VertexData[] vertexData = new VertexData[meshPart.VertexBuffer.VertexCount];
                        int[] indices = new int[meshPart.IndexBuffer.IndexCount];

                        meshPart.VertexBuffer.GetData(vertexData);
                        meshPart.IndexBuffer.GetData(indices);

                        foreach (VertexData data in vertexData)
                        {
                            vertexPosCache.Add(data.Position);
                        }
                    }
            }

            foreach (Vector3 pos in vertexPosCache)
            {
                if (Position != Vector3.Zero || Rotation != Vector3.Zero || Scale != Vector3.Zero)
                {


                    positions.Add(Vector3.Transform(pos, GetWorldMatrix()));

                }
                else
                {

                    positions.Add(pos);

                }
            }

            return positions;

        }


        public List<BoundingBox> GetSubdividedBoundingBoxes()
        {

            var data = GetMeshVertices();
            var result = GenerateBoundingBoxes(data, 2);

            return result;

        }

        public static List<BoundingBox> GenerateBoundingBoxes(List<Vector3> vertices, int boxesPerAxis)
        {
            if (boxesPerAxis <= 0)
            {
                throw new ArgumentException("Number of boxes per axis must be greater than zero.");
            }

            // Determine the min and max bounds of the mesh
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var vertex in vertices)
            {
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            // Calculate the size of each box
            Vector3 size = (max - min) / boxesPerAxis;
            List<BoundingBox> allBoundingBoxes = new List<BoundingBox>();

            // Generate the bounding boxes
            for (int x = 0; x < boxesPerAxis; x++)
            {
                for (int y = 0; y < boxesPerAxis; y++)
                {
                    for (int z = 0; z < boxesPerAxis; z++)
                    {
                        Vector3 boxMin = min + new Vector3(x, y, z) * size;
                        Vector3 boxMax = boxMin + size;
                        allBoundingBoxes.Add(new BoundingBox(boxMin, boxMax));
                    }
                }
            }

            // Filter out bounding boxes that do not contain any vertices
            List<BoundingBox> filteredBoundingBoxes = new List<BoundingBox>();

            foreach (var box in allBoundingBoxes)
            {
                foreach (var vertex in vertices)
                {
                    if (box.Contains(vertex) != ContainmentType.Disjoint)
                    {
                        filteredBoundingBoxes.Add(box);
                        break;
                    }
                }
            }

            return filteredBoundingBoxes;
        }

        public bool isNegativeScale()
        {
            return (Scale.X * Scale.Y * Scale.Z) < 0;
        }

        protected void ApplyShaderParams(Effect effect, MeshPartData meshPartData, bool skipLight = true)
        {
            effect.Parameters["World"]?.SetValue(frameStaticMeshData.World);

            effect.Parameters["Transparency"]?.SetValue(frameStaticMeshData.Transparency);

            effect.Parameters["isParticle"]?.SetValue(isParticle);
            effect.Parameters["Viewmodel"]?.SetValue(frameStaticMeshData.Viewmodel);

            effect.Parameters["Masked"]?.SetValue(Masked);
            effect.Parameters["depthTestEqual"]?.SetValue(DepthTestEqual);

            effect.Parameters["DitherDisolve"]?.SetValue(DitherDisolve);

            effect.Parameters["LargeObject"]?.SetValue(LargeObject);

            effect.Parameters["earlyZ"]?.SetValue(Graphics.EarlyDepthDiscardShader && (SimpleTransperent == false || Transperent == false));

            if (Viewmodel&&Graphics.ViewmodelShadows)
            {

                effect.Parameters["ShadowMap"]?.SetValue(GameMain.Instance.render.shadowMapViewmodel);
                effect.Parameters["ShadowMapViewProjection"]?.SetValue(Graphics.LightViewProjectionViewmodel);
                effect.Parameters["ShadowMapResolution"]?.SetValue(Graphics.ViewmodelShadowMapResolution);

            }
            else
            {

                effect.Parameters["ShadowMap"]?.SetValue(GameMain.Instance.render.shadowMap);
                effect.Parameters["ShadowMapViewProjection"]?.SetValue(Graphics.LightViewProjection);
                effect.Parameters["ShadowMapResolution"]?.SetValue(Graphics.shadowMapResolution);

            }

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

            if (skipLight) return;

        }

        protected void ApplyPointLights(Effect effect)
        {

            if (Graphics.GlobalPointLights == false)
            {
                Vector4[] LightPos = new Vector4[LightManager.MAX_POINT_LIGHTS];
                Vector3[] LightColor = new Vector3[LightManager.MAX_POINT_LIGHTS];
                float[] LightRadius = new float[LightManager.MAX_POINT_LIGHTS];
                float[] LightRes = new float[LightManager.MAX_POINT_LIGHTS];
                Vector4[] LightDir = new Vector4[LightManager.MAX_POINT_LIGHTS];
                Texture[] LightMaps = new Texture[LightManager.MAX_POINT_LIGHTS];


                LightManager.FinalPointLights = LightManager.FinalPointLights.OrderBy(l => Vector3.Distance(l.Position, useAvgVertexPosition ? avgVertexPosition : Position) / l.shadowData.Priority).ToList();

                List<LightManager.PointLightData> objectLights = new List<LightManager.PointLightData>();

                int filledLights = 0;

                HashSet<int> filled = new HashSet<int>();

                int shaderPointLightsShadowed = 7;

                //shadows
                for (int i = 0; i < LightManager.FinalPointLights.Count && filledLights < Math.Min(LightManager.MAX_POINT_LIGHTS, shaderPointLightsShadowed); i++)
                {
                    if (LightManager.FinalPointLights[i].visible == false) continue;
                    if (LightManager.FinalPointLights[i].shadowData.CastShadows == false) continue;
                    bool intersects = IntersectsBoundingSphere(new BoundingSphere { Radius = LightManager.FinalPointLights[i].Radius, Center = LightManager.FinalPointLights[i].Position });

                    if (intersects == false) continue;

                    filled.Add(i);

                    objectLights.Add(LightManager.FinalPointLights[i]);
                    filledLights++;

                }

                //no shadows
                for (int i = 0; i < LightManager.FinalPointLights.Count && filledLights < LightManager.MAX_POINT_LIGHTS; i++)
                {
                    //if (LightManager.FinalPointLights[i].shadowData.CastShadows == true) continue;

                    if (LightManager.FinalPointLights[i].visible == false) continue;

                    if (filled.Contains(i))
                        continue;

                    bool intersects = IntersectsBoundingSphere(new BoundingSphere { Radius = LightManager.FinalPointLights[i].Radius, Center = LightManager.FinalPointLights[i].Position });

                    if (intersects == false) continue;

                    objectLights.Add(LightManager.FinalPointLights[i]);
                    filledLights++;

                }

                objectLights = objectLights.OrderByDescending(l => l.shadowData.CastShadows).ToList();

                for (int i = 0; i < objectLights.Count; i++)
                {
                    LightPos[i] = new Vector4(objectLights[i].Position, objectLights[i].InnerMinDot);
                    LightColor[i] = objectLights[i].Color;
                    LightRadius[i] = objectLights[i].Radius;
                    LightRes[i] = objectLights[i].Resolution;

                    if (objectLights[i].shadowData.CastShadows == false)
                        LightRes[i] = 0;

                    LightDir[i] = new Vector4(objectLights[i].Direction, objectLights[i].MinDot);

                    LightMaps[i] = objectLights[i].shadowData.renderTarget;
                }

                effect.Parameters["LightPositions"]?.SetValue(LightPos);
                effect.Parameters["LightColors"]?.SetValue(LightColor);
                effect.Parameters["LightRadiuses"]?.SetValue(LightRadius);
                effect.Parameters["LightResolutions"]?.SetValue(LightRes);
                effect.Parameters["LightDirections"]?.SetValue(LightDir);

                effect.Parameters["PointLightsNumber"]?.SetValue(objectLights.Count);

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


        public virtual bool IntersectsBoundingSphere(BoundingSphere sphere)
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
                graphicsDevice.RasterizerState = Graphics.DisableBackFaceCulling || TwoSided ? RasterizerState.CullNone : (isNegativeScale() ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise);
            }
            else
            {
                graphicsDevice.DepthStencilState = DepthStencilState.Default;
                graphicsDevice.RasterizerState = Graphics.DisableBackFaceCulling || TwoSided ? RasterizerState.CullNone : (isNegativeScale() ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise);

            }

        }

        protected virtual Matrix GetLocalOffset()
        {
            return Matrix.Identity;
        }

        protected virtual Matrix GetLocalRotationOffset()
        {
            return Matrix.Identity;
        }

        public virtual void DrawUnified()
        {
            if ((frameStaticMeshData.IsRendered == false) && frameStaticMeshData.Viewmodel == false || occluded) return;

            if (Transperent && Render.DrawOnlyOpaque) return;
            if(Transperent == false && Render.DrawOnlyTransparent) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = Shader.GetAndApply(Transperent ? SurfaceShaderInstance.ShaderSurfaceType.Transperent : SurfaceShaderInstance.ShaderSurfaceType.Default);

            

            if (frameStaticMeshData.model is not null)
            {

                if (DepthTestEqual)
                {
                    if (Viewmodel == false)
                    {
                        GameMain.Instance.render.OcclusionEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjection);
                        GameMain.Instance.render.OcclusionStaticEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjection);
                    }
                    else
                    {

                        GameMain.Instance.render.OcclusionEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjectionViewmodel);
                        GameMain.Instance.render.OcclusionStaticEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjectionViewmodel);
                    }

                    DrawDepth(renderTransperent: true);



                    DepthStencilState customDepthStencilState = new DepthStencilState
                    {
                        DepthBufferEnable = true,
                        DepthBufferWriteEnable = true,
                        DepthBufferFunction = CompareFunction.Equal,
                        StencilEnable = true
                    };

                    graphicsDevice.DepthStencilState = customDepthStencilState;

                    BlendState blend = new BlendState { ColorWriteChannels = ColorWriteChannels.None };

                    graphicsDevice.BlendState = blend;
                }
                else
                {
                    graphicsDevice.DepthStencilState = DepthStencilState.Default;

                }

                SetupBlending();

                if (DepthTestEqual)
                {
                    DepthStencilState customDepthStencilState = new DepthStencilState
                    {
                        DepthBufferEnable = true,
                        DepthBufferWriteEnable = false,
                        DepthBufferFunction = CompareFunction.LessEqual,
                        StencilEnable = false,

                    };


                    graphicsDevice.DepthStencilState = customDepthStencilState;


                    RasterizerState rasterizerState = new RasterizerState()
                    {
                        CullMode = graphicsDevice.RasterizerState.CullMode,
                        FillMode = graphicsDevice.RasterizerState.FillMode,
                        DepthBias = Viewmodel ? -0.000005f : -0.0001f,
                        MultiSampleAntiAlias = false,
                        ScissorTestEnable = graphicsDevice.RasterizerState.ScissorTestEnable,
                        SlopeScaleDepthBias = graphicsDevice.RasterizerState.SlopeScaleDepthBias,
                        DepthClipEnable = graphicsDevice.RasterizerState.DepthClipEnable

                    };

                    graphicsDevice.RasterizerState = rasterizerState;

                }

                ApplyPointLights(effect);

                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;


                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                        if(meshPartData != null)
                        {
                            if (finalizedMeshHide.Contains(meshPartData.Name))
                                continue;
                        }

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

        public virtual void DrawShadow(bool closeShadow = false, bool veryClose = false, bool viewmodel = false)
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

            float bias = 0.09f;

            if (closeShadow)
                bias = 0.045f;

            if (veryClose)
                bias = 0.02f;

            bias *= NormalBiasScale;

            bias /= Graphics.ShadowResolutionScale;

            bias *= Graphics.LightDistanceMultiplier;

            //bias = 0.015f;

            effect.Parameters["bias"].SetValue(bias);
            effect.Parameters["depthBias"].SetValue(BackFaceShadows ? 0.004f : 0);

            if(veryClose)
                effect.Parameters["depthBias"].SetValue(BackFaceShadows ? 0.004f : 0);

            if (closeShadow)
                effect.Parameters["depthBias"].SetValue(BackFaceShadows ? 0.001f : 0);

            if (viewmodel)
            {
                
                graphicsDevice.RasterizerState = RasterizerState.CullNone;
            }
            else
            {
                if (BackFaceShadows)
                {
                    graphicsDevice.RasterizerState = isNegativeScale() == false ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
                }
                else
                {
                    graphicsDevice.RasterizerState = isNegativeScale() ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
                    //graphicsDevice.RasterizerState = RasterizerState.CullNone;
                }

                


            }

            if (closeShadow)
                Graphics.LightViewProjectionClose = frameStaticMeshData.LightViewClose * frameStaticMeshData.LightProjectionClose;
            else if (veryClose)
                Graphics.LightViewProjectionVeryClose = frameStaticMeshData.LightViewVeryClose * frameStaticMeshData.LightProjectionVeryClose;
            else if (viewmodel)
                Graphics.LightViewProjectionViewmodel = frameStaticMeshData.LightViewmodelView * frameStaticMeshData.LightViewmodelProjection;
            else
                Graphics.LightViewProjection = frameStaticMeshData.LightView * frameStaticMeshData.LightProjection;


            if (frameStaticMeshData.model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {

                    if (closeShadow)
                        if (Graphics.DirectionalLightFrustrumClose.Contains(mesh.BoundingSphere.Transform(WorldMatrix)) == ContainmentType.Disjoint) continue;


                    if (veryClose)
                        if (Graphics.DirectionalLightFrustrumVeryClose.Contains(mesh.BoundingSphere.Transform(WorldMatrix)) == ContainmentType.Disjoint) continue;


                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                        if (meshPartData != null)
                        {
                            if (finalizedMeshHide.Contains(meshPartData.Name))
                                continue;
                        }


                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;



                        if (Masked||Transperent)
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
                        else if (viewmodel)
                        {
                            effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightViewmodelProjection);
                            effect.Parameters["View"].SetValue(frameStaticMeshData.LightViewmodelView);
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

        protected virtual bool isObjectInsideFrustrum(BoundingFrustum frustum)
        {
            return true;
        }

        public virtual void DrawDepth(bool pointLightDraw = false, bool renderTransperent = false)
        {

            if (Viewmodel) return;


            if (Transparency < 1 && renderTransperent == false) return;
            if (DitherDisolve > 0) return;

            if (Render.IgnoreFrustrumCheck == false)
                if (frameStaticMeshData.InFrustrum == false) return;

            

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.OcclusionStaticEffect;

            effect.Parameters["Masked"]?.SetValue(false);

            bool mask = Masked;

            if (Transperent)
                mask = true;

            graphicsDevice.RasterizerState = Graphics.DisableBackFaceCulling || TwoSided ? RasterizerState.CullNone : ((isNegativeScale()) ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise);


            if (GameMain.Instance.render.BoundingSphere.Radius == 0 || IntersectsBoundingSphere(GameMain.Instance.render.BoundingSphere))

                if (frameStaticMeshData.model is not null)
                {


                    effect.Parameters["World"].SetValue(frameStaticMeshData.World);
                    effect.Parameters["Masked"]?.SetValue(mask);

                    if (mask == false)
                    {
                        ApplyShaderParams(effect, null, true);
                        effect.Techniques[0].Passes[0].Apply();
                    }

                    if (frameStaticMeshData.model.Meshes is not null)
                        foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                        {
                            if (Render.CustomFrustrum != null)
                            {
                                if (Render.CustomFrustrum.Contains(mesh.BoundingSphere.Transform(frameStaticMeshData.World))  == ContainmentType.Disjoint)
                                {
                                    continue;
                                }
                            }
                            foreach (ModelMeshPart meshPart in mesh.MeshParts)
                            {


                                MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                                if (meshPartData != null)
                                {
                                    if (finalizedMeshHide.Contains(meshPartData.Name))
                                        continue;
                                }

                                // Set the vertex buffer and index buffer for this mesh part
                                graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                                graphicsDevice.Indices = meshPart.IndexBuffer;




                                if (mask)
                                {
                                    ApplyShaderParams(effect, meshPartData, true);
                                    effect.Parameters["Masked"]?.SetValue(mask);

                                    if (texture.GetType() == typeof(RenderTargetCube))
                                        effect.Parameters["Texture"].SetValue(AssetRegistry.LoadTextureFromFile("engine/textures/white.png"));

                                    effect.Techniques[0].Passes[0].Apply();
                                }



                                graphicsDevice.DrawIndexedPrimitives(
                                    PrimitiveType.TriangleList,
                                    meshPart.VertexOffset,
                                    meshPart.StartIndex,
                                    meshPart.PrimitiveCount);

                            }
                        }
                }
        }

        public virtual void AddNormalsToPositionNormalDictionary(ref Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)> positionToNormals)
        {
            if (model == null) return;

            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {

                    VertexData[] vertices = new VertexData[meshPart.VertexBuffer.VertexCount];

                    meshPart.VertexBuffer.GetData(vertices);

                    AddNormalsToPositionNormalDictionary(vertices, ref positionToNormals);

                }
            }
        }

        public virtual void GenerateSmoothNormalsFromDictionary(Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)> positionToNormals)
        {
            if (model == null) return;

            foreach (var mesh in model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                {

                    VertexData[] vertices = new VertexData[meshPart.NumVertices];

                    meshPart.VertexBuffer.GetData(vertices);

                    var data = GenerateSmoothNormalsForBuffer(vertices, positionToNormals);

                    meshPart.VertexBuffer.SetData(data, 0, data.Length);

                }
            }
        }

        public virtual void GenerateSmoothNormals()
        {

            Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)> positionToNormals = new Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)>();

            AddNormalsToPositionNormalDictionary(ref positionToNormals);

            GenerateSmoothNormalsFromDictionary(positionToNormals);
        }

        public static void AddNormalsToPositionNormalDictionary(VertexData[] vertices, ref Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)> positionToNormals)
        {
            // Iterate through each vertex in the index buffer
            for (int i = 0; i < vertices.Length; i++)
            {
                int index = i;
                Vector3 position = vertices[index].Position;
                Vector3 normal = vertices[index].Normal;

                if (positionToNormals.ContainsKey(position))
                {

                    var list = positionToNormals[position].existingNormals;

                    if (list.Contains(normal))
                        continue;

                    list.Add(normal);

                    positionToNormals[position] = (
                        positionToNormals[position].accumulatedNormal + normal,
                        list

                    );
                }
                else
                {
                    positionToNormals[position] = (normal, new List<Vector3> { normal });
                }
            }
        }

        public static VertexData[] GenerateSmoothNormalsForBuffer(VertexData[] vertices, Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)> positionToNormals)
        {

            // Assign the smooth normals to each vertex
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 position = vertices[i].Position;
                vertices[i].SmoothNormal = positionToNormals[position].accumulatedNormal.Normalized();
            }

            return vertices;
        }

        public static VertexData[] GenerateSmoothNormalsForBuffer(VertexData[] vertices)
        {
            // Create a dictionary to accumulate normals and count occurrences for each unique vertex position
            Dictionary<Vector3, (Vector3 accumulatedNormal, int count, List<Vector3> existingNormals)> positionToNormals = new Dictionary<Vector3, (Vector3, int, List<Vector3>)>();

            // Iterate through each vertex in the index buffer
            for (int i = 0; i < vertices.Length; i++)
            {
                int index = i;
                Vector3 position = vertices[index].Position;
                Vector3 normal = vertices[index].Normal;

                if (positionToNormals.ContainsKey(position))
                {

                    var list = positionToNormals[position].existingNormals;

                    if (list.Contains(normal))
                        continue;

                    list.Add(normal);

                    positionToNormals[position] = (
                        positionToNormals[position].accumulatedNormal + normal,
                        positionToNormals[position].count + 1,
                        list

                    );
                }
                else
                {
                    positionToNormals[position] = (normal, 1, new List<Vector3> { normal});
                }
            }

            // Calculate the average normal for each vertex position
            foreach (var kvp in positionToNormals)
            {

                //Console.WriteLine(kvp.Value.count);

                Vector3 averageNormal = kvp.Value.accumulatedNormal / kvp.Value.count;
                averageNormal.Normalize(); // Normalize the average normal
                positionToNormals[kvp.Key] = (averageNormal, kvp.Value.count, kvp.Value.existingNormals);
            }

            // Assign the smooth normals to each vertex
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 position = vertices[i].Position;
                vertices[i].SmoothNormal = positionToNormals[position].accumulatedNormal.Normalized();
            }

            return vertices;
        }


        public virtual void DrawGeometryShadow()
        {

            if (CastGeometricShadow == false) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.GeometryShadowEffect;





            if (frameStaticMeshData.model is not null)
            {

                var hit = Physics.LineTraceForStatic((Position - Graphics.LightDirection / 8).ToPhysics(), (Position + Graphics.LightDirection.Normalized() * 100).ToPhysics());

                if (hit.HasHit == false) return;

                Vector3 hitPoint = hit.HitPointWorld;

                if (hitPoint.Y > Position.Y)
                {
                    hitPoint = Position;
                    hitPoint.Y = hit.HitPointWorld.Y;
                }

                Vector3 normal = hit.HitNormalWorld;

                if (Vector3.Dot(hit.HitNormalWorld, Graphics.LightDirection.Normalized()) > -0.5)
                {
                    normal = Vector3.UnitY;
                }

                Plane plane = new Plane(hitPoint, normal);

                Matrix shadow = Matrix.CreateShadow(Graphics.LightDirection, plane);


                effect.Parameters["World"].SetValue(frameStaticMeshData.World * -shadow);

                effect.Techniques[0].Passes[0].Apply();

                if (frameStaticMeshData.model.Meshes is not null)
                    foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                    {

                        foreach (ModelMeshPart meshPart in mesh.MeshParts)
                        {

                            // Set the vertex buffer and index buffer for this mesh part
                            graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                            graphicsDevice.Indices = meshPart.IndexBuffer;


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

        List<string> nullTextures = new List<string>();

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
            name = name.Replace(".", $"{sufix}.");

            if (textures.ContainsKey(name))
                return textures[name];


            if (textureSearchPaths.Count > 0)
            {

                Texture2D output;
                if (nullTextures.Contains(name) == false)
                {
                    foreach (string item in textureSearchPaths)
                    {
                        output = AssetRegistry.LoadTextureFromFile(item + name, true);
                        if (output != null)
                        {
                            textures.TryAdd(name, output);
                            return output;
                        }
                    }

                    nullTextures.Add(name);

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

        public virtual Matrix GetWorldMatrix()
        {

            if (UseAlternativeRotationCalculation)
            {
                Matrix worldMatrix = Matrix.CreateScale(Scale) *
                            Matrix.CreateRotationZ(Rotation.Z / 180 * (float)Math.PI) *
                            Matrix.CreateRotationX(Rotation.X / 180 * (float)Math.PI) *
                            Matrix.CreateRotationY(Rotation.Y / 180 * (float)Math.PI) *
                            GetLocalRotationOffset() *
                            Matrix.CreateTranslation(Position);

                if (float.IsNaN(worldMatrix.M11))
                    return Matrix.Identity;

                return worldMatrix * GetLocalOffset();
            }
            else
            {

                Matrix worldMatrix = Matrix.CreateScale(Scale) *
                                Matrix.CreateRotationX(Rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(Rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(Rotation.Z / 180 * (float)Math.PI) *
                                GetLocalRotationOffset() *
                                Matrix.CreateTranslation(Position) * ParrentTransform;


                if (float.IsNaN(worldMatrix.M11))
                    return Matrix.Identity;

                return worldMatrix * GetLocalOffset();
            }
        }

        public virtual void LoadFromFile(string filePath)
        {
            model = GetModelFromPath(filePath);

            avgVertexPosition = CalculateAvgVertexLocation();
        }

        public static Dictionary<string, Assimp.Scene> loadedScenes = new Dictionary<string, Assimp.Scene>();
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
                using (var asset = AssetRegistry.GetFileStreamFromPath(filePath))
                {
                    scene = importer.ImportFileFromStream(asset.FileStream, Assimp.PostProcessSteps.MakeLeftHanded | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.CalculateTangentSpace | Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.FindDegenerates, formatHint: hint);
                }
                //loadedScenes.Add(filePath, scene);
            }

            while (loadedScenes.Keys.Count > 2)
            {
                loadedScenes.Remove(loadedScenes.Keys.First());
            }

            //scene.Materials[0].TextureSpecular.

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
                        Position = new Vector3(-vertex.X, vertex.Y, vertex.Z), // Negate x-coordinate
                        Normal = new Vector3(-normal.X, normal.Y, normal.Z),
                        TextureCoordinate = new Vector2(textureCoord.X, textureCoord.Y),
                        Tangent = new Vector3(-tangent.X, tangent.Y, tangent.Z),
                        BiTangent = new Vector3(-BiTangent.X, BiTangent.Y, BiTangent.Z)
                    });
                }


                VertexBuffer vertexBuffer;

                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexData), vertices.Count, BufferUsage.None);

                vertices = GenerateSmoothNormalsForBuffer(vertices.ToArray()).ToList();

                vertexBuffer.SetData(vertices.ToArray());
                var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.None);
                indexBuffer.SetData(indices.ToArray());

                int numFaces = mesh.FaceCount;
                int primitiveCount = numFaces * 3;  // Each face is a triangle with 3 vertices


                boundingSphere = CalculateBoundingSphere(vertices.ToArray());


                meshParts.Add(new ModelMeshPart { VertexBuffer = vertexBuffer, IndexBuffer = indexBuffer, StartIndex = 0, NumVertices = indices.Count, PrimitiveCount = primitiveCount, Tag = new MeshPartData { textureName = Path.GetFileName(scene.Materials[mesh.MaterialIndex].TextureDiffuse.FilePath), Points = points, Name = mesh.Name } });
            }


            modelMesh.Add(new ModelMesh(graphicsDevice, meshParts) { BoundingSphere = boundingSphere });

            Model model = new Model(graphicsDevice, new List<ModelBone>(), modelMesh);

            model.Tag = filePath;

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
            frameStaticMeshData.LightViewmodelView = Graphics.LightViewmodelView;

            frameStaticMeshData.LightViewmodelProjection = Graphics.LightViewmodelProjection;
            frameStaticMeshData.LightProjection = Graphics.LightProjection;
            frameStaticMeshData.LightProjectionClose = Graphics.LightCloseProjection;
            frameStaticMeshData.LightProjectionVeryClose = Graphics.LightVeryCloseProjection;

            frameStaticMeshData.Transparency = Transparency;
            frameStaticMeshData.IsRendered = isRendered;
            frameStaticMeshData.IsRenderedShadow = isRenderedShadow;
            frameStaticMeshData.InFrustrum = inFrustrum;

            finalizedMeshHide = MeshHideList.ToArray();

            if (DitherDisolve > 0)
                Masked = true;

        }

        public virtual void UpdateCulling()
        {
            isRendered = false;
            isRenderedShadow = true;
            inFrustrum = false;

            if (Visible == false) return;

            if (model is null) return;

            WorldMatrix = GetWorldMatrix();

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
            //Dispose();
            
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
                    part.Tag = null;
                }
        }

    }
}
