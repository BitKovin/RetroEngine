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
        public static float GlobalLighting = 0.5f;
        public static Vector3 LightDirection = new Vector3 (-0.2f, -1, 0);
        public static Color BackgroundColor = new Color(0.15f,0.15f,0.2f);

        public static float ShadowBias = 0.05f;

        public static Matrix GetLightProjection()
        {
            
            return Matrix.CreateOrthographic(20,20, -50, 50);
        }

        public static Matrix GetLightView()
        {
            return Matrix.CreateLookAt(Camera.position, Camera.position + LightDirection, new Vector3(1, 0, 0));
        }

    }
}
