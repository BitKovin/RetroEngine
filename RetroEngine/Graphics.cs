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

        public static float ShadowBias = -0.0001f;//0025f
        public static int shadowMapResolution = 2048*2;
        public static int closeShadowMapResolution = 2048;
        public static int veryCloseShadowMapResolution = 2048;

        public static float Gamma = 2.4f;
        public static float Exposure = 0.4f;
        public static float Saturation = 0;

        public static Matrix LightViewProjection;
        public static Matrix LightViewProjectionClose;
        public static Matrix LightViewProjectionVeryClose;
        public static float LightDistance = 300;
        public static float CloseLightDistance = 63;
        public static float VeryCloseLightDistance = 17;

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

        public static Vector3 lightlocation = new Vector3();

        public static BoundingFrustum DirectionalLightFrustrum = new BoundingFrustum(Matrix.Identity);
        public static BoundingFrustum DirectionalLightFrustrumClose = new BoundingFrustum(Matrix.Identity);
        public static BoundingFrustum DirectionalLightFrustrumVeryClose = new BoundingFrustum(Matrix.Identity);

        public static void UpdateDirectionalLight()
        {
            DirectionalLightFrustrum.Matrix = GetLightView() * GetLightProjection();
            DirectionalLightFrustrumClose.Matrix = GetLightViewClose() * GetCloseLightProjection();
            DirectionalLightFrustrumVeryClose.Matrix = GetLightViewVeryClose() * GetVeryCloseLightProjection();
        }

        public static Matrix GetLightProjection()
        {

            return Matrix.CreateOrthographic(LightDistance, LightDistance, -LightDistance, LightDistance);
        }

        public static Matrix GetCloseLightProjection()
        {

            return Matrix.CreateOrthographic(CloseLightDistance, CloseLightDistance, -100, 100);
        }

        public static Matrix GetVeryCloseLightProjection()
        {

            return Matrix.CreateOrthographic(VeryCloseLightDistance, VeryCloseLightDistance, -100, 100);
        }

        public static Matrix GetLightView()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(LightDistance), GetCameraPositionByPixelGrid(LightDistance) + LightDirection, new Vector3(0,0,1));
        }

        public static Matrix GetLightViewClose()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(CloseLightDistance/1.5f), GetCameraPositionByPixelGrid(CloseLightDistance / 1.5f) + LightDirection, new Vector3(0, 0, 1));
        }
        public static Matrix GetLightViewVeryClose()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(VeryCloseLightDistance / 1.5f), GetCameraPositionByPixelGrid(VeryCloseLightDistance / 1.5f) + LightDirection, new Vector3(0, 0, 1));
        }

        static Vector3 GetCameraPositionByPixelGrid(float lightDistance)
        {

            float hFactor = 1f - Math.Abs(Camera.rotation.GetForwardVector().Y);

            Vector3 pos = Camera.position + Camera.rotation.GetForwardVector().XZ().Normalized() * lightDistance / 2f * hFactor;

            return Camera.position;

            //ector3 pos = Camera.position - new Vector3(0, 1, 0);

            // Calculate step based on shadow map resolution and light distance
            float step = 1f/(shadowMapResolution/LightDistance);

            // Adjust the position using the calculated step
            pos *= step;
            pos.Floor();
            pos /= step;

            lightlocation = pos;

            return pos;
        }

    }
}
