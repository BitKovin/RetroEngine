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

        RenderTarget2D DeferredOutput;

        RenderTarget2D miscPath;
        RenderTarget2D postProcessingOutput;
        public RenderTarget2D DepthOutput;

        public RenderTarget2D shadowMap;
        public RenderTarget2D shadowMapClose;

        public Texture2D black;

        RenderTarget2D ssaoOutput;
        RenderTarget2D outputPath;

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

        public Delay shadowPassRenderDelay = new Delay();

        public List<ParticleEmitter.Particle> particlesToDraw = new List<ParticleEmitter.Particle>();

        public Render()
        {
            //lightingEffect = GameMain.content.Load<Effect>("DeferredLighting");
            //NormalEffect = GameMain.content.Load<Effect>("NormalOutput");
            //MiscEffect = GameMain.content.Load<Effect>("MiscOutput");
            
            
            ShadowMapEffect = GameMain.content.Load<Effect>("ShadowMap");
            UnifiedEffect = GameMain.content.Load<Effect>("UnifiedOutput");
            fxaaEffect = GameMain.content.Load<Effect>("fxaa");
            //PostProcessingEffect = GameMain.content.Load<Effect>("PostProcessing");
            //ColorEffect = GameMain.content.Load<Effect>("ColorOutput");
            //ParticleColorEffect = GameMain.content.Load<Effect>("ParticleColorOutput");
            //SSAOEffect = GameMain.content.Load<Effect>("SSAO");
            BuffersEffect = GameMain.content.Load<Effect>("GPathesOutput");

            DeferredEffect = GameMain.content.Load<Effect>("DeferredShading");
        }

        public RenderTarget2D StartRenderLevel(Level level)
        {
            graphics = GameMain.inst._graphics;

            CreateBlackTexture();

            InitRenderTargetIfNeed(ref colorPath);
            InitRenderTargetIfNeed(ref emissivePath);
            InitRenderTargetIfNeed(ref normalPath);
            InitRenderTargetVectorIfNeed(ref positionPath);

            InitRenderTargetIfNeed(ref DeferredOutput);

            InitRenderTargetIfNeed(ref outputPath);
            InitRenderTargetIfNeed(ref ssaoOutput);
            //InitRenderTargetIfNeed(ref miscPath);
            InitRenderTargetIfNeed(ref postProcessingOutput);

            if (shadowMap is null)
                InitShadowMap(ref shadowMap);

            if (shadowMapClose is null)
                InitCloseShadowMap(ref shadowMapClose);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            List<StaticMesh> renderList = level.GetMeshesToRender();

            graphics.GraphicsDevice.SamplerStates[0] = Graphics.TextureFiltration ? (Graphics.AnisotropicFiltration ? SamplerState.AnisotropicWrap : SamplerState.LinearWrap) : SamplerState.PointWrap;

            //RenderDepthPath(renderList);
            //RenderNormalPath(renderList);

            RenderShadowMap(renderList);


            RenderUnifiedPath(renderList);
            //DrawPathes(renderList);
            //PerformDifferedShading();
            //RenderColorPath(renderList);
            //PerformLighting();

            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            PerformPostProcessing();

            return outputPath;
        }

        void RenderUnifiedPath(List<StaticMesh> renderList)
        {
            graphics.GraphicsDevice.SetRenderTarget(DeferredOutput);
            graphics.GraphicsDevice.Clear(Graphics.BackgroundColor);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            UnifiedEffect.Parameters["GlobalLightColor"].SetValue(Graphics.LightColor);

            particlesToDraw.Clear();

            foreach (StaticMesh mesh in renderList)
                {
                    mesh.DrawUnified();
                }

            ParticleEmitter.LoadRenderEmitter();
            ParticleEmitter.RenderEmitter.DrawParticles(particlesToDraw);

        }

        void DrawPathes(List<StaticMesh> renderList)
        {
            graphics.GraphicsDevice.SetRenderTargets(colorPath,emissivePath,normalPath,positionPath);
            graphics.GraphicsDevice.Clear(Color.Transparent);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            particlesToDraw.Clear();

            foreach (StaticMesh mesh in renderList)
            {
                mesh.DrawPathes();
            }

            ParticleEmitter.LoadRenderEmitter();
            ParticleEmitter.RenderEmitter.DrawParticles(particlesToDraw);
        }

        void PerformDifferedShading()
        {
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            graphics.GraphicsDevice.SetRenderTarget(DeferredOutput);

            graphics.GraphicsDevice.Clear(Graphics.BackgroundColor);

            DeferredEffect.Parameters["ColorTexture"].SetValue(colorPath);
            DeferredEffect.Parameters["EmissiveTexture"].SetValue(emissivePath);
            DeferredEffect.Parameters["NormalTexture"].SetValue(normalPath);
            DeferredEffect.Parameters["PositionTexture"].SetValue(positionPath);

            DeferredEffect.Parameters["DirectBrightness"].SetValue(Graphics.DirectLighting);
            DeferredEffect.Parameters["GlobalBrightness"].SetValue(Graphics.GlobalLighting);
            DeferredEffect.Parameters["LightDirection"].SetValue(Graphics.LightDirection);


            Vector3[] LightPos = new Vector3[LightManager.MAX_POINT_LIGHTS];
            Vector3[] LightColor = new Vector3[LightManager.MAX_POINT_LIGHTS];
            float[] LightRadius = new float[LightManager.MAX_POINT_LIGHTS];

            for (int i = 0; i < LightManager.MAX_POINT_LIGHTS; i++)
            {
                LightPos[i] = LightManager.FinalPointLights[i].Position;
                LightColor[i] = LightManager.FinalPointLights[i].Color;
                LightRadius[i] = LightManager.FinalPointLights[i].Radius;
            }

            DeferredEffect.Parameters["LightPositions"].SetValue(LightPos);
            DeferredEffect.Parameters["LightColors"].SetValue(LightColor);
            DeferredEffect.Parameters["LightRadiuses"].SetValue(LightRadius);


            DeferredEffect.Parameters["ShadowMapViewProjection"].SetValue(Graphics.LightViewProjection);
            DeferredEffect.Parameters["ShadowMap"].SetValue(shadowMap);
            DeferredEffect.Parameters["ShadowBias"].SetValue(Graphics.ShadowBias);
            DeferredEffect.Parameters["ShadowMapResolution"].SetValue((float)Graphics.shadowMapResolution);

            DeferredEffect.Parameters["GlobalLightColor"].SetValue(Graphics.LightColor);

            SpriteBatch spriteBatch = GameMain.inst.SpriteBatch;

            spriteBatch.Begin(effect: DeferredEffect);

            DrawFullScreenQuad(spriteBatch, colorPath);

            spriteBatch.End();
            graphics.GraphicsDevice.SetRenderTarget(null);
        }

        void RenderDepthPath(List<StaticMesh> renderList)
        {
            graphics.GraphicsDevice.SetRenderTarget(DepthOutput);
            graphics.GraphicsDevice.Clear(Color.Red);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);


            foreach (StaticMesh mesh in renderList)
                if (mesh.isRendered)
                {
                    mesh.DrawDepth();
                }

        }

        void RenderShadowMap(List<StaticMesh> renderList)
        {

            if (shadowPassRenderDelay.Wait()) return;

            shadowPassRenderDelay.AddDelay(0.05f);

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

            UnifiedEffect.Parameters["ShadowMap"].SetValue(GameMain.inst.render.shadowMap);

            UnifiedEffect.Parameters["ShadowBias"].SetValue(Graphics.ShadowBias);
            UnifiedEffect.Parameters["ShadowMapResolution"].SetValue((float)Graphics.shadowMapResolution);

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
            PerformFXAA();
            return;
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            graphics.GraphicsDevice.SetRenderTarget(postProcessingOutput);

            PostProcessingEffect.Parameters["ColorTexture"].SetValue(colorPath);
            //PostProcessingEffect.Parameters["Enabled"].SetValue(Graphics.EnablePostPocessing);

            SpriteBatch spriteBatch = GameMain.inst.SpriteBatch;

            spriteBatch.Begin(effect: PostProcessingEffect);

            DrawFullScreenQuad(spriteBatch, colorPath);

            spriteBatch.End();

                

        }

        void PerformSSAO()
        {
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            graphics.GraphicsDevice.SetRenderTarget(ssaoOutput);

            SSAOEffect.Parameters["ColorTexture"].SetValue(colorPath);
            SSAOEffect.Parameters["NormalTexture"].SetValue(normalPath);
            SSAOEffect.Parameters["DepthTexture"].SetValue(DepthOutput);
            SSAOEffect.Parameters["screenWidth"].SetValue(1280/3);
            SSAOEffect.Parameters["screenHeight"].SetValue(720/3);
            SSAOEffect.Parameters["ssaoRadius"].SetValue(20);
            //SSAOEffect.Parameters["ssaoBias"].SetValue(0.000001f);
            SSAOEffect.Parameters["ssaoIntensity"].SetValue(0.6f);


            SpriteBatch spriteBatch = GameMain.inst.SpriteBatch;

            spriteBatch.Begin(effect: SSAOEffect);

            DrawFullScreenQuad(spriteBatch, colorPath);

            spriteBatch.End();
        }
        void PerformFXAA()
        {
            if(Graphics.EnableAntiAliasing == false)
            {
                outputPath = DeferredOutput;
                return;
            }
            // Set the render target to the output path
            graphics.GraphicsDevice.SetRenderTarget(outputPath);

            float fxaaQualitySubpix = 0.75f;
            float fxaaQualityEdgeThreshold = 0.166f;
            float fxaaQualityEdgeThresholdMin = 0.0833f;

            fxaaEffect.CurrentTechnique = fxaaEffect.Techniques["ppfxaa_PC"];

            fxaaEffect.Parameters["fxaaQualitySubpix"].SetValue(fxaaQualitySubpix);
            fxaaEffect.Parameters["fxaaQualityEdgeThreshold"].SetValue(fxaaQualityEdgeThreshold);
            fxaaEffect.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(fxaaQualityEdgeThresholdMin);

            fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / graphics.PreferredBackBufferWidth);
            fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / graphics.PreferredBackBufferHeight);
            fxaaEffect.Parameters["screenColor"].SetValue(DeferredOutput);

            // Begin drawing with SpriteBatch
            SpriteBatch spriteBatch = GameMain.inst.SpriteBatch;
            spriteBatch.Begin(effect: fxaaEffect);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, DeferredOutput);

            // End the SpriteBatch
            spriteBatch.End();

        }

        void RenderColorPath(List<StaticMesh> renderList)
        {
            graphics.GraphicsDevice.SetRenderTarget(colorPath);
            graphics.GraphicsDevice.Clear(Graphics.BackgroundColor);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            particlesToDraw.Clear();

            foreach (StaticMesh mesh in renderList)
            {
                mesh.Draw();
            }

            ParticleEmitter.LoadRenderEmitter();
            ParticleEmitter.RenderEmitter.DrawParticlesPathes(particlesToDraw);

        }

        void RenderNormalPath(List<StaticMesh> renderList)
        {
            graphics.GraphicsDevice.SetRenderTarget(normalPath);
            graphics.GraphicsDevice.Clear(new Color(0,0,0,0));

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            foreach (StaticMesh mesh in renderList)
            {
                mesh.DrawNormals();
            }

            ParticleEmitter.LoadRenderEmitter();
            ParticleEmitter.RenderEmitter.DrawParticlesPathes(particlesToDraw);
            
        }

        void RenderMiscPath(Level level)
        {
            graphics.GraphicsDevice.SetRenderTarget(miscPath);

            graphics.GraphicsDevice.Clear(Color.White);


            foreach (Entity ent in level.entities)
            {

                if (ent.meshes is not null)
                    foreach (StaticMesh mesh in ent.meshes)
                        mesh.DrawMisc();
            }
        }



        void PerformLighting()
        {


            // Set the render target to the output path
            graphics.GraphicsDevice.SetRenderTarget(outputPath);

            // Clear the render target
            //graphics.GraphicsDevice.Clear(Color.Transparent);

            // Set the necessary parameters for the lighting effect
            lightingEffect.Parameters["ColorTexture"].SetValue(colorPath);
            lightingEffect.Parameters["NormalTexture"].SetValue(normalPath);
            lightingEffect.Parameters["MiscTexture"].SetValue(miscPath);

            // Begin drawing with SpriteBatch
            SpriteBatch spriteBatch = GameMain.inst.SpriteBatch;
            spriteBatch.Begin(effect: lightingEffect);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, colorPath);

            // End the SpriteBatch
            spriteBatch.End();
        }

        void DrawFullScreenQuad(SpriteBatch spriteBatch, Texture2D inputTexture)
        {
            // Create a rectangle covering the entire screen
            Rectangle screenRectangle = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            // Draw the full-screen quad using SpriteBatch
            spriteBatch.Draw(inputTexture, screenRectangle, Color.White);
        }

        void InitShadowMap(ref RenderTarget2D target)
        {
            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth24;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                Graphics.shadowMapResolution,
                Graphics.shadowMapResolution,
                false, // No mipmaps
                SurfaceFormat.Single, // Color format
                depthFormat); // Depth format
        }

        void InitCloseShadowMap(ref RenderTarget2D target)
        {
            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth24;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                Graphics.closeShadowMapResolution,
                Graphics.closeShadowMapResolution,
                false, // No mipmaps
                SurfaceFormat.Single, // Color format
                depthFormat); // Depth format
        }

        void InitRenderTargetIfNeed(ref RenderTarget2D target)
        {
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
                    SurfaceFormat.Rgba64, // Color format
                    depthFormat); // Depth format
            }
        }

        void InitRenderTargetVectorIfNeed(ref RenderTarget2D target)
        {
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
