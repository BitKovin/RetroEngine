using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static RetroEngine.Physics;

namespace RetroEngine
{

    public class MyClosestConvexResultCallback : ClosestConvexResultCallback
    {
        public MyClosestConvexResultCallback(ref Vector3 rayFromWorld, ref Vector3 rayToWorld) : base(ref rayFromWorld, ref rayToWorld)
        {
        }

        public CollisionFlags FlagToRespond = CollisionFlags.None;

        public List<CollisionObject> ignoreList = new List<CollisionObject>();


        public Physics.BodyType BodyTypeMask = Physics.BodyType.All;

        public override bool NeedsCollision(BroadphaseProxy proxy0)
        {

            if (proxy0.ClientObject is CollisionObject collisionObject)
            {
                if (ignoreList.Contains(proxy0.ClientObject)) return false;

                

                if (collisionObject.UserIndex2 > -1)
                {

                    Physics.BodyType bodyType = (Physics.BodyType)collisionObject.UserIndex2;

                    if (bodyType.HasFlag(Physics.BodyType.NoRayTest))
                        return false;

                        if (BodyTypeMask.HasFlag(bodyType) == false)
                        {
                            return false; // Exclude this object from the collision test
                        }
                }

                if (FlagToRespond!= CollisionFlags.None)
                    if(collisionObject.CollisionFlags.HasFlag(FlagToRespond) == false)
                    {
                        return false; // Exclude this object from the collision test
                    }

            }

            return base.NeedsCollision(proxy0);
        }

    }
}
