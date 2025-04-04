﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Entities.Light;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class LightManager
    {

        public static int MAX_POINT_LIGHTS = 20;

        public static int MAX_POINT_SHADOW_CASTERS = 6;

        static List<PointLightData> pointLights = new List<PointLightData>();

        public static List<PointLightData> FinalPointLights = new List<PointLightData>();

        public static List<PointLightData> FinalShadowCasters = new List<PointLightData>();

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

            var shadowCasterList = pointLights.Where(l => l.sourceData.ShadowCaster).ToList();

            FinalShadowCasters.Clear();
            FinalPointLights.Clear();

            for (int i = 0; i < MAX_POINT_SHADOW_CASTERS; i++)
            {
                if (shadowCasterList.Count <= i)
                {
                    FinalShadowCasters.Add(new PointLightData());
                }
                else
                {
                    FinalShadowCasters.Add(shadowCasterList[i]);
                }
            }

            pointLights = pointLights.Where(l=> l.sourceData.ShadowCaster == false).ToList();

            if (Graphics.GlobalPointLights == false)
            {
                FinalPointLights.AddRange(pointLights);
            }
            else
            {
                for (int i = 0; i < MAX_POINT_LIGHTS; i++)
                {
                    if (pointLights.Count <= i)
                    {
                        FinalPointLights.Add(new PointLightData());
                    }
                    else
                    {
                        FinalPointLights.Add(pointLights[i]);
                    }
                }
            }

        }

        public struct PointLightData
        {
            public Vector3 Position = new Vector3(0, 0, 0);
            public Vector3 Color = new Vector3(0,0,0);
            public Vector3 Direction = new Vector3(0,0,0);
            public float MinDot = 0;
            public float InnerMinDot = 0;
            public float Radius = 0;
            public int Resolution = 256;
            public PointLight sourceData = null;

            public bool visible = true;

            public PointLightData()
            {
            }
        }

    }
}
