using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using Microsoft.Xna.Framework;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assimp.Metadata;

namespace RetroEngine
{

    public class MyClosestConvexResultCallback : ClosestConvexResultCallback
    {
        public MyClosestConvexResultCallback(ref Vector3 rayFromWorld, ref Vector3 rayToWorld) : base(rayFromWorld, rayToWorld)
        {
            Start = rayFromWorld;
            End = rayToWorld;
        }

        public CollisionFlags FlagToRespond = CollisionFlags.None;

        public List<RigidBody> ignoreList = new List<RigidBody>();

        public Vector3 HitShapeLocation = Vector3.Zero;

        public Vector3 Start;
        public Vector3 End;

        public PhysicsSystem.BodyType BodyTypeMask = PhysicsSystem.BodyType.GroupAll;

        public Entity entity;

        public bool DisableCustomChecks = false;

        public override float AddSingleResult(ref LocalConvexResult convexResult, bool normalInWorldSpace)
        {

            if(DisableCustomChecks)
                return base.AddSingleResult(ref convexResult, normalInWorldSpace);

            HitShapeLocation = Vector3.Lerp(Start, End, convexResult.HitFraction);

            if (convexResult.HitCollisionObject != null)
            {

                var userObject = convexResult.HitCollisionObject.UserObject;

                if(userObject == null) return base.AddSingleResult(ref convexResult, normalInWorldSpace);

                var data = (RigidbodyData)userObject;

                Entity hitEnt = data.Entity;

                if (hitEnt != null)
                    entity = hitEnt;

            }

            return base.AddSingleResult(ref convexResult, normalInWorldSpace);
        }

        public override bool NeedsCollision(BroadphaseProxy proxy0)
        {

            if(DisableCustomChecks)
                return base.NeedsCollision(proxy0);

            CollisionObject collisionObject = CollisionFilterCallback.GetCollisionObjectFromProxy(proxy0);

            if (collisionObject != null)
            {
                if (ignoreList.Contains(proxy0.ClientObject)) return false;



                if ((int)collisionObject.GetBodyType() > 0)
                {

                    PhysicsSystem.BodyType bodyType = collisionObject.GetBodyType();

                    if (bodyType == BodyType.None) return false;


                    if (bodyType.HasFlag(PhysicsSystem.BodyType.NoRayTest))
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
