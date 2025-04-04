﻿using RetroEngine;
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
using RetroEngine.Entities;
using RetroEngine.Entities.Light;
using RetroEngine.Graphic;
using RetroEngine.PhysicsSystem;
using CppNet;
using SharpDX.Multimedia;

namespace RetroEngine
{
    public class Render
    {

        RenderTarget2D colorPath;
        internal RenderTarget2D normalPath;
        public RenderTarget2D positionPath;

        RenderTarget2D ForwardOutput;
        internal RenderTarget2D ReflectionOutput;
        RenderTarget2D ReflectivenessOutput;
        internal RenderTarget2D DepthPrepathOutput;
        internal RenderTarget2D DepthPrepathBufferOutput;

        RenderTarget2D postProcessingOutput;
        public RenderTarget2D DepthOutput;

        public RenderTarget2D shadowMapViewmodel;
        public RenderTarget2D shadowMap;
        public RenderTarget2D shadowMapClose;

        public Texture2D black;

        RenderTarget2D ssaoOutput;
        RenderTarget2D ComposedOutput;
        RenderTarget2D FxaaOutput;

        RenderTarget2D bloomSample;
        RenderTarget2D bloomSample2;
        RenderTarget2D bloomSample3;
        RenderTarget2D bloomSample4;

        RenderTarget2D outputPath;

        RenderTarget2D occlusionTestPath;

        RenderTarget2D oldFrame;

        GraphicsDeviceManager graphics;

        Effect lightingEffect;

        public Effect BuffersEffect;
        public Effect DeferredEffect;

        Effect DenoiseEffect;

        Effect DepthApplyEffect;

        public Effect ReflectionEffect;
        public Effect ReflectionResultEffect;

        public Effect OcclusionEffect;
        public Effect OcclusionStaticEffect;

        public Effect GeometryShadowEffect;

        public Effect ColorEffect;
        public Effect ParticleColorEffect;
        public Effect NormalEffect;
        public Effect MiscEffect;

        public Effect ShadowMapEffect;
        public Effect ShadowMapMaskedEffect;

        public Effect fxaaEffect;

        public Effect maxDepth;

        public Effect PostProcessingEffect;

        public Effect SSAOEffect;

        public Effect BloomEffect;

        public Effect BlurEffect;

        Shader ShadowCasterPathEffect;
        Effect ShadowCasterApplyEffect;

        public Effect ComposeEffect;
        Effect TonemapperEffect;

        public Delay shadowPassRenderDelay = new Delay();

        public List<ParticleEmitter.Particle> particlesToDraw = new List<ParticleEmitter.Particle>();

        SamplerState samplerState = new SamplerState();

        public static float ResolutionScale = 1f;

        bool dirtySampler = true;

        internal BoundingSphere BoundingSphere = new BoundingSphere();

        static internal List<StaticMesh> testedMeshes = new List<StaticMesh>();

        static internal bool IgnoreFrustrumCheck = false;

        static internal BoundingFrustum CustomFrustrum = null;

        public static Texture2D LUT;

        public static bool StableDirectShadows = false;

        public static bool UsesOpenGL = false;

        internal static bool DrawOnlyOpaque = true;
        internal static bool DrawOnlyTransparent = true;

        public static bool DisableMultiPass = false;

        public static bool AsyncPresent = true;

        public static bool SimpleRender = false;

        public static bool LimitedColorSpace = false;

        public Render()
        {
            graphics = GameMain.Instance._graphics;

            //lightingEffect = GameMain.content.Load<Effect>("DeferredLighting");
            //NormalEffect = GameMain.content.Load<Effect>("NormalOutput");
            //MiscEffect = GameMain.content.Load<Effect>("MiscOutput");


            ShadowMapEffect = GameMain.content.Load<Effect>("Shaders/ShadowMap");
            ShadowMapMaskedEffect = GameMain.content.Load<Effect>("Shaders/ShadowMapMasked");
            fxaaEffect = GameMain.content.Load<Effect>("Shaders/fxaa");
            maxDepth = GameMain.content.Load<Effect>("Shaders/MaxDepth");
            //PostProcessingEffect = GameMain.content.Load<Effect>("PostProcessing");
            //ColorEffect = GameMain.content.Load<Effect>("ColorOutput");
            //ParticleColorEffect = GameMain.content.Load<Effect>("ParticleColorOutput");
            SSAOEffect = GameMain.content.Load<Effect>("Shaders/ssao");
            BuffersEffect = GameMain.content.Load<Effect>("Shaders/GPathesOutput");

            DeferredEffect = GameMain.content.Load<Effect>("Shaders/DeferredShading");

            DepthApplyEffect = GameMain.content.Load<Effect>("Shaders/DepthFromTex");

            ComposeEffect = GameMain.content.Load<Effect>("Shaders/ComposedColor");

            BloomEffect = GameMain.content.Load<Effect>("Shaders/BloomSampler");

            OcclusionEffect = GameMain.content.Load<Effect>("Shaders/OcclusionPath");
            OcclusionStaticEffect = GameMain.content.Load<Effect>("Shaders/OcclusionPathStatic");

            GeometryShadowEffect = GameMain.content.Load<Effect>("Shaders/GeometryShadow");
            GeometryShadowEffect.Name = "GeometryShadow";

            DenoiseEffect = GameMain.content.Load<Effect>("Shaders/Denoise");

            TonemapperEffect = GameMain.content.Load<Effect>("Shaders/Tonemap");

            BlurEffect = GameMain.content.Load<Effect>("Shaders/SimpleBlur");

            ReflectionEffect = AssetRegistry.GetShaderFromName("ReflectionPath");
            ReflectionResultEffect = GameMain.content.Load<Effect>("Shaders/ReflectionResult");

            ShadowCasterPathEffect = AssetRegistry.GetPostProcessShaderFromName("ShadowCasterPath");

            ShadowCasterApplyEffect = GameMain.content.Load<Effect>("Shaders/ShadowCasterApply");


            //InitSampler();
        }

        public void UpdateShaderFrameData()
        {
            var shaders = AssetRegistry.GetAllShaders();

            foreach (Shader effect in shaders)
            {
                UpdateDataForShader(effect);
            }
        }

