using RetroEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using RetroEngine.Particles;

namespace RetroEngine
{
    public class Render
    {

        RenderTarget2D colorPath;
        RenderTarget2D emissivePath;
        RenderTarget2D normalPath;
        RenderTarget2D positionPath;
        RenderTarget2D depthPath;

        RenderTarget2D DeferredOutput;
        RenderTarget2D DepthPrepathOutput;

        RenderTarget2D ForwardOutput;
        RenderTarget2D ForwardDepth;

        RenderTarget2D miscPath;
        RenderTarget2D postProcessingOutput;
        public RenderTarget2D DepthOutput;

        public RenderTarget2D shadowMap;
        public RenderTarget2D shadowMapClose;

        public Texture2D black;

        RenderTarget2D ssaoOutput;
        RenderTarget2D ComposedOutput;
        RenderTarget2D FxaaOutput;

        RenderTarget2D bloomSample;
        RenderTarget2D bloomSample2;
        RenderTarget2D bloomSample3;

        RenderTarget2D outputPath;

        RenderTarget2D occlusionTestPath;

        GraphicsDeviceManager graphics;

        Effect lightingEffect;

        public Effect BuffersEffect;
        public Effect DeferredEffect;

        public Effect OcclusionEffect;

        public Effect ColorEffect;
        public Effect ParticleColorEffect;
        public Effect NormalEffect;
        public Effect MiscEffect;

        public Effect ShadowMapEffect;

        public Effect fxaaEffect;

        public Effect maxDepth;

        public Effect PostProcessingEffect;

        public Effect SSAOEffect;

        public Effect BloomEffect;

        public Effect ComposeEffect;

        public Delay shadowPassRenderDelay = new Delay();

        public List<ParticleEmitter.Particle> particlesToDraw = new List<ParticleEmitter.Particle>();

        SamplerState samplerState = new SamplerState();

        public static OcclusionQuery occlusionQuery;

        public static float ResolutionScale = 1f;

        bool dirtySampler = true;

        public Render()
        {
            graphics = GameMain.Instance._graphics;

            //lightingEffect = GameMain.content.Load<Effect>("DeferredLighting");
            //NormalEffect = GameMain.content.Load<Effect>("NormalOutput");
            //MiscEffect = GameMain.content.Load<Effect>("MiscOutput");


            ShadowMapEffect = GameMain.content.Load<Effect>("ShadowMap");
            fxaaEffect = GameMain.content.Load<Effect>("fxaa");
            maxDepth = GameMain.content.Load<Effect>("maxDepth");
            //PostProcessingEffect = GameMain.content.Load<Effect>("PostProcessing");
            //ColorEffect = GameMain.content.Load<Effect>("ColorOutput");
            //ParticleColorEffect = GameMain.content.Load<Effect>("ParticleColorOutput");
            SSAOEffect = GameMain.content.Load<Effect>("ssao");
            BuffersEffect = GameMain.content.Load<Effect>("GPathesOutput");

            DeferredEffect = GameMain.content.Load<Effect>("DeferredShading");

            ComposeEffect = GameMain.content.Load<Effect>("ComposedColor");

            BloomEffect = GameMain.content.Load<Effect>("BloomSampler");

            OcclusionEffect = GameMain.content.Load<Effect>("OcclusionPath");

            occlusionQuery = new OcclusionQuery(GameMain.Instance.GraphicsDevice);

            InitSampler();
        }

