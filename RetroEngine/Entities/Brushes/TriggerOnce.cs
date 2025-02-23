using RetroEngine;
using RetroEngine.Entities;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{

    [LevelObject("triggerOnce")]
    public class TriggerOnce : TriggerBase
    {

        string target = "";

        string enterAction = "trigger_enter";
        string exitAction = "trigger_exit";

        [JsonInclude]
        public bool active = true;

        public TriggerOnce()
        {
            SaveGame = true;

            AsyncUpdateOrder = -10;

        }

        public override void Start()
        {
            base.Start();

            if (meshes.Count == 0) return;
            meshes[0].CastShadows = false;
            meshes[0].Visible = false;
        }



        public override void FromData(EntityData data)
        {
            base.FromData(data);

            target = data.GetPropertyString("target");

            enterAction = data.GetPropertyString("onEnterAction", enterAction);
            exitAction = data.GetPropertyString("onExitAction",exitAction);

        }

        public override void OnTriggerEnter(Entity entity)
        {
            base.OnTriggerEnter(entity);

            if (active == false) return;

            if (entity.Tags.Contains("player"))
            {
                var ents = Level.GetCurrent().FindAllEntitiesWithName(target);

                if (ents.Length > 0)
                {
                    System.Threading.Tasks.Parallel.For(0, ents.Length, i =>
                    {
                        var ent = ents[i];
                        if (ent != null)
                        {
                            ent.OnAction(enterAction);
                        }
                    });

                    active = false;
                    Destroy();
                }


            }
        }
        public override void OnTriggerExit(Entity entity)
        {
            base.OnTriggerExit(entity);

            if (entity.Tags.Contains("player"))
            {
                Entity targetEntity = Level.GetCurrent().FindEntityByName(target);
                if (targetEntity != null)
                {
                    targetEntity.OnAction(exitAction);
                }
            }

        }

    }
}
