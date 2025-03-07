using RetroEngine;
using RetroEngine.Entities;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{

    [LevelObject("trigger")]
    public class Trigger : TriggerBase
    {

        string target = "";

        string enterAction = "trigger_enter";
        string exitAction = "trigger_exit";

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

            if (entity.Tags.Contains("player"))
            {
                CallActionOnEntsWithName(target, enterAction);
                Logger.Log("enter");
            }

        }

        public override void OnTriggerExit(Entity entity)
        {
            base.OnTriggerExit(entity);

            if (entity.Tags.Contains("player"))
            {
                CallActionOnEntsWithName(target, exitAction);
                Logger.Log("exit");
            }

        }

    }
}