        public void UpdateShaderFrameData()
        {
            var shaders = AssetRegistry.GetAllShaders();

            foreach (Effect effect in shaders)
            {

                effect.Parameters["viewDir"]?.SetValue(Camera.finalizedForward);
                effect.Parameters["viewPos"]?.SetValue(Camera.finalizedPosition);

                effect.Parameters["DirectBrightness"]?.SetValue(Graphics.DirectLighting);
                effect.Parameters["GlobalBrightness"]?.SetValue(Graphics.GlobalLighting);
                effect.Parameters["LightDirection"]?.SetValue(Graphics.LightDirection.Normalized());

                effect.Parameters["ShadowMapViewProjection"]?.SetValue(Graphics.LightViewProjection);
                effect.Parameters["ShadowMapViewProjectionClose"]?.SetValue(Graphics.LightViewProjectionClose);

                effect.Parameters["ShadowBias"]?.SetValue(Graphics.ShadowBias);
                effect.Parameters["ShadowMapResolution"]?.SetValue((float)Graphics.shadowMapResolution);

                effect.Parameters["ShadowMap"]?.SetValue(GameMain.Instance.render.shadowMap);


                effect.Parameters["View"]?.SetValue(Camera.finalizedView);
                effect.Parameters["Projection"]?.SetValue(Camera.finalizedProjection);
                effect.Parameters["ProjectionViewmodel"]?.SetValue(Camera.finalizedProjectionViewmodel);

                effect.Parameters["GlobalLightColor"]?.SetValue(Graphics.LightColor);

                if (Graphics.GlobalPointLights)
                {
                    Vector3[] LightPos = new Vector3[LightManager.MAX_POINT_LIGHTS];
                    Vector3[] LightColor = new Vector3[LightManager.MAX_POINT_LIGHTS];
                    float[] LightRadius = new float[LightManager.MAX_POINT_LIGHTS];

                    for (int i = 0; i < LightManager.MAX_POINT_LIGHTS; i++)
                    {
                        LightPos[i] = LightManager.FinalPointLights[i].Position;
                        LightColor[i] = LightManager.FinalPointLights[i].Color;
                        LightRadius[i] = LightManager.FinalPointLights[i].Radius;
                    }

                    effect.Parameters["LightPositions"]?.SetValue(LightPos);
                    effect.Parameters["LightColors"]?.SetValue(LightColor);
                    effect.Parameters["LightRadiuses"]?.SetValue(LightRadius);
                }

                effect.Parameters["DepthTexture"]?.SetValue(DepthPrepathOutput);
            }
        }
        public RenderTarget2D StartRenderLevel(Level level)
        {
            
            

            CreateBlackTexture();

            InitRenderTargetDepth(ref DepthOutput);

            InitRenderTargetDepth(ref DepthPrepathOutput);

            InitRenderTargetVectorIfNeed(ref DeferredOutput);

            InitRenderTargetIfNeed(ref normalPath);

            InitRenderTargetIfNeed(ref ComposedOutput);

            InitRenderTargetIfNeed(ref FxaaOutput);


            InitSizedRenderTargetIfNeed(ref ssaoOutput,512);

            InitSizedRenderTargetIfNeed(ref bloomSample, 64);
            InitSizedRenderTargetIfNeed(ref bloomSample2, 32);
            InitSizedRenderTargetIfNeed(ref bloomSample3, 16);

            InitRenderTargetIfNeed(ref postProcessingOutput);


            InitShadowMap(ref shadowMap);

            List<StaticMesh> renderList = level.GetMeshesToRender();

            

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            
            RenderShadowMap(renderList);

            graphics.GraphicsDevice.RasterizerState = Graphics.DisableBackFaceCulling? RasterizerState.CullNone : RasterizerState.CullClockwise;

            InitSampler(5);

            EndOcclusionTest(renderList);
            RenderPrepass(renderList);
            

            RenderForwardPath(renderList);
            

            graphics.GraphicsDevice.BlendState = BlendState.Opaque;


            PerformPostProcessing();

            graphics.GraphicsDevice.SetRenderTarget(null);

            if(Input.GetAction("test2").Holding())
                return DepthPrepathOutput;

            return outputPath;

        }

