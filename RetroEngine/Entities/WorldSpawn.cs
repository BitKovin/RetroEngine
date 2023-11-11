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
        public WorldSpawn() {  }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Logger.Log("world created");

        }

    }
}
