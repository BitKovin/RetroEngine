using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("camera_point")]
    internal class CameraPoint : Entity
    {

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Position = data.GetPropertyVectorPosition("origin");

            Logger.Log("loaded");
        }

        public override void Update()
        {
            base.Update();

            Entity target = Level.GetCurrent().FindEntityByName("camera_target");

            if (target != null)
            {
                Camera.rotation = MathHelper.FindLookAtRotation(Position, target.Position);


                Camera.position = Position;
            }

        }

    }
}
