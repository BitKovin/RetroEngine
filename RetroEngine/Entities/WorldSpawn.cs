using Microsoft.Xna.Framework;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("worldspawn")]
    public class WorldSpawn : Entity
    {
        public WorldSpawn() {  }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Logger.Log("world created");

            Graphics.LightColor = data.GetPropertyVector("globalLightColor", new Vector3(1, 1, 1));
            Graphics.GlobalLighting = data.GetPropertyFloat("globalLightBrightness", 0.15f);
            Graphics.DirectLighting = data.GetPropertyFloat("directLightBrightness", 0.7f);

            Graphics.LightDirection = data.GetPropertyVector("globalLightDirection", new Vector3(-1f, -1, -0.2f));

            Vector3 skyColor = data.GetPropertyVector("skyColor", new Vector3(0.15f, 0.15f, 0.2f));

            Graphics.BackgroundColor = new Color(skyColor.X, skyColor.Y, skyColor.Z);

        }

    }
}
