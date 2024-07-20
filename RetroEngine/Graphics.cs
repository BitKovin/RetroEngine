using Microsoft.Xna.Framework;
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
        public static Color BackgroundColor = new Color(0.15f, 0.15f, 0.2f);
        public static Vector3 LightColor = new Vector3(1,1,1);

        public static float ShadowBias = -0.000f;//0025f
        public static int shadowMapResolution = 2048*2;
        public static int closeShadowMapResolution = 2048;
        public static int veryCloseShadowMapResolution = 2048;
        public static int ViewmodelShadowMapResolution = 2048;

        public static bool GeometricalShadowsEnabled = true;

        public static bool DynamicSunShadowsEnabled = true;

        public static bool ViewmodelShadows = false;

        public static float SSRResolutionScale = 0.8f;
        public static bool EnableSSR = true;

        public static bool EnableSSAO = true;

        public static float Brightness = 1;
        public static float Gamma = 2.4f;
        public static float Exposure = 0.35f;
        public static float Saturation = 0;

        public static float LightDistanceMultiplier = 1;
        public static Matrix LightViewProjection;
        public static Matrix LightViewProjectionViewmodel;
        public static Matrix LightViewProjectionClose;
        public static Matrix LightViewProjectionVeryClose;
        public static float LightDistance = 150;
        public static float CloseLightDistance = 51;
        public static float VeryCloseLightDistance = 16;

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

        public static Vector3 lightlocation = new Vector3();

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

            return Matrix.CreateOrthographic(LightDistance* LightDistanceMultiplier, LightDistance* LightDistanceMultiplier, -100, 100);
        }

        public static Matrix GetCloseLightProjection()
        {

            return Matrix.CreateOrthographic(CloseLightDistance * LightDistanceMultiplier, CloseLightDistance * LightDistanceMultiplier, -100, CloseLightDistance);
        }

        public static Matrix GetVeryCloseLightProjection()
        {

            return Matrix.CreateOrthographic(VeryCloseLightDistance * LightDistanceMultiplier, VeryCloseLightDistance * LightDistanceMultiplier, -100, VeryCloseLightDistance);
        }

        public static Matrix GetLightView()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(LightDistance * LightDistanceMultiplier), GetCameraPositionByPixelGrid(LightDistance * LightDistanceMultiplier) + LightDirection, GetLightUpVector());
        }

        public static Matrix GetLightViewViewmodel()
        {
            return Matrix.CreateLookAt(Camera.position + Camera.Forward*0.5f, Camera.position + Camera.Forward * 0.5f + LightDirection, GetLightUpVector());
        }

        public static Matrix GetLightProjectionViewmodel()
        {

            if (Graphics.ViewmodelShadows == false) return Matrix.Identity;

            return Matrix.CreateOrthographic(1.5f, 1.5f, -100, 4);
        }

        static Vector3 GetLightUpVector()
        {
            return new Vector3(0, 0, 1).RotateVector(Vector3.UnitY, Camera.rotation.Y+45);
        }

        public static Matrix GetLightViewClose()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(CloseLightDistance * LightDistanceMultiplier / 1.5f), GetCameraPositionByPixelGrid(CloseLightDistance * LightDistanceMultiplier / 1.5f) + LightDirection, GetLightUpVector());
        }
        public static Matrix GetLightViewVeryClose()
        {

            if(DynamicSunShadowsEnabled)
            {
                return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(VeryCloseLightDistance * LightDistanceMultiplier / 1.5f), GetCameraPositionByPixelGrid(VeryCloseLightDistance * LightDistanceMultiplier / 1.5f) + LightDirection, GetLightUpVector());
            }else
            {
                return new Matrix(0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);
            }

            
        }

        static Vector3 GetCameraPositionByPixelGrid(float lightDistance)
        {

            float hFactor = 1f - Math.Abs(Camera.rotation.GetForwardVector().Y);

            Vector3 pos = Camera.position+ Camera.rotation.GetForwardVector().XZ().Normalized() * lightDistance / 2f * hFactor;

            //return pos;

            //ector3 pos = Camera.position - new Vector3(0, 1, 0);

            // Calculate step based on shadow map resolution and light distance
            float step = 0.1f;

            // Adjust the position using the calculated step
            pos /= step;
            pos.Floor();
            pos *= step;

            lightlocation = pos;

            return pos;
        }

    }
}
