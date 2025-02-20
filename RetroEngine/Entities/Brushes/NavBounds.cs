using Microsoft.Xna.Framework;
using RetroEngine.Map;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Brushes
{
    [LevelObject("navBounds")]
    public class NavBounds : Entity
    {

        internal static Vector3 min = Vector3.Zero;
        internal static Vector3 max = Vector3.Zero;

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            foreach(var mesh in meshes)
            {

                var box = BoundingBox.CreateFromPoints(mesh.GetMeshVertices());

                min = box.Min;
                max = box.Max;

                min -= Vector3.One;
                max += Vector3.One;


            }

            foreach (var mesh in meshes)
                mesh.Visible = false;

            foreach(var body in bodies)
                Physics.Remove(body);



        }

        public override void Destroy()
        {
            base.Destroy();

            min = Vector3.Zero;
            max = Vector3.Zero;

        }

    }
}
