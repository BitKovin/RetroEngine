using BulletSharp;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{

    public class MyClosestRayResultCallback : ClosestRayResultCallback
    {
        public MyClosestRayResultCallback(ref Vector3 rayFromWorld, ref Vector3 rayToWorld) : base(ref rayFromWorld, ref rayToWorld)
        {
        }

        public CollisionFlags FlagToRespond = CollisionFlags.None;

        public PhysicsSystem.BodyType BodyTypeMask = PhysicsSystem.BodyType.GroupAll;

        public List<CollisionObject> ignoreList = new List<CollisionObject>();

        public Entity entity;

        public bool DisableCustomChecks = false;

        public override float AddSingleResult(ref LocalRayResult rayResult, bool normalInWorldSpace)
        {

            if(DisableCustomChecks)
                return base.AddSingleResult(ref rayResult, normalInWorldSpace);

            if (rayResult.CollisionObject != null)
            {

                var obj = rayResult.CollisionObject.UserObject;

                if (obj != null)
                {
                    var data = (RigidbodyData)obj;

                    Entity hitEnt = data.Entity;

                    if (hitEnt != null)
                        entity = hitEnt;
                }
            }


            return base.AddSingleResult(ref rayResult, normalInWorldSpace);
        }

        public override bool NeedsCollision(BroadphaseProxy proxy0)
        {

            if(DisableCustomChecks)
                return base.NeedsCollision(proxy0);

            CollisionObject collisionObject = CollisionFilterCallback.GetCollisionObjectFromProxy(proxy0);

            if (collisionObject != null)
            {
                if (ignoreList.Contains(proxy0.ClientObject)) return false;


                PhysicsSystem.BodyType bodyType = collisionObject.GetBodyType();

                if(bodyType== BodyType.None) return false;

                if ((int)collisionObject.GetBodyType() > 0)
                {

                    if(bodyType.HasFlag(PhysicsSystem.BodyType.NoRayTest))
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
