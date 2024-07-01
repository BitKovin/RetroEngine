using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Events
{
    [LevelObject("event_start")]
    internal class event_start : Entity
    {

        string target = "";

        string action = "start";

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            target = data.GetPropertyString("target");
            action = data.GetPropertyString("actionName");

        }

        public override void Start()
        {
            base.Start();

            var ents = Level.GetCurrent().FindAllEntitiesWithName(target);

            foreach (var ent in ents)
            {
                ent?.OnAction(action);
            }

        }

    }
}
