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

        public static float ShadowBias = 0.002f;
        public static int shadowMapResolution = 2048*2;
        public static int closeShadowMapResolution = 2048;

        public static Matrix LightViewProjection;
        public static Matrix LightViewProjectionClose;
        public static float LightDistance = 100;
        public static float CloseLightDistance = 50;

        public static bool EnablePostPocessing = true;
        public static bool TextureFiltration = true;
        public static bool AnisotropicFiltration = true;
        public static bool EnableAntiAliasing = true;

        public static bool EnableBloom = false;

        public static Vector3 lightlocation = new Vector3();

        public static BoundingFrustum DirectionalLightFrustrum = new BoundingFrustum(Matrix.Identity);

        public static void UpdateDirectionalLight()
        {
            DirectionalLightFrustrum.Matrix = GetLightView() * GetLightProjection();
        }

        public static Matrix GetLightProjection()
        {

            return Matrix.CreateOrthographic(LightDistance, LightDistance, -100, 100);
        }

        public static Matrix GetCloseLightProjection()
        {

            return Matrix.CreateOrthographic(CloseLightDistance, CloseLightDistance, -100, 100);
        }

        public static Matrix GetLightView()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid() - LightDirection * 20f, GetCameraPositionByPixelGrid(), MathHelper.FindLookAtRotation(new Vector3(), LightDirection).GetUpVector());
        }

        static Vector3 GetCameraPositionByPixelGrid()
        {
            Vector3 pos = Camera.position + Camera.rotation.GetForwardVector().XZ().Normalized() * LightDistance/5;

            //ector3 pos = Camera.position - new Vector3(0, 1, 0);

            // Calculate step based on shadow map resolution and light distance
            float step = 1f/2f;

            // Adjust the position using the calculated step
            pos *= step;
            pos.Floor();
            pos /= step;

            lightlocation = pos;

            return pos;
        }

    }
}
