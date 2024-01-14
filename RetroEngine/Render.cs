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

        public Effect UnifiedEffect;
        public Effect BuffersEffect;
        public Effect DeferredEffect;

        public Effect ColorEffect;
        public Effect ParticleColorEffect;
        public Effect NormalEffect;
        public Effect MiscEffect;

        public Effect ShadowMapEffect;

        public Effect fxaaEffect;

        public Effect PostProcessingEffect;

        public Effect SSAOEffect;

        public Effect BloomEffect;

        public Effect ComposeEffect;

        public Delay shadowPassRenderDelay = new Delay();

        public List<ParticleEmitter.Particle> particlesToDraw = new List<ParticleEmitter.Particle>();

        SamplerState samplerState = new SamplerState();

        public static OcclusionQuery occlusionQuery;

        public Render()
        {
            graphics = GameMain.Instance._graphics;

            //lightingEffect = GameMain.content.Load<Effect>("DeferredLighting");
            //NormalEffect = GameMain.content.Load<Effect>("NormalOutput");
            //MiscEffect = GameMain.content.Load<Effect>("MiscOutput");


            ShadowMapEffect = GameMain.content.Load<Effect>("ShadowMap");
            UnifiedEffect = GameMain.content.Load<Effect>("UnifiedOutput");
            fxaaEffect = GameMain.content.Load<Effect>("fxaa");
            //PostProcessingEffect = GameMain.content.Load<Effect>("PostProcessing");
            //ColorEffect = GameMain.content.Load<Effect>("ColorOutput");
            //ParticleColorEffect = GameMain.content.Load<Effect>("ParticleColorOutput");
            SSAOEffect = GameMain.content.Load<Effect>("ssao");
            BuffersEffect = GameMain.content.Load<Effect>("GPathesOutput");

            DeferredEffect = GameMain.content.Load<Effect>("DeferredShading");

            ComposeEffect = GameMain.content.Load<Effect>("ComposedColor");

            BloomEffect = GameMain.content.Load<Effect>("BloomSampler");

            occlusionQuery = new OcclusionQuery(GameMain.Instance.GraphicsDevice);

            InitSampler();
        }

        public void UpdateShaderFrameData()
        {

            Effect effect = UnifiedEffect;

            effect.Parameters["viewDir"]?.SetValue(Camera.rotation.GetForwardVector());
            effect.Parameters["viewPos"]?.SetValue(Camera.position);

            effect.Parameters["DirectBrightness"]?.SetValue(Graphics.DirectLighting);
            effect.Parameters["GlobalBrightness"]?.SetValue(Graphics.GlobalLighting);
            effect.Parameters["LightDirection"]?.SetValue(Graphics.LightDirection.Normalized());

            effect.Parameters["ShadowMapViewProjection"]?.SetValue(Graphics.LightViewProjection);
            effect.Parameters["ShadowMapViewProjectionClose"]?.SetValue(Graphics.LightViewProjectionClose);

            effect.Parameters["ShadowBias"]?.SetValue(Graphics.ShadowBias);
            effect.Parameters["ShadowMapResolution"]?.SetValue((float)Graphics.shadowMapResolution);


            effect.Parameters["View"]?.SetValue(Camera.finalizedView);
            effect.Parameters["Projection"]?.SetValue(Camera.finalizedProjection);
            effect.Parameters["ProjectionViewmodel"]?.SetValue(Camera.finalizedProjectionViewmodel);


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
        public RenderTarget2D StartRenderLevel(Level level)
        {
            

            CreateBlackTexture();

            InitRenderTargetIfNeed(ref DepthOutput);

            InitRenderTargetVectorIfNeed(ref DeferredOutput);

            InitRenderTargetIfNeed(ref normalPath);

            InitRenderTargetIfNeed(ref ComposedOutput);

            InitRenderTargetIfNeed(ref FxaaOutput);

            InitRenderTargetIfNeed(ref outputPath);

            InitSizedRenderTargetIfNeed(ref ssaoOutput, 128);

            InitSizedRenderTargetIfNeed(ref bloomSample, 64);
            InitSizedRenderTargetIfNeed(ref bloomSample2, 32);
            InitSizedRenderTargetIfNeed(ref bloomSample3, 16);

            InitRenderTargetIfNeed(ref postProcessingOutput);


            InitShadowMap(ref shadowMap);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            

            List<StaticMesh> renderList = level.GetMeshesToRender();



            RenderShadowMap(renderList);

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;


            RenderForwardPath(renderList);

            //graphics.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            graphics.GraphicsDevice.SamplerStates[1] = SamplerState.AnisotropicClamp;
            graphics.GraphicsDevice.SamplerStates[2] = SamplerState.AnisotropicClamp;
            graphics.GraphicsDevice.SamplerStates[3] = SamplerState.AnisotropicClamp;
            graphics.GraphicsDevice.SamplerStates[4] = SamplerState.AnisotropicClamp;

            PerformPostProcessing();

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

            InitSampler(5);

            UpdateShaderFrameData();

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            graphics.GraphicsDevice.SetRenderTargets(DeferredOutput,DepthOutput, normalPath);
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphics.GraphicsDevice.Clear(Graphics.BackgroundColor);


            UnifiedEffect.Parameters["GlobalLightColor"]?.SetValue(Graphics.LightColor);


            particlesToDraw.Clear();


            foreach (StaticMesh mesh in renderList)
            {
                if (mesh.Transperent || onlyTransperent == false)
                {
                    mesh.DrawUnified();
                }
            }

            ParticleEmitter.LoadRenderEmitter();

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            ParticleEmitter.RenderEmitter.DrawParticles(particlesToDraw);

            
        }

        void RenderShadowMap(List<StaticMesh> renderList)
        {

            if (shadowPassRenderDelay.Wait()) return;

            shadowPassRenderDelay.AddDelay(1000.05f);

            // Set up the shadow map render target with the desired resolution
            graphics.GraphicsDevice.SetRenderTarget(shadowMap);
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Graphics.shadowMapResolution, Graphics.shadowMapResolution);

            // Clear the shadow map with the desired clear color (e.g., Color.White)
            graphics.GraphicsDevice.Clear(Color.White);

            // Iterate through meshes and draw shadows
            foreach (StaticMesh mesh in renderList)
            {
                mesh.DrawShadow();
            }

            UnifiedEffect.Parameters["ShadowMap"]?.SetValue(GameMain.Instance.render.shadowMap);

            UnifiedEffect.Parameters["ShadowBias"]?.SetValue(Graphics.ShadowBias);
            UnifiedEffect.Parameters["ShadowMapResolution"]?.SetValue((float)Graphics.shadowMapResolution);

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
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

        }

        void PerformPostProcessing()
        {
            //PerformSSAO();

            
            CalculateBloom();


            PerformCompose();

            PerformFXAA();


            return;
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

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

            SSAOEffect.Parameters["ColorTexture"].SetValue(DeferredOutput);
            SSAOEffect.Parameters["NormalTexture"].SetValue(normalPath);
            SSAOEffect.Parameters["DepthTexture"].SetValue(DepthOutput);
            SSAOEffect.Parameters["screenWidth"].SetValue(ssaoOutput.Width/2);
            SSAOEffect.Parameters["screenHeight"].SetValue(ssaoOutput.Height/2);
            SSAOEffect.Parameters["ssaoRadius"].SetValue(1f);
            //SSAOEffect.Parameters["ssaoBias"].SetValue(0.000001f);
            SSAOEffect.Parameters["ssaoIntensity"].SetValue(10f);


            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: SSAOEffect);

            DrawFullScreenQuad(spriteBatch, DeferredOutput);

            spriteBatch.End();
        }

        void PerformFXAA()
        {

            if (Graphics.EnableAntiAliasing == false)
            {
                outputPath = ComposedOutput;

                Console.WriteLine("AA disabled");

                return;
            }

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, FxaaOutput.Width, FxaaOutput.Height);

            // Set the render target to the output path
            graphics.GraphicsDevice.SetRenderTarget(FxaaOutput);
            

            float fxaaQualitySubpix = 0.75f;
            float fxaaQualityEdgeThreshold = 0.166f;
            float fxaaQualityEdgeThresholdMin = 0.0833f;

            fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_PC"];

            fxaaEffect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
            fxaaEffect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
            fxaaEffect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);

            fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / graphics.PreferredBackBufferWidth);
            fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / graphics.PreferredBackBufferHeight);
            fxaaEffect.Parameters["screenColor"].SetValue(ComposedOutput);

            // Begin drawing with SpriteBatch
            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: fxaaEffect);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, ComposedOutput);

            // End the SpriteBatch
            spriteBatch.End();

            outputPath = FxaaOutput;

        }

        void PerformCompose()
        {
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            graphics.GraphicsDevice.SetRenderTarget(ComposedOutput);

            ComposeEffect.Parameters["ColorTexture"].SetValue(DeferredOutput);
            ComposeEffect.Parameters["SSAOTexture"]?.SetValue(ssaoOutput);
            ComposeEffect.Parameters["BloomTexture"].SetValue(bloomSample);
            ComposeEffect.Parameters["Bloom2Texture"].SetValue(bloomSample2);
            ComposeEffect.Parameters["Bloom3Texture"].SetValue(bloomSample3);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: ComposeEffect);

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

            spriteBatch.Begin(effect: BloomEffect);

            DrawFullScreenQuad(spriteBatch, DeferredOutput);

            spriteBatch.End();

            DownsampleToTexture(bloomSample, bloomSample2);
            DownsampleToTexture(bloomSample, bloomSample3);

        }
        public static bool performingOcclusionTest = false;
        public void PerformOcclusionTest(List<StaticMesh> meshes)
        {
            performingOcclusionTest = true;
            Stats.StartRecord("occlusion test");


            InitOcclusionMap(ref occlusionTestPath);

            graphics.GraphicsDevice.SetRenderTarget(occlusionTestPath);
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphics.GraphicsDevice.Clear(Color.Black);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            foreach (StaticMesh mesh in meshes)
            {
                mesh.StartOcclusionTest();
            }

            foreach (StaticMesh mesh in meshes)
            {
                mesh.EndOcclusionTest();
            }

            Stats.StopRecord("occlusion test");
            performingOcclusionTest = false;
        }

        void DownsampleToTexture(Texture2D source, RenderTarget2D target)
        {
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, target.Width, target.Height);
            graphics.GraphicsDevice.SetRenderTarget(target);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin();

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

        void InitShadowMap(ref RenderTarget2D target)
        {
            if (shadowMap is not null && shadowMap.Height == Graphics.shadowMapResolution) return;

            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth16;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                Graphics.shadowMapResolution,
                Graphics.shadowMapResolution,
                false, // No mipmaps
                SurfaceFormat.HalfSingle, // Color format
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
                1280,
                720,
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
            if(graphics.PreferredBackBufferWidth>0 && graphics.PreferredBackBufferHeight>0)

            if (target is null || target.Width != graphics.PreferredBackBufferWidth || target.Height != graphics.PreferredBackBufferHeight)
            {
                // Dispose of the old render target if it exists
                target?.Dispose();


                // Create the new render target with the specified depth format
                target = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    graphics.PreferredBackBufferWidth,
                    graphics.PreferredBackBufferHeight,
                    false, // No mipmaps
                    SurfaceFormat.Rgba64, // Color format
                    depthFormat); // Depth format
            }
        }

        void InitSizedRenderTargetIfNeed(ref RenderTarget2D target, float height, DepthFormat depthFormat = DepthFormat.None)
        {

            float ratio = ((float)graphics.PreferredBackBufferWidth) / ((float)graphics.PreferredBackBufferHeight);

            int width = (int)(height * ratio);

            if (width > 0 && height > 0)

                if (target is null || target.Width != width || target.Height != height)
                {
                    // Dispose of the old render target if it exists
                    target?.Dispose();


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
            if (graphics.PreferredBackBufferWidth > 0 && graphics.PreferredBackBufferHeight > 0)

                if (target is null || target.Width != graphics.PreferredBackBufferWidth || target.Height != graphics.PreferredBackBufferHeight)
                {
                    // Dispose of the old render target if it exists
                    target?.Dispose();

                    // Set the depth format based on your requirements
                    DepthFormat depthFormat = DepthFormat.Depth16;

                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                        graphics.GraphicsDevice,
                        graphics.PreferredBackBufferWidth,
                        graphics.PreferredBackBufferHeight,
                        false, // No mipmaps
                        SurfaceFormat.HalfVector4, // Color format
                        depthFormat); // Depth format
                }
        }

        void InitRenderTargetVectorIfNeed(ref RenderTarget2D target)
        {
            if(graphics.PreferredBackBufferWidth>0 && graphics.PreferredBackBufferHeight > 0)
            if (target is null || target.Width != graphics.PreferredBackBufferWidth || target.Height != graphics.PreferredBackBufferHeight)
            {
                // Dispose of the old render target if it exists
                target?.Dispose();

                // Set the depth format based on your requirements
                DepthFormat depthFormat = DepthFormat.Depth24;

                // Create the new render target with the specified depth format
                target = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    graphics.PreferredBackBufferWidth,
                    graphics.PreferredBackBufferHeight,
                    false, // No mipmaps
                    SurfaceFormat.HalfVector4, // Color format
                    depthFormat); // Depth format
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