        public void UpdateDataForShader(Shader effect)
        {
            effect.Parameters["viewDirForward"]?.SetValue(Camera.finalizedRotation.GetForwardVector());
            effect.Parameters["viewDirUp"]?.SetValue(Camera.finalizedRotation.GetUpVector());
            effect.Parameters["viewDirRight"]?.SetValue(Camera.finalizedRotation.GetRightVector());
            effect.Parameters["viewPos"]?.SetValue(Camera.finalizedPosition);


            effect.Parameters["DirectBrightness"]?.SetValue(Graphics.DirectLighting);
            effect.Parameters["GlobalBrightness"]?.SetValue(Graphics.GlobalLighting);
            effect.Parameters["LightDirection"]?.SetValue(Graphics.LightDirection.Normalized());

            effect.Parameters["ShadowMapViewProjection"]?.SetValue(Graphics.LightViewProjection);
            effect.Parameters["ShadowMapViewProjectionClose"]?.SetValue(Graphics.LightViewProjectionClose);
            effect.Parameters["ShadowMapViewProjectionVeryClose"]?.SetValue(Graphics.LightViewProjectionVeryClose);

            effect.Parameters["ShadowBias"]?.SetValue(Graphics.ShadowBias);
            effect.Parameters["ShadowMapResolution"]?.SetValue((float)Graphics.shadowMapResolution);
            effect.Parameters["ShadowMapResolutionClose"]?.SetValue((float)Graphics.closeShadowMapResolution);
            effect.Parameters["ShadowMapResolutionVeryClose"]?.SetValue((float)Graphics.veryCloseShadowMapResolution);

            effect.Parameters["ShadowMap"]?.SetValue(GameMain.Instance.render.shadowMap);
            effect.Parameters["ShadowMapClose"]?.SetValue(GameMain.Instance.render.shadowMapClose);
            //effect.Parameters["ShadowMapVeryClose"]?.SetValue(GameMain.Instance.render.shadowMapVeryClose);


            effect.Parameters["InverseViewProjection"]?.SetValue(Matrix.Invert(Camera.finalizedView * Camera.finalizedProjection));

            effect.Parameters["View"]?.SetValue(Camera.finalizedView);
            effect.Parameters["Projection"]?.SetValue(Camera.finalizedProjection);
            effect.Parameters["ProjectionViewmodel"]?.SetValue(Camera.finalizedProjectionViewmodel);

            effect.Parameters["GlobalLightColor"]?.SetValue(Graphics.LightColor);
            effect.Parameters["SkyColor"]?.SetValue(Graphics.SkyLightColor);

            if (Graphics.GlobalPointLights)
            {
                Vector3[] LightPos = new Vector3[LightManager.MAX_POINT_LIGHTS];
                Vector3[] LightColor = new Vector3[LightManager.MAX_POINT_LIGHTS];
                float[] LightRadius = new float[LightManager.MAX_POINT_LIGHTS];
                float[] LightRes = new float[LightManager.MAX_POINT_LIGHTS];

                for (int i = 0; i < LightManager.MAX_POINT_LIGHTS; i++)
                {
                    LightPos[i] = LightManager.FinalPointLights[i].Position;
                    LightColor[i] = LightManager.FinalPointLights[i].Color;
                    LightRadius[i] = LightManager.FinalPointLights[i].Radius;
                    LightRes[i] = LightManager.FinalPointLights[i].Resolution;

                }

                effect.Parameters["LightPositions"]?.SetValue(LightPos);
                effect.Parameters["LightColors"]?.SetValue(LightColor);
                effect.Parameters["LightRadiuses"]?.SetValue(LightRadius);
                effect.Parameters["LightResolutions"]?.SetValue(LightRes);
            }


            effect.Parameters["DepthTexture"]?.SetValue(DepthPrepathOutput);
            effect.Parameters["ReflectionTexture"]?.SetValue(ReflectionOutput);
            effect.Parameters["FrameTexture"]?.SetValue(oldFrame);

            var cubeMap = CubeMap.GetClosestToCamera();
            if (cubeMap != null)
            {
                effect.Parameters["ReflectionCubemap"]?.SetValue(cubeMap.map);
                effect.Parameters["ReflectionCubemapMin"]?.SetValue(cubeMap.boundingBoxMin);
                effect.Parameters["ReflectionCubemapMax"]?.SetValue(cubeMap.boundingBoxMax);
                effect.Parameters["ReflectionCubemapPosition"]?.SetValue(cubeMap.Position);

            }else
            {
                effect.Parameters["ReflectionCubemapMin"]?.SetValue(Vector3.Zero);
                effect.Parameters["ReflectionCubemapMax"]?.SetValue(Vector3.Zero);
                effect.Parameters["ReflectionCubemapPosition"]?.SetValue(Vector3.Zero);
            }
            if (ForwardOutput != null)
            {
                effect.Parameters["ScreenHeight"]?.SetValue(ForwardOutput.Height);
                effect.Parameters["ScreenWidth"]?.SetValue(ForwardOutput.Width);
            }
            effect.Parameters["LightDistanceMultiplier"]?.SetValue(Graphics.LightDistanceMultiplier) ;

            if (reflection != null)
            {
                effect.Parameters["SSRHeight"]?.SetValue(reflection.Width);
                effect.Parameters["SSRWidth"]?.SetValue(reflection.Height);
            }

            effect.Parameters["earlyZ"]?.SetValue(Graphics.EarlyDepthDiscardShader);

            effect.Parameters["ViewmodelShadowsEnabled"]?.SetValue(Graphics.ViewmodelShadows);

            effect.Parameters["PointLightShadowQuality"]?.SetValue(Graphics.PointLightShadowQuality);
            effect.Parameters["DirectionalLightShadowQuality"]?.SetValue(Graphics.DirectionalLightShadowQuality);

            effect.ApplyValues();

        }


