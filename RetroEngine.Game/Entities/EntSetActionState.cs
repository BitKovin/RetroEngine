using RetroEngine.Game.Entities.Player;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{
    [LevelObject("setActionState")]
    public class EntSetActionState : Entity
    {


        bool newState = false;

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            newState = data.GetPropertyBool("newState", newState);

        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            PlayerGlobal.Instance.InAction = newState;

            Console.WriteLine(newState);

        }

    }
}
