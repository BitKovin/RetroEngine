using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Brushes
{
    [LevelObject("navBlock")]
    public class NavBlock : Entity
    {

        public NavBlock() : base()
        {
            SaveGame = false;

            Static = true;

            AffectNavigation = true;

        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            foreach (var mesh in meshes)
            {
                mesh.Visible = false;
                mesh.Transperent = true;
                mesh.Transparency = 0;
            }


            foreach (var body in bodies)
            {
                body.SetCollisionMask(PhysicsSystem.BodyType.None);
                body.SetBodyType(PhysicsSystem.BodyType.None);
            }

        }

        public override void Update()
        {
            Destroy();
        }
    }
}
