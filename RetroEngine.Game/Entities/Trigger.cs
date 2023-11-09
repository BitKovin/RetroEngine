using RetroEngine;
using RetroEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{

    [LevelObject("trigger")]
    public class Trigger : TriggerBase
    {

        public override void OnTriggerEnter(Entity entity)
        {
            base.OnTriggerEnter(entity);

            Console.WriteLine(entity.Position.ToString());

        }

    }
}
