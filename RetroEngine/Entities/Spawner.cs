using Microsoft.Xna.Framework;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("spawner")]
    public class Spawner : Entity
    {

        string className = "npc_base";

        string target = "";
        string onSpawnEventName = "";
        string onDespawnEventName = "";

        [JsonInclude]
        public bool isEntAlive = false;

        [JsonInclude]
        public bool active = true;

        public Spawner() 
        {
            SaveGame = true;
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            className = data.GetPropertyString("className", className);

            target = data.GetPropertyString("target", target);

            onSpawnEventName = data.GetPropertyString("onSpawned", onSpawnEventName);
            onDespawnEventName = data.GetPropertyString("onDespawned", onSpawnEventName);

            Vector3 importRot = data.GetPropertyVector("angles", Vector3.Zero);

            Rotation = EntityData.ConvertRotation(importRot, true);

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            var ent = LevelObjectFactory.CreateByTechnicalName(className);

            ent.LoadAssetsIfNeeded();

        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            if(action == "despawned")
            {

                if (target == "") return;


                isEntAlive = false;

                CallActionOnEntsWithName(target, onDespawnEventName);

                return;
            }

            if (active == false) return;




            var ent = LevelObjectFactory.CreateByTechnicalName(className); 

            if(ent!=null)
            {
                ent.Position = Position;
                ent.Rotation = Rotation;
                ent.SetOwner(this);

                ent.Start();

                Level.GetCurrent().AddEntity(ent);

            }

            CallActionOnEntsWithName(target, onSpawnEventName);

            //active = false;
            isEntAlive = true;


        }

    }
}
