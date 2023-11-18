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

namespace RetroEngine
{
    public class Render
    {

        RenderTarget2D colorPath;
        RenderTarget2D normalPath;
        RenderTarget2D miscPath;
        RenderTarget2D postProcessingOutput;

        public RenderTarget2D shadowMap;

        public Texture2D black;

        RenderTarget2D outputPath;

        GraphicsDeviceManager graphics;

        Effect lightingEffect;

        public Effect NormalEffect;
        public Effect MiscEffect;
        public Effect UnifiedEffect;

        public Effect ShadowMapEffect;

        public Effect fxaaEffect;

        public Effect PostProcessingEffect;

        Delay shadowPassRenderDelay = new Delay();

        public Render()
        {
            lightingEffect = GameMain.content.Load<Effect>("DeferredLighting");
            NormalEffect = GameMain.content.Load<Effect>("NormalOutput");
            MiscEffect = GameMain.content.Load<Effect>("MiscOutput");
            UnifiedEffect = GameMain.content.Load<Effect>("UnifiedOutput");
            fxaaEffect = GameMain.content.Load<Effect>("fxaa");
            ShadowMapEffect = GameMain.content.Load<Effect>("ShadowMap");
            PostProcessingEffect = GameMain.content.Load<Effect>("PostProcessing");
        }

        public RenderTarget2D StartRenderLevel(Level level)
        {
            graphics = GameMain.inst._graphics;

            CreateBlackTexture();

            InitRenderTargetIfNeed(ref colorPath);
            InitRenderTargetIfNeed(ref normalPath);
            InitRenderTargetIfNeed(ref outputPath);
            //InitRenderTargetIfNeed(ref miscPath);
            InitRenderTargetIfNeed(ref postProcessingOutput);

            if (shadowMap is null)
            InitShadowMap(ref shadowMap);

            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            List<StaticMesh> renderList = level.GetMeshesToRender();

            RenderShadowMap(renderList);

            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            RenderUnifiedPath(renderList);
            //RenderColorPath(level);
            //RenderNormalPath(level);
            //RenderMiscPath(level);
            //PerformLighting();


            PerformPostProcessing();

            return outputPath;
        }

        void RenderUnifiedPath(List<StaticMesh> renderList)
        {

            graphics.GraphicsDevice.SetRenderTarget(colorPath);
            graphics.GraphicsDevice.Clear(Graphics.BackgroundColor * 0.25f);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            foreach (StaticMesh mesh in renderList)
                if (mesh.isRendered)
                {
                    mesh.DrawUnified();
                }

        }

        void RenderShadowMap(List<StaticMesh> renderList)
        {

            if (shadowPassRenderDelay.Wait()) return;

            shadowPassRenderDelay.AddDelay(0.02f);

            Graphics.LightViewProjection = Graphics.GetLightView() * Graphics.GetLightProjection();

            // Set up the shadow map render target with the desired resolution
            graphics.GraphicsDevice.SetRenderTarget(shadowMap);
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Graphics.shadowMapResolution, Graphics.shadowMapResolution);

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
                    mesh.DrawShadow();
                }
            }

            // Reset the render target and viewport to the back buffer's dimensions
            graphics.GraphicsDevice.SetRenderTarget(null);
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        }

        void PerformPostProcessing()
        {
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            graphics.GraphicsDevice.SetRenderTarget(postProcessingOutput);

            PostProcessingEffect.Parameters["ColorTexture"].SetValue(colorPath);


            SpriteBatch spriteBatch = GameMain.inst.SpriteBatch;
            spriteBatch.Begin(effect: PostProcessingEffect);

            DrawFullScreenQuad(spriteBatch, colorPath);

            spriteBatch.End();

            PerformFXAA();

            graphics.GraphicsDevice.SetRenderTarget(null);          

        }

        void PerformFXAA()
        {

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
            fxaaEffect.Parameters["screenColor"].SetValue(postProcessingOutput);

            // Begin drawing with SpriteBatch
            SpriteBatch spriteBatch = GameMain.inst.SpriteBatch;
            spriteBatch.Begin(effect: fxaaEffect);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, postProcessingOutput);

            // End the SpriteBatch
            spriteBatch.End();

            // Reset the render target to the default render target
            graphics.GraphicsDevice.SetRenderTarget(null);
        }

        void RenderColorPath(Level level)
        {
            graphics.GraphicsDevice.SetRenderTarget(colorPath);


            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (Entity ent in level.entities)
            {

                if (ent.meshes is not null)
                    foreach (StaticMesh mesh in ent.meshes)
                        mesh.Draw();
            }
        }

        void RenderNormalPath(Level level)
        {
            graphics.GraphicsDevice.SetRenderTarget(normalPath);

            graphics.GraphicsDevice.Clear(Color.Black);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (Entity ent in level.entities)
            {

                if (ent.meshes is not null)
                    foreach (StaticMesh mesh in ent.meshes)
                        mesh.DrawNormals();
            }
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

            // Reset the render target to the default render target
            graphics.GraphicsDevice.SetRenderTarget(null);
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
                SurfaceFormat.Color, // Color format
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
                    SurfaceFormat.Color, // Color format
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
