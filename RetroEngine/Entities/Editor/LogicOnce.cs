using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Editor
{
    [LevelObject("logicOnce")]
    public class LogicOnce : Entity
    {

        string target = "";

        public LogicOnce() : base() 
        {
            SaveGame = true;
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            target = data.GetPropertyString("target", target);

        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            if(target != "")
            {
                CallActionOnEntsWithName(target, action);
                Destroy();
            }

        }

    }
}
