﻿using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{

    public class MyClosestConvexResultCallback : ClosestConvexResultCallback
    {
        public MyClosestConvexResultCallback(ref Vector3 rayFromWorld, ref Vector3 rayToWorld) : base(ref rayFromWorld, ref rayToWorld)
        {
        }

        public CollisionFlags FlagToRespond = CollisionFlags.None;

        public List<CollisionObject> ignoreList = new List<CollisionObject>();

        public override bool NeedsCollision(BroadphaseProxy proxy0)
        {

            if (proxy0.ClientObject is CollisionObject collisionObject)
            {
                if (ignoreList.Contains(proxy0.ClientObject)) return false;

                RayFlags rayFlags = (RayFlags)collisionObject.UserIndex;

                if (collisionObject.UserIndex > -1)
                {
                    if (rayFlags.HasFlag(RayFlags.NoRayTest))
                    {
                        return false; // Exclude this object from the collision test
                    }
                }

                if(FlagToRespond!= CollisionFlags.None)
                    if(collisionObject.CollisionFlags.HasFlag(FlagToRespond) == false)
                    {
                        return false; // Exclude this object from the collision test
                    }

            }

            return base.NeedsCollision(proxy0);
        }

    }
}