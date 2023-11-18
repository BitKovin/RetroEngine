using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Brushes
{
    [LevelObject("destructible")]
    public class DestructibleBrush : Entity
    {

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Health = data.GetPropertyFloat("health",30);

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            if(Health<=0)
                Destroy();

        }

    }
}
