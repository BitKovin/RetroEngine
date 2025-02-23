using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("delay")]
    public class DelayEntity : Entity
    {

        public Delay delay = new Delay();

        public float DelayTime = 1;

        public string Target = "";
        public string EventName = "";

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            DelayTime = data.GetPropertyFloat("time", DelayTime);
            Target = data.GetPropertyString("target", Target);
            EventName = data.GetPropertyString("eventName", EventName);

            delay.AddDelay(1000000000);

        }

        public override void Update()
        {
            base.Update();

            if (delay.Wait()) return;
            delay.AddDelay(1000000000);

            if (Target != "")
                CallActionOnEntsWithName(Target, EventName);



        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            delay.AddDelay(DelayTime);

        }

    }
}
