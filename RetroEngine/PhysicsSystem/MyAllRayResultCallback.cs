using BulletSharp;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Assimp.Metadata;

namespace RetroEngine
{

    public class MyAllRayResultCallback : RayResultCallback
    {

        public CollisionFlags FlagToRespond = CollisionFlags.None;

        public PhysicsSystem.BodyType BodyTypeMask = PhysicsSystem.BodyType.GroupAll;

        public List<CollisionObject> ignoreList = new List<CollisionObject>();


        public Vector3 RayFromWorld;

        public Vector3 RayToWorld;

        public List<MyClosestRayResultCallback> Hits = new List<MyClosestRayResultCallback>();

        public MyAllRayResultCallback(Vector3 rayFromWorld, Vector3 rayToWorld)
        {
            RayFromWorld = rayFromWorld;
            RayToWorld = rayToWorld;
        }

        public override float AddSingleResult(ref LocalRayResult rayResult, bool normalInWorldSpace)
        {

            MyClosestRayResultCallback resultCallback = new MyClosestRayResultCallback(ref RayFromWorld, ref RayToWorld);
            resultCallback.CollisionObject = rayResult.CollisionObject;

            if (normalInWorldSpace)
            {
                resultCallback.HitNormalWorld = rayResult.HitNormalLocal;
            }
            else
            {
                Matrix4x4 worldTransform = base.CollisionObject.WorldTransform;
                resultCallback.HitNormalWorld = Vector3.Transform(rayResult.HitNormalLocal, worldTransform.GetBasis());
            }

            resultCallback.HitPointWorld = Vector3.Lerp(RayFromWorld, RayToWorld, rayResult.HitFraction);

            resultCallback.ClosestHitFraction = rayResult.HitFraction;

            if (rayResult.CollisionObject != null)
            {


                var userObject = rayResult.CollisionObject.UserObject;

                if (userObject == null) return base.ClosestHitFraction;

                var data = (RigidbodyData)userObject;

                Entity hitEnt = data.Entity;

                if (hitEnt != null)
                    resultCallback.entity = hitEnt;

            }

            Hits.Add(resultCallback);

            return base.ClosestHitFraction;
        }

        public override bool NeedsCollision(BroadphaseProxy proxy0)
        {

            CollisionObject collisionObject = CollisionFilterCallback.GetCollisionObjectFromProxy(proxy0);

            if (collisionObject != null)
            {
                if (ignoreList.Contains(proxy0.ClientObject)) return false;


                PhysicsSystem.BodyType bodyType = (PhysicsSystem.BodyType)collisionObject.UserIndex2;

                if (collisionObject.UserIndex2 > -1)
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
