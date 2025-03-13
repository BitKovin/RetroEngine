using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Graphic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Light
{

    [LevelObject("shadow_point")]
    public class PointShadowCaster : PointLight
    {
        public PointShadowCaster() : base()
        {

            ShadowCaster = true;

            OnlyDynamicObjects = true;

            resolution = 1024;

        }


        public override void Start()
        {
            base.Start();

        }

        internal static void ApplyShadowCastersToShader(Effect effect)
        {

            Vector4[] LightPos = new Vector4[LightManager.MAX_POINT_SHADOW_CASTERS];
            Vector3[] LightColors = new Vector3[LightManager.MAX_POINT_SHADOW_CASTERS];
            float[] LightRadius = new float[LightManager.MAX_POINT_SHADOW_CASTERS];
            float[] LightRes = new float[LightManager.MAX_POINT_SHADOW_CASTERS];
            Vector4[] LightDirections = new Vector4[LightManager.MAX_POINT_SHADOW_CASTERS];
            TextureCube[] textureCubes = new TextureCube[LightManager.MAX_POINT_SHADOW_CASTERS];

            for (int i = 0; i < LightManager.MAX_POINT_SHADOW_CASTERS; i++)
            {

                if (LightManager.FinalShadowCasters[i].sourceData == null) continue;

                LightPos[i] = new Vector4(LightManager.FinalShadowCasters[i].Position, LightManager.FinalShadowCasters[i].InnerMinDot);
                LightDirections[i] = new Vector4(LightManager.FinalShadowCasters[i].Direction, LightManager.FinalShadowCasters[i].MinDot);
                LightColors[i] = LightManager.FinalShadowCasters[i].Color;
                LightRadius[i] = LightManager.FinalShadowCasters[i].Radius;
                LightRes[i] = LightManager.FinalShadowCasters[i].Resolution;

                if(LightManager.FinalShadowCasters[i].sourceData!= null) 
                    effect.Parameters[$"PointLightCubemap{i + 1}"]?.SetValue(LightManager.FinalShadowCasters[i].sourceData.renderTargetCube);

            }

            effect.Parameters["LightPositions"]?.SetValue(LightPos);
            effect.Parameters["LightRadiuses"]?.SetValue(LightRadius);
            effect.Parameters["LightResolutions"]?.SetValue(LightRes);
            effect.Parameters["LightDirections"]?.SetValue(LightDirections);
            effect.Parameters["LightColors"]?.SetValue(LightColors);

        }

    }
}