        public void InitSampler(int max = 10)
        {


            samplerState = new SamplerState();

            samplerState.Filter = Graphics.TextureFiltration ? (Graphics.AnisotropicFiltration ? TextureFilter.Anisotropic : TextureFilter.Linear) : TextureFilter.PointMipLinear;

            samplerState.AddressU = TextureAddressMode.Wrap;
            samplerState.AddressV = TextureAddressMode.Wrap;
            samplerState.AddressW = TextureAddressMode.Wrap;

            samplerState.MipMapLevelOfDetailBias = -2;


            int i = 0;
            while (i<=max)
            {
                try
                {
                    graphics.GraphicsDevice.SamplerStates[i] = samplerState;
                    i++;

                    if (i > 10) break;

                }catch(Exception e) { break; }
            }
                

        }

        void RenderForwardPath(List<StaticMesh> renderList, bool onlyTransperent = false)
        {

            Stats.RenderedMehses = 0;

            

            UpdateShaderFrameData();

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, (int)GetScreenResolution().X, (int)GetScreenResolution().Y);

            graphics.GraphicsDevice.SetRenderTargets(DeferredOutput,DepthOutput, normalPath);
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphics.GraphicsDevice.Clear(Graphics.BackgroundColor);



            //particlesToDraw.Clear();

            ParticleEmitter.LoadRenderEmitter();

            foreach (StaticMesh mesh in renderList)
            {
                if (mesh.Transperent || onlyTransperent == false)
                {
                    try
                    {
                        mesh.DrawUnified();

                    } catch(Exception e) { }
                }
            }

            

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            //ParticleEmitter.RenderEmitter.DrawParticles(particlesToDraw);

            if (Graphics.DrawPhysics)
                Physics.DebugDraw();

        }

        void RenderPrepass(List<StaticMesh> renderList)
        {
            UpdateShaderFrameData();

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, (int)GetScreenResolution().X, (int)GetScreenResolution().Y);

