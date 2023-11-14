using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{

    public enum RayFlags
    {
        None = 0x0,
        NoRayTest = 0x1
    }

    public class MyClosestRayResultCallback : ClosestRayResultCallback
    {
        public MyClosestRayResultCallback(ref Vector3 rayFromWorld, ref Vector3 rayToWorld) : base(ref rayFromWorld, ref rayToWorld)
        {
        }

        public override bool NeedsCollision(BroadphaseProxy proxy0)
        {

            if (proxy0.ClientObject is CollisionObject collisionObject)
            {
                RayFlags rayFlags = (RayFlags)collisionObject.UserIndex;

                if (collisionObject.UserIndex > -1)
                {
                    if (rayFlags.HasFlag(RayFlags.NoRayTest))
                    {
                        return false; // Exclude this object from the collision test
                    }
                }
            }

            return base.NeedsCollision(proxy0);
        }

    }
}