        public RenderTarget2D StartRenderLevel(Level level)
        {
           

            CreateBlackTexture();



            InitSizedRenderTargetIfNeed(ref DepthPrepathOutput, (int)GetScreenResolution().Y, DepthFormat.Depth24, SurfaceFormat.Single);
            InitSizedRenderTargetIfNeed(ref DepthPrepathBufferOutput, (int)GetScreenResolution().Y, DepthFormat.Depth24, SurfaceFormat.Single);

            InitRenderTargetVectorIfNeed(ref ForwardOutput, true);

            

            InitRenderTargetVectorIfNeed(ref oldFrame);

            if (DisableMultiPass == false)
            {
                InitRenderTargetVectorSpaceIfNeed(ref positionPath);

                InitRenderTargetIfNeed(ref normalPath);

                InitRenderTargetIfNeed(ref ReflectivenessOutput);

            }else
            {
                positionPath?.Dispose();
                positionPath = null;
                normalPath?.Dispose();
                normalPath = null;
                ReflectivenessOutput?.Dispose();
                ReflectivenessOutput = null;

                reflection?.Dispose();
                reflection = null;
            }
            InitRenderTargetIfNeed(ref ComposedOutput);

            InitRenderTargetIfNeed(ref FxaaOutput);


            InitSizedRenderTargetIfNeed(ref ssaoOutput,(int)(GetScreenResolution().Y/2));

            InitSizedRenderTargetIfNeed(ref bloomSample, 256, surfaceFormat: SurfaceFormat.HalfVector4);
            InitSizedRenderTargetIfNeed(ref bloomSample2, 128, surfaceFormat: SurfaceFormat.HalfVector4);
            InitSizedRenderTargetIfNeed(ref bloomSample3, 64, surfaceFormat: SurfaceFormat.HalfVector4);
            InitSizedRenderTargetIfNeed(ref bloomSample4, 32, surfaceFormat: SurfaceFormat.HalfVector4);

            InitRenderTargetIfNeed(ref postProcessingOutput);

            GameMain.Instance.WaitForFramePresent();

            //if (outputPath!=null)

            List<StaticMesh> renderList = level.GetMeshesToRender();

            DrawOnlyOpaque = false;
            DrawOnlyTransparent = false;

            RenderPrepass(renderList);
            

            

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            if (Graphics.DirectLighting > 0)
            {
                ShadowMapEffect.Parameters["LightDirection"]?.SetValue(Graphics.LightDirection);
                ShadowMapMaskedEffect.Parameters["LightDirection"]?.SetValue(Graphics.LightDirection);


                if (Graphics.ShadowResolutionScale > 0.001f)
                {
                    RenderShadowMap(renderList);
                    RenderShadowMapClose(renderList);
                    RenderShadowMapVeryClose(renderList);
                    RenderShadowMapViewmodel(renderList);
                }
            }
            graphics.GraphicsDevice.RasterizerState = Graphics.DisableBackFaceCulling? RasterizerState.CullCounterClockwise : RasterizerState.CullNone;
                
            //InitSampler(4);

            //EndOcclusionTest(renderList);
            //return DeferredOutput;




            PointLight.DrawDirtyPointLights();


            if (SimpleRender)
            {
                RenderForwardPath(renderList);
                //return ForwardOutput;
                DownsampleToTexture(ForwardOutput, oldFrame);
            }
            else
            {

                DrawOnlyOpaque = true;
                DrawOnlyTransparent = false;
                RenderForwardPath(renderList);

                DownsampleToTexture(ForwardOutput, oldFrame);


                DrawOnlyOpaque = false;
                DrawOnlyTransparent = true;
                RenderForwardPath(renderList, true);
                DrawOnlyOpaque = false;
                DrawOnlyTransparent = false;
            }

            DrawShadowCasterPath();

            if (SimpleRender && false)
            {
                PerformSimplePostProcessing();
            }
            else
            {
                PerformPostProcessing();
            }


            if (Input.GetAction("test").Holding())
                return normalPath;

            return outputPath;

        }

        public void InitSampler(int max = 10)
        {


            samplerState = new SamplerState();

            samplerState.Filter = Graphics.TextureFiltration ? (Graphics.AnisotropicFiltration ? TextureFilter.Anisotropic : TextureFilter.Linear) : TextureFilter.PointMipLinear;

            samplerState.AddressU = TextureAddressMode.Wrap;
            samplerState.AddressV = TextureAddressMode.Wrap;
            samplerState.AddressW = TextureAddressMode.Wrap;

            samplerState.MipMapLevelOfDetailBias = Graphics.MipLevel;
            samplerState.MaxAnisotropy = 16;

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

            if (DisableMultiPass)
            {
                graphics.GraphicsDevice.SetRenderTarget(ForwardOutput);
            }
            else
            {
                graphics.GraphicsDevice.SetRenderTargets(ForwardOutput, normalPath, ReflectivenessOutput, positionPath);
            }

            if (onlyTransperent == false)
            {
                graphics.GraphicsDevice.DepthStencilState = new DepthStencilState()
                {
                    DepthBufferWriteEnable = true,
                    DepthBufferEnable = true,
                    DepthBufferFunction = CompareFunction.LessEqual,
                    StencilEnable = false,
                };



                if (GameMain.Instance.DefaultShader.ToLower() == "overdraw")
                    graphics.GraphicsDevice.Clear(Color.Black);
                else
                    graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

                
                DepthApplyEffect.Parameters["OldFrame"].SetValue(GameMain.Instance.DefaultShader.ToLower() == "overdraw" ? black : oldFrame);
                    DrawFullScreenQuad(DepthPrepathBufferOutput, DepthApplyEffect);
            }

            RenderLevelGeometryForward(renderList, onlyTransperent);



            if (onlyTransperent == false || true)
            {
                if (Graphics.GeometricalShadowsEnabled)
                {
                    graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                    GeometryShadowEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjection);
                    GeometryShadowEffect.Parameters["ScreenHeight"].SetValue(ForwardOutput.Height);
                    GeometryShadowEffect.Parameters["ScreenWidth"].SetValue(ForwardOutput.Width);

                    foreach (var mesh in renderList)
                    {
                        mesh?.DrawGeometryShadow();
                    }
                }
            }

            if (DrawOnlyOpaque || SimpleRender)
            {
                if (Graphics.DrawPhysics)
                    Physics.DebugDraw();

                DrawDebug.Draw();
            }
        }

