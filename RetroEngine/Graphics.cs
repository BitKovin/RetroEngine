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
        public static Vector3 LightDirection = new Vector3 (-1, -1, 0);
        public static Color BackgroundColor = new Color(0.15f,0.15f,0.2f);
    }
}
