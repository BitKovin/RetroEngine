using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class LightManager
    {

        public static int MAX_POINT_LIGHTS = 15;

        static List<PointLightData> pointLights = new List<PointLightData>();

        public static PointLightData[] FinalPointLights = new PointLightData[MAX_POINT_LIGHTS];

        public static void AddPointLight(PointLightData pointLight)
        {
            pointLights.Add(pointLight);
        }
        
        public static void ClearPointLights()
        {
            pointLights.Clear();
        }

        public static void PrepareLightSources()
        {
            pointLights = pointLights.OrderBy(l => Vector3.Distance(l.Position,Camera.position)).ToList();

            for(int i = 0; i < MAX_POINT_LIGHTS; i++) 
            {
                if(pointLights.Count <= i)
                {
                    FinalPointLights[i] = new PointLightData();
                }
                else
                {
                    FinalPointLights[i] = pointLights[i];
                }
            }

        }

        public struct PointLightData
        {
            public Vector3 Position = new Vector3(0, 0, 0);
            public Vector3 Color = new Vector3(0,0,0);
            public float Radius = 0;

            public PointLightData()
            {
            }
        }

    }
}
