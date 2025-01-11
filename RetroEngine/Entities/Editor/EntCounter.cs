using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Editor
{
    [LevelObject("counter")]
    public class EntCounter : Entity
    {

        [JsonInclude]
        public int num = 0;

        public int targetNum = 0;

        string target = "";
        string eventName = "reached";

        public EntCounter()
        {
            SaveGame = true;
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            num = (int)data.GetPropertyFloat("startNum", num);
            targetNum = (int)data.GetPropertyFloat("targetNum", targetNum);

            target = data.GetPropertyString("target", target);
            eventName = data.GetPropertyString("onReached", eventName);

        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            if (action =="add")
            {
                num++;
                CheckNum();
            }

            if (action == "sub")
            {
                num--;
                CheckNum();
            }

            Console.WriteLine(action + "  " + num);

        }

        void CheckNum()
        {
            if(num == targetNum)
            {
                var targets = Level.GetCurrent().FindAllEntitiesWithName(target);

                foreach (var t in targets)
                {
                    t.OnAction(eventName);
                }

            }

        }

    }
}
