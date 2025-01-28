using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class Graphics
    {
        public static float DirectLighting = 0.7f;
        public static float GlobalLighting = 0.15f;
        public static Vector3 LightDirection = new Vector3(-1f, -1, -0.2f);
        public static Vector3 SkyLightColor = new Vector3(0.72f, 0.72f, 1);
        public static Vector3 LightColor = new Vector3(1,1,1);

        public static Point Resolution = new Point(1280, 720);

        public static float ShadowBias = -0.000f;//0025f
        public static int shadowMapResolution { get { return (int)(2048 * 2 * ShadowResolutionScale); } }
        public static int closeShadowMapResolution { get { return (int)(2048 * ShadowResolutionScale); } }
        public static int veryCloseShadowMapResolution { get { return (int)(2048 * ShadowResolutionScale); } }
        public static int ViewmodelShadowMapResolution { get { return (int)(2048 * ShadowResolutionScale); } }

        public static float ShadowResolutionScale = 1f;


        public static bool GeometricalShadowsEnabled = false;

        public static bool DynamicSunShadowsEnabled = true;

        public static bool ViewmodelShadows = false;


        public static int PointLightShadowQuality = 3;
        public static int DirectionalLightShadowQuality = 3;

        public static float SSRResolutionScale = 0.7f;
        public static bool EnableSSR = true;

        public static bool EnableSSAO = true;

        public static int MipLevel = 0;

        public static float Brightness = 1;
        public static float Gamma = 1.05f;
        public static float Exposure = 0.5f;
        public static float Saturation = 0.0f;

        public static float LightDistanceMultiplier = 1;
        public static Matrix LightViewProjection;
        public static Matrix LightViewProjectionViewmodel;
        public static Matrix LightViewProjectionClose;
        public static Matrix LightViewProjectionVeryClose;
        public static float LightDistance = 200;
        public static float CloseLightDistance = 51;
        public static float VeryCloseLightDistance = 16;

        public static float LightNearDistance = 300;

        public static bool EnablePostPocessing = true;
        public static bool TextureFiltration = true;
        public static bool AnisotropicFiltration = true;
        public static bool EnableAntiAliasing = true;

        public static bool DrawPhysics = false;

        public static bool EnableBloom = true;

        public static bool OpaqueBlending = false;

        public static bool DefaultUnlit = false;

        public static bool DisableBackFaceCulling = false;

        public static bool GlobalPointLights = false;

        public static bool LowLatency = false;

        public static BoundingFrustum DirectionalLightFrustrum = new BoundingFrustum(Matrix.Identity);
        public static BoundingFrustum DirectionalLightFrustrumClose = new BoundingFrustum(Matrix.Identity);
        public static BoundingFrustum DirectionalLightFrustrumVeryClose = new BoundingFrustum(Matrix.Identity);

        public static BoundingFrustum DirectionalLightFrustrumViewmodel = new BoundingFrustum(Matrix.Identity);

        public static bool EarlyDepthDiscard = true;
        public static bool EarlyDepthDiscardShader = true;

        internal static Matrix LightVeryCloseView;
        internal static Matrix LightCloseView;
        internal static Matrix LightView;
        internal static Matrix LightViewmodelView;

        internal static Matrix LightVeryCloseProjection;
        internal static Matrix LightCloseProjection;
        internal static Matrix LightProjection;
        internal static Matrix LightViewmodelProjection;

        public static void UpdateDirectionalLight()
        {
            DirectionalLightFrustrum.Matrix = GetLightView() * GetLightProjection();
            DirectionalLightFrustrumClose.Matrix = GetLightViewClose() * GetCloseLightProjection();

            if (DynamicSunShadowsEnabled)
            {
                DirectionalLightFrustrumVeryClose.Matrix = GetLightViewVeryClose() * GetVeryCloseLightProjection();
            }else
            {
                DirectionalLightFrustrumVeryClose.Matrix = Matrix.Identity;
            }

            DirectionalLightFrustrumViewmodel.Matrix = GetLightViewViewmodel() * GetLightProjectionViewmodel();

            LightVeryCloseView = GetLightViewVeryClose();
            LightCloseView = GetLightViewClose();
            LightView = GetLightView();

            LightViewmodelView = GetLightViewViewmodel();

            LightVeryCloseProjection = GetVeryCloseLightProjection();
            LightCloseProjection = GetCloseLightProjection();
            LightProjection = GetLightProjection();

            LightViewmodelProjection = GetLightProjectionViewmodel();

        }

        public static Matrix GetLightProjection()
        {

            return Matrix.CreateOrthographic(LightDistance* LightDistanceMultiplier, LightDistance* LightDistanceMultiplier, -LightNearDistance, 100);
        }

        public static Matrix GetCloseLightProjection()
        {

            return Matrix.CreateOrthographic(CloseLightDistance * LightDistanceMultiplier, CloseLightDistance * LightDistanceMultiplier, -LightNearDistance, CloseLightDistance);
        }

        public static Matrix GetVeryCloseLightProjection()
        {

            return Matrix.CreateOrthographic(VeryCloseLightDistance * LightDistanceMultiplier, VeryCloseLightDistance * LightDistanceMultiplier, -LightNearDistance, VeryCloseLightDistance);
        }

        public static Matrix GetLightView()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(LightDistance, shadowMapResolution), GetCameraPositionByPixelGrid(LightDistance, shadowMapResolution) + LightDirection, Vector3.UnitZ);
        }

        public static Matrix GetLightViewViewmodel()
        {
            return Matrix.CreateLookAt(Camera.finalizedPosition + Camera.finalizedRotation.GetForwardVector()*0.5f, Camera.finalizedPosition + Camera.finalizedRotation.GetForwardVector() * 0.5f + LightDirection, GetLightUpVector());
        }

        public static Matrix GetLightProjectionViewmodel()
        {

            if (Graphics.ViewmodelShadows == false) return Matrix.Identity;

            return Matrix.CreateOrthographic(1.5f, 1.5f, -4, 4);
        }

        static Vector3 GetLightUpVector()
        {
            return new Vector3(0, 0, 1).RotateVector(Vector3.UnitY, Camera.finalizedRotation.Y+45);
        }

        public static Matrix GetLightViewClose()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(CloseLightDistance, closeShadowMapResolution), GetCameraPositionByPixelGrid(CloseLightDistance, closeShadowMapResolution) + LightDirection, Vector3.UnitZ);
        }
        public static Matrix GetLightViewVeryClose()
        {

            if(DynamicSunShadowsEnabled)
            {
                return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(VeryCloseLightDistance, veryCloseShadowMapResolution), GetCameraPositionByPixelGrid(VeryCloseLightDistance, veryCloseShadowMapResolution) + LightDirection, Vector3.UnitZ);
            }else
            {
                return new Matrix(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);
            }

            
        }

        [ConsoleCommand("g.miplevel")]
        public static void SetMipLevel(int mipLevel)
        {
            MipLevel = mipLevel;
        }

        [ConsoleCommand("g.pointshadows")]
        public static void SetPointLightShadowQuality(int value)
        {
            PointLightShadowQuality = value;
        }

        [ConsoleCommand("g.directshadows")]
        public static void SetDirectionalLightShadowQuality(int value)
        {
            DirectionalLightShadowQuality = value;
        }

        static Vector3 GetCameraPositionByPixelGrid(float lightDistance, float resolution)
        {

            float hFactor = 1f - Math.Abs(Camera.finalizedRotation.GetForwardVector().GetForwardVector().Y);

            Vector3 pos = Camera.finalizedPosition;// + Camera.rotation.GetForwardVector().XZ().Normalized() * lightDistance / 3f * hFactor * Graphics.LightDistanceMultiplier;

            //return pos;

            //ector3 pos = Camera.position - new Vector3(0, 1, 0);

            // Calculate step based on shadow map resolution and light distance
            float step = lightDistance / resolution;

            // Adjust the position using the calculated step
            pos = pos.SnapToGrid(step);

            //DrawDebug.Line(pos - LightDirection * 100, pos + LightDirection*100,null, 0.01f);

            return pos;
        }

        

    }
}
