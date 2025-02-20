using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Brushes
{
    [LevelObject("groupBrush")]
    public class GroupBrush : Entity
    {

        public GroupBrush() 
        {
            mergeBrushes = true;
            Static = true;
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            AffectNavigation = data.GetPropertyBool("affectsNavigation", AffectNavigation);

        }

    }
}