            graphics.GraphicsDevice.SetRenderTargets(DepthPrepathOutput);
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: maxDepth);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, black);

            // End the SpriteBatch
            spriteBatch.End();

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            graphics.GraphicsDevice.BlendState = BlendState.Opaque;


            OcclusionEffect.Parameters["View"].SetValue(Camera.finalizedView);
            OcclusionEffect.Parameters["Projection"].SetValue(Camera.projection);


            foreach (StaticMesh mesh in renderList)
            {
                if (mesh.Transperent == false)
                {
                    try
                    {
                        mesh.StartOcclusionTest();

                    }
                    catch (Exception e) { }
                }
            }
        }

        public bool renderShadow()
        {
            return !shadowPassRenderDelay.Wait();
        }

        void RenderShadowMap(List<StaticMesh> renderList)
        {
            if (renderShadow() == false) return;

            shadowPassRenderDelay.AddDelay(0.05f);

            // Set up the shadow map render target with the desired resolution
            graphics.GraphicsDevice.SetRenderTarget(shadowMap);
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Graphics.shadowMapResolution, Graphics.shadowMapResolution);

            // Clear the shadow map with the desired clear color (e.g., Color.White)
            graphics.GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: maxDepth, sortMode: SpriteSortMode.FrontToBack);

            // Draw a full-screen quad to apply the lighting
            DrawShadowQuad(spriteBatch, black);

            // End the SpriteBatch
            spriteBatch.End();

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            // Iterate through meshes and draw shadows
            foreach (StaticMesh mesh in renderList)
            {
                mesh.DrawShadow();
            }

            return;
            // Set up the shadow map render target with the desired resolution
            graphics.GraphicsDevice.SetRenderTarget(shadowMapClose);
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Graphics.closeShadowMapResolution, Graphics.closeShadowMapResolution);

            // Clear the shadow map with the desired clear color (e.g., Color.White)
            graphics.GraphicsDevice.Clear(Color.White);

            // Set depth stencil and rasterizer states
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Iterate through meshes and draw shadows
            foreach (StaticMesh mesh in renderList)
            {
                if (mesh.isRendered)
                {
                    mesh.DrawShadow(true);
                }

            }

            // Reset the render target and viewport to the back buffer's dimensions
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, (int)GetScreenResolution().X, (int)GetScreenResolution().Y);

        }

        void PerformPostProcessing()
        {
            PerformSSAO();

            
            CalculateBloom();


            PerformCompose();

            PerformFXAA();


            return;
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, (int)GetScreenResolution().X, (int)GetScreenResolution().Y);

            graphics.GraphicsDevice.SetRenderTarget(postProcessingOutput);

            PostProcessingEffect.Parameters["ColorTexture"].SetValue(colorPath);
            //PostProcessingEffect.Parameters["Enabled"].SetValue(Graphics.EnablePostPocessing);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: PostProcessingEffect);

            DrawFullScreenQuad(spriteBatch, colorPath);

            spriteBatch.End();

                

        }

        void PerformSSAO()
        {

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, ssaoOutput.Width, ssaoOutput.Height);

            graphics.GraphicsDevice.SetRenderTarget(ssaoOutput);

            SSAOEffect.Parameters["NormalTexture"]?.SetValue(normalPath);
            SSAOEffect.Parameters["DepthTexture"]?.SetValue(DepthPrepathOutput);
            SSAOEffect.Parameters["screenWidth"]?.SetValue(ssaoOutput.Width);
            SSAOEffect.Parameters["screenHeight"]?.SetValue(ssaoOutput.Height);
            SSAOEffect.Parameters["ssaoRadius"]?.SetValue(10);
            SSAOEffect.Parameters["ssaoBias"]?.SetValue(0.001f);
            SSAOEffect.Parameters["ssaoIntensity"]?.SetValue(7);


            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: SSAOEffect, blendState: BlendState.Opaque);

            DrawFullScreenQuad(spriteBatch, DepthOutput);

            spriteBatch.End();
        }

        void PerformFXAA()
        {

            if (Graphics.EnableAntiAliasing == false)
            {
                outputPath = ComposedOutput;


                return;
            }

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, FxaaOutput.Width, FxaaOutput.Height);

            // Set the render target to the output path
            graphics.GraphicsDevice.SetRenderTarget(FxaaOutput);
            

            float fxaaQualitySubpix = 0.75f;
            float fxaaQualityEdgeThreshold = 0.066f;
            float fxaaQualityEdgeThresholdMin = 0.0833f;

            fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_PC"];

            fxaaEffect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
            fxaaEffect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
            fxaaEffect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);

            fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / ComposedOutput.Width);
            fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / ComposedOutput.Height);
            fxaaEffect.Parameters["screenColor"].SetValue(ComposedOutput);

            // Begin drawing with SpriteBatch
            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: fxaaEffect, blendState: BlendState.Opaque);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, ComposedOutput);

            // End the SpriteBatch
            spriteBatch.End();

            outputPath = FxaaOutput;

        }

        void PerformCompose()
        {
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, (int)GetScreenResolution().X, (int)GetScreenResolution().Y);

            graphics.GraphicsDevice.SetRenderTarget(ComposedOutput);

            ComposeEffect.Parameters["ColorTexture"].SetValue(DeferredOutput);
            ComposeEffect.Parameters["SSAOTexture"]?.SetValue(ssaoOutput);
            ComposeEffect.Parameters["BloomTexture"].SetValue(bloomSample);
            ComposeEffect.Parameters["Bloom2Texture"].SetValue(bloomSample2);
            ComposeEffect.Parameters["Bloom3Texture"].SetValue(bloomSample3);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: ComposeEffect, blendState: BlendState.Opaque);

            DrawFullScreenQuad(spriteBatch, DeferredOutput);

            spriteBatch.End();
        }

        void CalculateBloom()
        {

            if (Graphics.EnableBloom == false) return;

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, bloomSample.Width, bloomSample.Height);
            graphics.GraphicsDevice.SetRenderTarget(bloomSample);

            BloomEffect.Parameters["screenWidth"].SetValue(bloomSample.Width);
            BloomEffect.Parameters["screenHeight"].SetValue(bloomSample.Height);
            BloomEffect.Parameters["offset"].SetValue(0.75f);


            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: BloomEffect, blendState: BlendState.Opaque);

            DrawFullScreenQuad(spriteBatch, DeferredOutput);

            spriteBatch.End();

            DownsampleToTexture(bloomSample, bloomSample2);
            DownsampleToTexture(bloomSample, bloomSample3);

        }
        public static bool performingOcclusionTest = false;
        public void PerformOcclusionTest(List<StaticMesh> meshes)
        {
            performingOcclusionTest = true;

            InitRenderTargetIfNeed(ref occlusionTestPath, DepthFormat.Depth16);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, occlusionTestPath.Width, occlusionTestPath.Height);

            OcclusionEffect.Parameters["View"].SetValue(Camera.finalizedView);
            OcclusionEffect.Parameters["Projection"].SetValue(Camera.projectionOcclusion);

            GameMain.Instance.WaitForFramePresent();

            graphics.GraphicsDevice.SetRenderTarget(occlusionTestPath);
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphics.GraphicsDevice.Clear(Color.Black);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            foreach (StaticMesh mesh in meshes)
            {
                mesh.StartOcclusionTest();
            }

            
        }

        public void EndOcclusionTest(List<StaticMesh> meshes)
        {
            foreach (StaticMesh mesh in meshes)
            {
                if(mesh.Transperent==false)
                    mesh.EndOcclusionTest();
            }

            performingOcclusionTest = false;

        }

            void DownsampleToTexture(Texture2D source, RenderTarget2D target)
        {
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, target.Width, target.Height);
            graphics.GraphicsDevice.SetRenderTarget(target);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(blendState: BlendState.Opaque);

            DrawFullScreenQuad(spriteBatch, source);

            spriteBatch.End();

        }

        void DrawFullScreenQuad(SpriteBatch spriteBatch, Texture2D inputTexture)
        {
            // Create a rectangle covering the entire screen
            Rectangle screenRectangle = new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            // Draw the full-screen quad using SpriteBatch
            spriteBatch.Draw(inputTexture, screenRectangle, Color.White);
        }

        void DrawShadowQuad(SpriteBatch spriteBatch, Texture2D inputTexture)
        {
            // Create a rectangle covering the entire screen
            Rectangle screenRectangle = new Rectangle(0, 0, shadowMap.Width, shadowMap.Height);

            // Draw the full-screen quad using SpriteBatch
            spriteBatch.Draw(inputTexture, screenRectangle, Color.White);
        }

        void InitShadowMap(ref RenderTarget2D target)
        {
            if (shadowMap is not null && shadowMap.Height == Graphics.shadowMapResolution) return;

            DestroyRenderTarget(shadowMap);

            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth16;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                Graphics.shadowMapResolution,
                Graphics.shadowMapResolution,
                false, // No mipmaps
                SurfaceFormat.Single, // Color format
                depthFormat); // Depth format
        }

        void InitOcclusionMap(ref RenderTarget2D target)
        {
            if (target is not null) return;

            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth16;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                480,
                480,
                false, // No mipmaps
                SurfaceFormat.HalfSingle, // Color format
                depthFormat); // Depth format
        }

        void InitCloseShadowMap(ref RenderTarget2D target)
        {
            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth16;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                Graphics.closeShadowMapResolution,
                Graphics.closeShadowMapResolution,
                false, // No mipmaps
                SurfaceFormat.Single, // Color format
                depthFormat); // Depth format
        }

        void InitRenderTargetIfNeed(ref RenderTarget2D target, DepthFormat depthFormat = DepthFormat.None)
        {
            if(GetScreenResolution().X>0 && GetScreenResolution().Y > 0)

            if (target is null || target.Width != (int)GetScreenResolution().X || target.Height != (int)GetScreenResolution().Y)
            {

                    DestroyRenderTarget(target);
                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    (int)GetScreenResolution().X,
                    (int)GetScreenResolution().Y,
                    false, // No mipmaps
                    SurfaceFormat.Rgba64, // Color format
                    depthFormat); // Depth format
            }
        }

        void InitRenderTargetDepth(ref RenderTarget2D target, DepthFormat depthFormat = DepthFormat.Depth24)
        {
            if (GetScreenResolution().X > 0 && GetScreenResolution().Y > 0)

                if (target is null || target.Width != (int)GetScreenResolution().X || target.Height != (int)GetScreenResolution().Y)
                {

                    DestroyRenderTarget(target);
                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    (int)GetScreenResolution().X,
                    (int)GetScreenResolution().Y,
                    false, // No mipmaps
                    SurfaceFormat.Single, // Color format
                    depthFormat); // Depth format
                }
        }

        void InitSizedRenderTargetIfNeed(ref RenderTarget2D target, float height, DepthFormat depthFormat = DepthFormat.None)
        {

            float ratio = ((float)GetScreenResolution().X) / ((float)GetScreenResolution().Y);

            int width = (int)(height * ratio);

            if (width > 0 && height > 0)

                if (target is null || target.Width != width || target.Height != height)
                {
                    DestroyRenderTarget(target);

                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                        graphics.GraphicsDevice,
                        width,
                        (int)height,
                        false, // No mipmaps
                        SurfaceFormat.Rgba64, // Color format
                        depthFormat); // Depth format
                }
        }

        void InitVectorRenderTargetIfNeed(ref RenderTarget2D target)
        {
            if (GetScreenResolution().X > 0 && GetScreenResolution().Y > 0)

                if (target is null || target.Width != (int)GetScreenResolution().X || target.Height != (int)GetScreenResolution().Y)
                {
                    DestroyRenderTarget(target);

                    // Set the depth format based on your requirements
                    DepthFormat depthFormat = DepthFormat.Depth16;

                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                        graphics.GraphicsDevice,
                        (int)GetScreenResolution().X,
                        (int)GetScreenResolution().Y,
                        false, // No mipmaps
                        SurfaceFormat.HalfVector4, // Color format
                        depthFormat); // Depth format
                }
        }

        void InitRenderTargetVectorIfNeed(ref RenderTarget2D target)
        {
            if (GetScreenResolution().X > 0 && GetScreenResolution().Y > 0)
                if (target is null || target.Width != (int)GetScreenResolution().X || target.Height != (int)GetScreenResolution().Y)
                {

                    DestroyRenderTarget(target);

                    // Set the depth format based on your requirements
                    DepthFormat depthFormat = DepthFormat.Depth24;

                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                        graphics.GraphicsDevice,
                        (int)GetScreenResolution().X,
                        (int)GetScreenResolution().Y,
                        false, // No mipmaps
                        SurfaceFormat.HalfVector4, // Color format
                        depthFormat,0,RenderTargetUsage.PreserveContents); // Depth format



                }

            graphics.ApplyChanges();
            

        }

        static List<GraphicsResource> destroyList = new List<GraphicsResource>();
        public static void DestroyPending()
        {
            foreach(var resource in destroyList)
            {
                resource.Dispose();
            }
        }

        void DestroyRenderTarget(RenderTarget2D target)
        {
            if (target != null)
            {

                destroyList.Add(target);

            }
        }

        public static Effect LoadEffect(string path, GraphicsDevice gd)
        {

            path = AssetRegistry.FindPathForFile(path);

            // check if file exists
            if (!File.Exists(path) || path == null)
            {
                return null;
            }

            Effect effect;
            using (BinaryReader b = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                effect = new Effect(gd, b.ReadBytes((int)b.BaseStream.Length));
            }

            return effect;
        }

        public Vector2 GetScreenResolution()
        {
            return new Vector2(graphics.PreferredBackBufferWidth * ResolutionScale, graphics.PreferredBackBufferHeight * ResolutionScale);
        }
        void CreateBlackTexture()
        {
            if (black != null) return;

            // Create a 1x1 black texture
            black = new Texture2D(graphics.GraphicsDevice, 1, 1);
            Color[] data = new Color[1] { Color.Black };
            black.SetData(data);
        }

    }
}
