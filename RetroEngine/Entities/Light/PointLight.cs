using Microsoft.Xna.Framework;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Light
{

    [LevelObject("light_point")]
    public class PointLight : Entity
    {

        BoundingSphere lightSphere = new BoundingSphere();

        public PointLight() 
        {
            LateUpdateWhilePaused = true;
        }

        LightManager.PointLightData lightData = new LightManager.PointLightData();


        public override void FromData(EntityData data)
        {
            base.FromData(data);

            lightData.Color = data.GetPropertyVector("light_color", new Vector3(1,1,1));
            lightData.Radius = data.GetPropertyFloat("radius", 5);
        }

        public override void LateUpdate()
        {
            lightData.Position = Position;

            
            lightSphere.Radius = lightData.Radius;

            if (IsBoundingSphereInFrustum(lightSphere))
                LightManager.AddPointLight(lightData);
        }

        protected Matrix GetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateTranslation(lightData.Position);
            return worldMatrix;
        }

        protected bool IsBoundingSphereInFrustum(BoundingSphere sphere)
        {
            return Camera.frustum.Contains(sphere.Transform(GetWorldMatrix())) != ContainmentType.Disjoint;
        }

    }
}
