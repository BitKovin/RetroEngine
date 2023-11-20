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
        public static float DirectLighting = 0.5f;
        public static float GlobalLighting = 0.3f;
        public static Vector3 LightDirection = new Vector3 (-1f, -1, -0.2f);
        public static Color BackgroundColor = new Color(0.15f,0.15f,0.2f);

        public static float ShadowBias = 0.003f;
        public static int shadowMapResolution = 2048;

        public static Matrix LightViewProjection;
        public static float LightDistance = 100;

        public static bool EnablePostPocessing = false;

        public static Matrix GetLightProjection()
        {
            
            return Matrix.CreateOrthographic(LightDistance, LightDistance, -30, 100);
        }

        public static Matrix GetLightView()
        {
            return Matrix.CreateLookAt(GetCameraPositionByPixelGrid(), GetCameraPositionByPixelGrid() + LightDirection, new Vector3(1, 0, 0));
        }

        static Vector3 GetCameraPositionByPixelGrid()
        {
            Vector3 pos = Camera.position;

            float step = 0.5f; 

            pos*=step;
            pos.Round();
            pos /= step;

            return pos;
        }

    }
}