        public void RenderLevelGeometryForward(List<StaticMesh> renderList, bool onlyTransperent = false, bool OnlyStatic = false, bool skipTransparent = false)
        {

            foreach (StaticMesh mesh in renderList)
            {

                if (skipTransparent && mesh.Transperent) continue;

                if (mesh == null) continue;
                if (mesh.Transperent || onlyTransperent == false || mesh.Viewmodel)
                {
                    
                    if(mesh.Static || OnlyStatic==false)
                        mesh.DrawUnified();

                }
            }
        }

        public void RenderLevelGeometryDepth(List<StaticMesh> renderList, bool OnlyStatic = false, bool onlyShadowCasters = false, RasterizerState rasterizerState = null, bool pointLight = false, bool OnlyDynamic = false)
        {
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (rasterizerState == null)
                graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            else
                graphics.GraphicsDevice.RasterizerState = rasterizerState;

            graphics.GraphicsDevice.BlendState = BlendState.Opaque;

            foreach (StaticMesh mesh in renderList)
            {
                if (mesh == null) continue;
                if (mesh.Transperent == false)
                {
                    if ((mesh.CastShadows == true && ((mesh.Static && OnlyDynamic == false) || (OnlyDynamic && mesh.Static == false) || (OnlyDynamic == false && OnlyStatic == false)))
                        || onlyShadowCasters == false)
                        mesh.DrawDepth(pointLight);

                }
            }
        }

        void RenderPrepass(List<StaticMesh> renderList)
        {
            UpdateShaderFrameData();

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, (int)GetScreenResolution().X, (int)GetScreenResolution().Y);

            graphics.GraphicsDevice.SetRenderTargets(DepthPrepathOutput,DepthPrepathBufferOutput);
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            graphics.GraphicsDevice.BlendState = BlendState.Opaque;


            OcclusionEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjection);
            OcclusionStaticEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjection);


            OcclusionEffect.Parameters["pointDistance"].SetValue(false);
            OcclusionStaticEffect.Parameters["pointDistance"].SetValue(false);

            var list = renderList.OrderBy(m => Vector3.Distance(Camera.finalizedPosition, m.GetClosestToCameraPosition()) * (m.Viewmodel? 0.1 : 1)).ToArray();

            foreach (StaticMesh mesh in list)
            {
                
                    OcclusionEffect.Parameters["Viewmodel"].SetValue(false);

                    if (mesh.Viewmodel)
                        OcclusionEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjectionViewmodel);
                    else
                        OcclusionEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjection);
                    mesh.StartOcclusionTest();

                
            }

            testedMeshes = new List<StaticMesh>(renderList);

        }

        internal void FillPrepas()
        {

            InitRenderTargetDepth(ref DepthPrepathOutput);

            UpdateShaderFrameData();

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, (int)GetScreenResolution().X, (int)GetScreenResolution().Y);

            graphics.GraphicsDevice.SetRenderTarget(DepthPrepathOutput);
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: maxDepth);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, black);

            // End the SpriteBatch
            spriteBatch.End();
            //graphics.GraphicsDevice.SetRenderTarget(null);
        }

        public bool renderShadow()
        {
            return !shadowPassRenderDelay.Wait() || Level.ChangingLevel;
        }

        RenderTarget2D shadowCasterPath;
        RenderTarget2D shadowCasterPathFinal;

        void DrawShadowCasterPath()
        {

            InitSizedRenderTargetIfNeed(ref shadowCasterPath, (int)(GetScreenResolution().Y / 2), DepthFormat.None, SurfaceFormat.Color);
            InitSizedRenderTargetIfNeed(ref shadowCasterPathFinal, (int)(GetScreenResolution().Y / 2), DepthFormat.None, SurfaceFormat.Color);

            graphics.GraphicsDevice.SetRenderTarget(shadowCasterPath);
            graphics.GraphicsDevice.Viewport = new Viewport(0,0, shadowCasterPath.Width, shadowCasterPath.Height);
            graphics.GraphicsDevice.Clear(Color.White);

            ShadowCasterPathEffect.Parameters["DepthTexture"]?.SetValue(GameMain.Instance.render.DepthPrepathOutput);

            ShadowCasterPathEffect.Parameters["PositionTexture"]?.SetValue(positionPath);


            ShadowCasterPathEffect.Parameters["NormalTexture"]?.SetValue(normalPath);

            UpdateDataForShader(ShadowCasterPathEffect);

            PointShadowCaster.ApplyShadowCastersToShader(ShadowCasterPathEffect);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: ShadowCasterPathEffect, sortMode: SpriteSortMode.FrontToBack, blendState: BlendState.Opaque);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, GameMain.Instance.render.positionPath);

            // End the SpriteBatch
            spriteBatch.End();


            DownsampleToTexture(shadowCasterPath, shadowCasterPathFinal, true, 0.5f);

        }

        internal void RenderShadowMap(List<StaticMesh> renderList)
        {

            if (Graphics.ShadowResolutionScale == 0) return;

            InitShadowMap(ref shadowMap);

            if (renderShadow() == false && StableDirectShadows == false) return;

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

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;



            // Iterate through meshes and draw shadows
            foreach (StaticMesh mesh in renderList)
            {
                if(mesh.Static == true && Graphics.DynamicSunShadowsEnabled == false || Graphics.DynamicSunShadowsEnabled == true)
                    mesh.DrawShadow();
            }

        }

        internal void RenderShadowMapClose(List<StaticMesh> renderList)
        {


            if (Graphics.ShadowResolutionScale == 0) return;

            InitShadowMapClose(ref shadowMapClose);

            

            // Set up the shadow map render target with the desired resolution
            graphics.GraphicsDevice.SetRenderTarget(shadowMapClose);
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, shadowMapClose.Height, shadowMapClose.Height);

            // Clear the shadow map with the desired clear color (e.g., Color.White)
            

            if (renderShadow() && StableDirectShadows == false) return;
            graphics.GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: maxDepth, sortMode: SpriteSortMode.FrontToBack);

            // Draw a full-screen quad to apply the lighting
            DrawShadowQuad(spriteBatch, black);

            // End the SpriteBatch
            spriteBatch.End();

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            graphics.GraphicsDevice.BlendState = BlendState.Opaque;

            // Iterate through meshes and draw shadows
            foreach (StaticMesh mesh in renderList)
            {
                if (mesh.Static == true && Graphics.DynamicSunShadowsEnabled == false || Graphics.DynamicSunShadowsEnabled == true)
                    mesh.DrawShadow(true);
            }

        }

        internal void RenderShadowMapVeryClose(List<StaticMesh> renderList)
        {


            if (Graphics.ShadowResolutionScale == 0) return;

            InitShadowMapVeryClose(ref shadowMapClose);


            // Set up the shadow map render target with the desired resolution
            graphics.GraphicsDevice.SetRenderTarget(shadowMapClose);
            graphics.GraphicsDevice.Viewport = new Viewport(shadowMapClose.Height, 0, shadowMapClose.Height, shadowMapClose.Height);

            // Clear the shadow map with the desired clear color (e.g., Color.White)
            //graphics.GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: maxDepth, sortMode: SpriteSortMode.FrontToBack);

            // Draw a full-screen quad to apply the lighting
            DrawShadowQuad(spriteBatch, black);

            // End the SpriteBatch
            spriteBatch.End();

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            graphics.GraphicsDevice.BlendState = BlendState.Opaque;

            // Iterate through meshes and draw shadows
            foreach (StaticMesh mesh in renderList)
            {
                if (mesh.Static == true && Graphics.DynamicSunShadowsEnabled == false || Graphics.DynamicSunShadowsEnabled == true)
                    mesh.DrawShadow(veryClose: true);
            }

        }

        internal void RenderShadowMapViewmodel(List<StaticMesh> renderList)
        {


            if (Graphics.ShadowResolutionScale == 0) return;

            if (Graphics.ViewmodelShadows == false) return;

            InitShadowMapViemodel(ref shadowMapViewmodel);


            // Set up the shadow map render target with the desired resolution
            graphics.GraphicsDevice.SetRenderTarget(shadowMapViewmodel);
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Graphics.ViewmodelShadowMapResolution, Graphics.ViewmodelShadowMapResolution);

            // Clear the shadow map with the desired clear color (e.g., Color.White)
            graphics.GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: maxDepth, sortMode: SpriteSortMode.FrontToBack);

            // Draw a full-screen quad to apply the lighting
            DrawShadowQuad(spriteBatch, black);

            // End the SpriteBatch
            spriteBatch.End();


            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;


            graphics.GraphicsDevice.BlendState = BlendState.Opaque;

            // Iterate through meshes and draw shadows
            foreach (StaticMesh mesh in renderList)
            {
                if (mesh.Viewmodel) // ||mesh.CastViewModelShadows
                {
                    mesh.DrawShadow(viewmodel: true);
                }
            }

        }

        RenderTarget2D TonemapResult;

        RenderTarget2D stepsResult;

        void PerformSimplePostProcessing()
        {
            lock (PostProcessStep.StepsBefore)
            {
                PerformPostProcessingShaders(ForwardOutput);
            }

            PerformTonemapping();

            lock (PostProcessStep.StepsAfter)
            {
                PerformPostProcessingShaders(TonemapResult, true);
            }

            PerformFXAA();

        }

        void PerformPostProcessing()
        {
            if (SimpleRender == false)
            {
                PerformReflection();
                ApplyReflection();
            }

            lock (PostProcessStep.StepsBefore)
            {
                PerformPostProcessingShaders(ForwardOutput);
            }

            PerformSSAO();

            PerformTonemapping();
            
            CalculateBloom();

            PerformCompose();

            lock (PostProcessStep.StepsAfter)
            {
                PerformPostProcessingShaders(ComposedOutput, true);
            }

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

        RenderTarget2D targetA;
        RenderTarget2D targetB;
        void PerformPostProcessingShaders(RenderTarget2D input, bool after = false)
        {

            if (SimpleRender)
            {
                InitRenderTargetIfNeed(ref targetA);
                InitRenderTargetIfNeed(ref targetB);
            }
            else
            {

                InitRenderTargetVectorIfNeed(ref targetA);
                InitRenderTargetVectorIfNeed(ref targetB);
            }


            if (after == false)
                if (PostProcessStep.StepsBefore.Count == 0)
                {
                    stepsResult = input;
                    return;
                }

            if (after == true)
                if (PostProcessStep.StepsAfter.Count == 0)
                {
                    stepsResult = input;
                    return;
                }


            DownsampleToTexture(input, targetA);
            DownsampleToTexture(input, targetB);


            RenderTarget2D currentTarget = targetA;
            bool currentA = true;

            List<PostProcessStep> steps = new List<PostProcessStep>();

            if(after == false)
                steps = new List<PostProcessStep>(PostProcessStep.StepsBefore);

            if (after == true)
                steps = new List<PostProcessStep>(PostProcessStep.StepsAfter);

            foreach (var step in steps)
            {
                step.BackBuffer = currentA? targetB : targetA;
                currentTarget = currentA ? targetA : targetB;
                step.RenderTarget = currentTarget;
                step.Perform();
                currentA = !currentA;
            }

            stepsResult = currentTarget;

        }

        RenderTarget2D reflection;
        void PerformReflection()
        {

            if (normalPath == null) return;

            InitSizedRenderTargetIfNeed(ref reflection, (int)(GetScreenResolution().Y*Graphics.SSRResolutionScale), surfaceFormat: SurfaceFormat.Color);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, reflection.Width, reflection.Height);

            graphics.GraphicsDevice.SetRenderTarget(reflection);

            graphics.GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            ReflectionEffect.Parameters["enableSSR"]?.SetValue(Graphics.EnableSSR);
            ReflectionEffect.Parameters["NormalTexture"]?.SetValue(normalPath);
            ReflectionEffect.Parameters["FrameTexture"]?.SetValue(ForwardOutput);
            ReflectionEffect.Parameters["PositionTexture"]?.SetValue(positionPath);
            ReflectionEffect.Parameters["FactorTexture"]?.SetValue(ReflectivenessOutput);
            ReflectionEffect.Parameters["ScreenHeight"]?.SetValue(reflection.Height);

            spriteBatch.Begin(effect: ReflectionEffect, blendState: BlendState.NonPremultiplied);

            DrawFullScreenQuad(spriteBatch, normalPath);

            spriteBatch.End();

        }

        void DenoiseReflection()
        {
            InitRenderTargetVectorIfNeed(ref ReflectionOutput);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, ReflectionOutput.Width, ReflectionOutput.Height);

            graphics.GraphicsDevice.SetRenderTarget(ReflectionOutput);

            graphics.GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            ReflectionResultEffect.Parameters["ReflectionTexture"]?.SetValue(reflection);
            ReflectionResultEffect.Parameters["FactorTexture"]?.SetValue(ReflectivenessOutput);

            spriteBatch.Begin(effect: ReflectionResultEffect, blendState: BlendState.Opaque);

            DrawFullScreenQuad(spriteBatch, reflection);

            spriteBatch.End();

        }

        void ApplyReflection()
        {
            InitRenderTargetVectorIfNeed(ref ReflectionOutput);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, ReflectionOutput.Width, ReflectionOutput.Height);

            graphics.GraphicsDevice.SetRenderTarget(ReflectionOutput);

            graphics.GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            ReflectionResultEffect.Parameters["screenWidth"]?.SetValue(ReflectionOutput.Width);
            ReflectionResultEffect.Parameters["screenHeight"]?.SetValue(ReflectionOutput.Height);

            spriteBatch.Begin(effect: ReflectionResultEffect, blendState: BlendState.NonPremultiplied);

            DrawFullScreenQuad(spriteBatch, reflection);

            spriteBatch.End();

        }

        void PerformTonemapping()
        {

            InitRenderTargetIfNeed(ref TonemapResult);

            

            TonemapperEffect.Parameters["Gamma"]?.SetValue(Graphics.Gamma);
            TonemapperEffect.Parameters["Exposure"]?.SetValue(Graphics.Exposure);
            TonemapperEffect.Parameters["Saturation"]?.SetValue(Graphics.Saturation);
            TonemapperEffect.Parameters["Brightness"]?.SetValue(Graphics.Brightness);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, TonemapResult.Width, TonemapResult.Height);

            graphics.GraphicsDevice.SetRenderTarget(TonemapResult);

            graphics.GraphicsDevice.Clear(Color.Black);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: TonemapperEffect, blendState: BlendState.NonPremultiplied);

            DrawFullScreenQuad(spriteBatch, stepsResult);

            spriteBatch.End();

        }


        RenderTarget2D ssaoResult;
        void PerformSSAO()
        {
            InitSizedRenderTargetIfNeed(ref ssaoResult, ssaoOutput.Height, DepthFormat.None, SurfaceFormat.Color);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, ssaoOutput.Width, ssaoOutput.Height);

            if (Graphics.EnableSSAO == false)
            {

                graphics.GraphicsDevice.SetRenderTarget(ssaoResult);

                graphics.GraphicsDevice.Clear(Color.White);
                return;
            }

            graphics.GraphicsDevice.SetRenderTarget(ssaoOutput);



            SSAOEffect.Parameters["NormalTexture"]?.SetValue(normalPath);
            SSAOEffect.Parameters["DepthTexture"]?.SetValue(DepthPrepathOutput);
            SSAOEffect.Parameters["PosTexture"]?.SetValue(positionPath);
            SSAOEffect.Parameters["viewPos"]?.SetValue(Camera.finalizedPosition);

            SSAOEffect.Parameters["screenWidth"]?.SetValue(ssaoOutput.Width);
            SSAOEffect.Parameters["screenHeight"]?.SetValue(ssaoOutput.Height);

            SSAOEffect.Parameters["ssaoRadius"]?.SetValue(0.5f);
            SSAOEffect.Parameters["ssaoBias"]?.SetValue(0.025f);
            SSAOEffect.Parameters["ssaoIntensity"]?.SetValue(1.2f);

            SSAOEffect.Parameters["Projection"]?.SetValue(Camera.finalizedProjection);
            SSAOEffect.Parameters["View"]?.SetValue(Camera.finalizedView);

            SSAOEffect.Parameters["Enabled"]?.SetValue(Graphics.EnableSSAO);


            //DrawFullScreenQuad(DepthPrepathOutput, SSAOEffect);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: SSAOEffect, blendState: BlendState.NonPremultiplied);

            DrawFullScreenQuad(spriteBatch, DepthPrepathOutput);

            spriteBatch.End();

            DownsampleToTexture(ssaoOutput, ssaoResult, true);

            //graphics.GraphicsDevice.SetRenderTarget(null);
        }

        void PerformFXAA()
        {

            if (Graphics.EnableAntiAliasing == false)
            {
                outputPath = stepsResult;

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

            fxaaEffect.Parameters["invViewportWidth"].SetValue(1f / stepsResult.Width);
            fxaaEffect.Parameters["invViewportHeight"].SetValue(1f / stepsResult.Height);
            fxaaEffect.Parameters["screenColor"].SetValue(stepsResult);

            // Begin drawing with SpriteBatch
            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;
            spriteBatch.Begin(effect: fxaaEffect, blendState: BlendState.Opaque);

            // Draw a full-screen quad to apply the lighting
            DrawFullScreenQuad(spriteBatch, stepsResult);

            // End the SpriteBatch
            spriteBatch.End();

            outputPath = FxaaOutput;

        }

        void PerformCompose()
        {
            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, (int)GetScreenResolution().X, (int)GetScreenResolution().Y);

            graphics.GraphicsDevice.SetRenderTarget(ComposedOutput);

            int lutSize = 0;

            if(LUT!=null)
                if(LUT.IsDisposed==false)
                    lutSize = LUT.Height;


            ComposeEffect.Parameters["ssaoResolution"].SetValue(new Vector2(ssaoOutput.Width, ssaoOutput.Height));

            ComposeEffect.Parameters["ColorTexture"].SetValue(TonemapResult);
            ComposeEffect.Parameters["SSAOTexture"].SetValue(ssaoResult);
            ComposeEffect.Parameters["ShadowTexture"].SetValue(shadowCasterPathFinal);
            ComposeEffect.Parameters["BloomTexture"].SetValue(bloomSample);
            ComposeEffect.Parameters["Bloom2Texture"].SetValue(bloomSample2);
            ComposeEffect.Parameters["Bloom3Texture"].SetValue(bloomSample3);
            ComposeEffect.Parameters["Bloom3Texture"].SetValue(bloomSample4);
            ComposeEffect.Parameters["LutTexture"].SetValue(LUT);
            ComposeEffect.Parameters["lutSize"].SetValue(lutSize);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: ComposeEffect);

            DrawFullScreenQuad(spriteBatch, TonemapResult);

            spriteBatch.End();
            //graphics.GraphicsDevice.SetRenderTarget(null);
        }

        void CalculateBloom()
        {


            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, bloomSample.Width, bloomSample.Height);
            graphics.GraphicsDevice.SetRenderTarget(bloomSample);

            if (Graphics.EnableBloom == false)
            {
                graphics.GraphicsDevice.Clear(Color.Black);

                DownsampleToTexture(bloomSample, bloomSample2, false);
                DownsampleToTexture(bloomSample2, bloomSample3, false);
                DownsampleToTexture(bloomSample3, bloomSample4, false);

                return;
            }

            BloomEffect.Parameters["screenWidth"].SetValue(bloomSample.Width);
            BloomEffect.Parameters["screenHeight"].SetValue(bloomSample.Height);
            BloomEffect.Parameters["offset"].SetValue(0.95f);


            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(effect: BloomEffect, blendState: BlendState.Opaque, samplerState: SamplerState.AnisotropicClamp);

            DrawFullScreenQuad(spriteBatch, TonemapResult);

            spriteBatch.End();

            DownsampleToTexture(bloomSample, bloomSample2, true);
            DownsampleToTexture(bloomSample2, bloomSample3, true);
            DownsampleToTexture(bloomSample3, bloomSample4, true);

        }
        public static bool performingOcclusionTest = false;
        public void PerformOcclusionTest(List<StaticMesh> meshes)
        {
            performingOcclusionTest = true;

            InitRenderTargetIfNeed(ref occlusionTestPath, DepthFormat.Depth16);

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, occlusionTestPath.Width, occlusionTestPath.Height);

            OcclusionEffect.Parameters["View"].SetValue(Camera.finalizedView);
            OcclusionEffect.Parameters["Projection"].SetValue(Camera.projectionOcclusion);

            

            graphics.GraphicsDevice.SetRenderTarget(occlusionTestPath);
            graphics.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphics.GraphicsDevice.Clear(Color.Black);

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (StaticMesh mesh in meshes)
            {
                mesh.StartOcclusionTest();
            }

            testedMeshes = new List<StaticMesh>(meshes);
            
        }

        public void EndOcclusionTest(List<StaticMesh> meshes)
        {
            foreach (StaticMesh mesh in meshes)
            {
                if(mesh == null) continue;
                if(mesh.Transperent==false)
                    mesh.EndOcclusionTest();
            }

            performingOcclusionTest = false;

        }

        void DownsampleToTexture(Texture2D source, RenderTarget2D target, bool blur = false, float blurRadiusMultiplier = 1)
        {

            graphics.GraphicsDevice.Viewport = new Viewport(0, 0, target.Width, target.Height);
            graphics.GraphicsDevice.SetRenderTarget(target);

            graphics.GraphicsDevice.Clear(Color.Black);





                SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

                BlurEffect.Parameters["screenWidth"].SetValue(source.Width / blurRadiusMultiplier);
                BlurEffect.Parameters["screenHeight"].SetValue(source.Height / blurRadiusMultiplier);

                spriteBatch.Begin(blendState: BlendState.Opaque, effect: blur ? BlurEffect : null);

                DrawFullScreenQuad(spriteBatch, source);

                spriteBatch.End();
            
        }

        internal static void DrawFullScreenQuad(SpriteBatch spriteBatch, Texture2D inputTexture)
        {

            if(inputTexture == null) return;

            // Create a rectangle covering the entire screen
            Rectangle screenRectangle = new Rectangle(0, 0, GameMain.Instance.GraphicsDevice.Viewport.Width, GameMain.Instance.GraphicsDevice.Viewport.Height);

            // Draw the full-screen quad using SpriteBatch
            spriteBatch.Draw(inputTexture, screenRectangle, Color.White);
        }

        private static VertexBuffer vertexBuffer;
        private static IndexBuffer indexBuffer;

        private static void InitializeFullScreenQuad(GraphicsDevice graphicsDevice)
        {
            if (vertexBuffer == null)
            {
                VertexData[] vertices =
                {
            new VertexData(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            new VertexData(new Vector3(-1,  1, 0), new Vector2(0, 0)),
            new VertexData(new Vector3( 1, -1, 0), new Vector2(1, 1)),
            new VertexData(new Vector3( 1,  1, 0), new Vector2(1, 0)),
                };

                vertexBuffer = new VertexBuffer(graphicsDevice, VertexData.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(vertices);

                int[] indices = { 0, 1, 2, 2, 1, 3 };

                indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices);
            }
        }

        static BasicEffect basicEffect;

        internal static void DrawFullScreenQuad(Texture2D inputTexture, Effect effect = null)
        {

            var graphicsDevice = GameMain.Instance.GraphicsDevice;

            InitializeFullScreenQuad(graphicsDevice);

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;



            if (effect != null)
            {
                effect.Parameters["Texture"]?.SetValue(inputTexture);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                }
            }
            else
            {
                if(basicEffect == null)
                basicEffect = new BasicEffect(graphicsDevice)
                {
                    TextureEnabled = true,
                    Texture = inputTexture,
                    VertexColorEnabled = false,
                };

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                }
            }
        }

        void DrawShadowQuad(SpriteBatch spriteBatch, Texture2D inputTexture)
        {
            // Create a rectangle covering the entire screen
            Rectangle screenRectangle = new Rectangle(0, 0, Graphics.shadowMapResolution, Graphics.shadowMapResolution);

            // Draw the full-screen quad using SpriteBatch
            spriteBatch.Draw(inputTexture, screenRectangle, Color.White);
        }

        void InitShadowMap(ref RenderTarget2D target)
        {
            if (shadowMap is not null && shadowMap.Height == Graphics.shadowMapResolution) return;

            DestroyRenderTarget(shadowMap);

            Console.WriteLine("InitShadowMap");

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

        void InitShadowMapClose(ref RenderTarget2D target)
        {
            if (target is not null && target.Height == Graphics.closeShadowMapResolution) return;

            DestroyRenderTarget(target);

            Console.WriteLine("InitShadowMapClose");

            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth16;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                Graphics.closeShadowMapResolution*2,
                Graphics.closeShadowMapResolution,
                false, // No mipmaps
                SurfaceFormat.Single, // Color format
                depthFormat); // Depth format
        }

        void InitShadowMapVeryClose(ref RenderTarget2D target)
        {
            if (target is not null && target.Height == Graphics.veryCloseShadowMapResolution) return;

            DestroyRenderTarget(target);

            Console.WriteLine("InitShadowMapVeryClose");

            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth24;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                Graphics.veryCloseShadowMapResolution,
                Graphics.veryCloseShadowMapResolution,
                false, // No mipmaps
                SurfaceFormat.Single, // Color format
                depthFormat); // Depth format
        }

        void InitShadowMapViemodel(ref RenderTarget2D target)
        {
            if (target is not null && target.Height == Graphics.ViewmodelShadowMapResolution) return;

            DestroyRenderTarget(target);

            Console.WriteLine("InitShadowMapViemodel");

            // Set the depth format based on your requirements
            DepthFormat depthFormat = DepthFormat.Depth24;

            // Create the new render target with the specified depth format
            target = new RenderTarget2D(
                graphics.GraphicsDevice,
                Graphics.ViewmodelShadowMapResolution,
                Graphics.ViewmodelShadowMapResolution,
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
                480,
                480,
                false, // No mipmaps
                SurfaceFormat.HalfSingle, // Color format
                depthFormat, 0, RenderTargetUsage.PreserveContents); // Depth format
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

                    Console.WriteLine("InitRenderTargetIfNeed");

                    DestroyRenderTarget(target);
                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    (int)GetScreenResolution().X,
                    (int)GetScreenResolution().Y,
                    false, // No mipmaps
                    SurfaceFormat.Color, // Color format
                    depthFormat, 0, RenderTargetUsage.PreserveContents); // Depth format
            }
        }

        void InitRenderTargetDepth(ref RenderTarget2D target, DepthFormat depthFormat = DepthFormat.Depth24)
        {
            if (GetScreenResolution().X > 0 && GetScreenResolution().Y > 0)

                if (target is null || target.Width != (int)GetScreenResolution().X || target.Height != (int)GetScreenResolution().Y)
                {

                    Console.WriteLine("InitRenderTargetDepth");

                    DestroyRenderTarget(target);
                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    (int)GetScreenResolution().X,
                    (int)GetScreenResolution().Y,
                    false, // No mipmaps
                    SurfaceFormat.Single, // Color format
                    depthFormat,0, RenderTargetUsage.PreserveContents); // Depth format
                }
        }

        void InitSizedRenderTargetIfNeed(ref RenderTarget2D target, int height, DepthFormat depthFormat = DepthFormat.None, SurfaceFormat surfaceFormat = SurfaceFormat.ColorSRgb)
        {

            float ratio = ((float)GetScreenResolution().X) / ((float)GetScreenResolution().Y);

            int width = (int)(height * ratio);

            if (width > 0 && height > 0)

                if (target is null || target.Width != width || target.Height != height)
                {
                    DestroyRenderTarget(target);

                    Console.WriteLine("InitSizedRenderTargetIfNeed");

                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                        graphics.GraphicsDevice,
                        width,
                        (int)height,
                        false, // No mipmaps
                        surfaceFormat, // Color format
                        depthFormat, 0, RenderTargetUsage.PreserveContents); // Depth format
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

        void InitRenderTargetVectorIfNeed(ref RenderTarget2D target, bool preserve = true)
        {
            if (GetScreenResolution().X > 0 && GetScreenResolution().Y > 0)
                if (target is null || target.Width != (int)GetScreenResolution().X || target.Height != (int)GetScreenResolution().Y)
                {

                    DestroyRenderTarget(target);

                    Console.WriteLine("InitRenderTargetVectorIfNeed");

                    // Set the depth format based on your requirements
                    DepthFormat depthFormat = DepthFormat.Depth24;

                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                        graphics.GraphicsDevice,
                        (int)GetScreenResolution().X,
                        (int)GetScreenResolution().Y,
                        false, // No mipmaps
                        LimitedColorSpace ? SurfaceFormat.Color : SurfaceFormat.HalfVector4, // Color format
                        depthFormat, 0, RenderTargetUsage.PreserveContents); // Depth format



                }

            graphics.ApplyChanges();
            

        }

        void InitRenderTargetVectorSpaceIfNeed(ref RenderTarget2D target)
        {
            if (GetScreenResolution().X > 0 && GetScreenResolution().Y > 0)
                if (target is null || target.Width != (int)GetScreenResolution().X || target.Height != (int)GetScreenResolution().Y)
                {

                    DestroyRenderTarget(target);

                    Console.WriteLine("InitRenderTargetVectorSpaceIfNeed");

                    // Set the depth format based on your requirements
                    DepthFormat depthFormat = DepthFormat.Depth24;

                    // Create the new render target with the specified depth format
                    target = new RenderTarget2D(
                        graphics.GraphicsDevice,
                        (int)GetScreenResolution().X,
                        (int)GetScreenResolution().Y,
                        false, // No mipmaps
                        SurfaceFormat.HalfVector4, // Color format
                        depthFormat, 0, RenderTargetUsage.PreserveContents); // Depth format



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

        [ConsoleCommand("r.asyncPresent")]
        public static void SetAsyncPresent(bool value)
        {
            AsyncPresent = value;
        }

    }
}
