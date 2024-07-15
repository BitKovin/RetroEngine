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
        public WorldSpawn() 
        {
            Static = true;

            ConvexBrush = false;

        }

        List<string> visibleLayers = new List<string>();

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Logger.Log("world created");

            Graphics.LightColor = data.GetPropertyVector("globalLightColor", new Vector3(1, 1, 1));
            Graphics.GlobalLighting = data.GetPropertyFloat("globalLightBrightness", 0.15f);

            Graphics.DirectLighting = data.GetPropertyFloat("directLightBrightness", 0.7f);

            Graphics.LightDirection = data.GetPropertyVector("globalLightDirection", new Vector3(-1f, -1, -0.2f));

            Graphics.DynamicSunShadowsEnabled = data.GetPropertyBool("dynamicSunShadowsEnabled", true);

            Graphics.GeometricalShadowsEnabled = data.GetPropertyBool("geometricalShadowsEnabled", false);

            Vector3 skyColor = data.GetPropertyVector("skyColor", new Vector3(0.15f, 0.15f, 0.2f));

            Graphics.BackgroundColor = new Color(skyColor.X, skyColor.Y, skyColor.Z);


            Level.GetCurrent().TryAddLayerName("Default Layer", 0);

            string listOfLayers = data.GetPropertyString("visibleLayers", "Default Layer, ");

            List<string> layers = new List<string>();

            listOfLayers = listOfLayers.Replace(", ",",");
            layers = listOfLayers.Split(',').ToList();

            foreach (string layer in layers)
            {
                if(layer!=null)
                if(layer.Length>1)
                    visibleLayers.Add(layer);
            }

            Level.GetCurrent().AddEntity(new Sky());


        }

        public override void Start()
        {
            base.Start();

            foreach(string layer in visibleLayers)
            {
                Level.GetCurrent().SetLayerVisibility(layer, true);
            }

        }

    }
}
