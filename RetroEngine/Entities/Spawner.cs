using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("spawner")]
    public class Spawner : Entity
    {

        string className = "npc_base";

        public Spawner() 
        {
            SaveGame = true;
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            className = data.GetPropertyString("className", className);

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            var ent = LevelObjectFactory.CreateByTechnicalName(className);

            ent.LoadAssetsIfNeeded();

            ent.Destroy();

        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            var ent = Level.GetCurrent().AddEntity(LevelObjectFactory.CreateByTechnicalName(className));

            if(ent!=null)
            {
                ent.Position = Position;
                ent.Start();
            }

            Destroy();


        }

    }
}
