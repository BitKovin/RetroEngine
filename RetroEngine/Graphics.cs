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

        public static float ShadowBias = 0.006f;
        public static int shadowMapResolution = 2048 * 2;

        public static Matrix LightViewProjection;
        public static float LightDistance = 200;

        public static bool EnablePostPocessing = false;
        public static bool TextureFiltration = false;
        public static bool AnisotropicFiltration = false;

        public static Vector3 lightlocation = new Vector3();

        public static Matrix GetLightProjection()
        {

            return Matrix.CreateOrthographic(LightDistance, LightDistance, -30, 100);
        }

        public static Matrix GetLightView()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid() - LightDirection, GetCameraPositionByPixelGrid(), LightDirection.XZ().Normalized());
        }

        static Vector3 GetCameraPositionByPixelGrid()
        {
            Vector3 pos = Camera.position + new Vector3(0,10,0);
            

            //ector3 pos = Camera.position - new Vector3(0, 1, 0);

            // Calculate step based on shadow map resolution and light distance
            float step = 1f/5f;

            // Adjust the position using the calculated step
            pos *= step;
            pos.Floor();
            pos /= step;

            lightlocation = pos;

            return pos;
        }

    }
}
