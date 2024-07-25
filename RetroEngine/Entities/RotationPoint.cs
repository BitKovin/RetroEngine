using Microsoft.Xna.Framework;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{

    [LevelObject("point_rotation")]
    public class RotationPoint : Entity
    {

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Vector3 importRot = data.GetPropertyVector("angles", Vector3.Zero);

            Rotation = EntityData.ConvertRotation(importRot);

        }
    }
}
