using BulletXNA;
using BulletXNA.BulletCollision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.PhysicsSystem
{

    

    internal class CollisionFilterCallback : MultiSapOverlapFilterCallback
    {

        internal static Dictionary<BroadphaseProxy, CollisionObject> savedValues = new Dictionary<BroadphaseProxy, CollisionObject>();

        public override bool NeedBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {

            if(proxy0==null || proxy1 == null)
                return false;



            // Retrieve user indices from collision objects
            var colObj0 = GetCollisionObjectFromProxy(proxy0);
            var colObj1 = GetCollisionObjectFromProxy(proxy1);

            if (colObj0 == null || colObj1 == null)
                return false;

            if (colObj0.CollisionFlags.HasFlag(CollisionFlags.NoContactResponse)) return false;
            if (colObj1.CollisionFlags.HasFlag(CollisionFlags.NoContactResponse)) return false;

            BodyType collidesWith1 = colObj0.GetCollisionMask();
            BodyType bodyType1 = colObj0.GetBodyType();

            BodyType collidesWith2 = colObj1.GetCollisionMask();
            BodyType bodyType2 = colObj1.GetBodyType();

            bool test1 = ShouldCollide(collidesWith1, bodyType2);
            bool test2 = ShouldCollide(collidesWith2, bodyType1);

            return test1 && test2;
        }

        public static CollisionObject GetCollisionObjectFromProxy(BroadphaseProxy proxy)
        {

            if(savedValues.ContainsKey(proxy)) return savedValues[proxy];

            savedValues.TryAdd(proxy, (CollisionObject)proxy.ClientObject);
            return savedValues[proxy];

        }

        private bool ShouldCollide(BodyType collidesWith, BodyType bodyType)
        {
            // Use bitwise AND to check if any flag is set in both collidesWith and bodyType
            return (collidesWith & bodyType) != 0;
        }

        internal static void ResetCache()
        {
            savedValues.Clear();
            savedValues.EnsureCapacity(10000);
        }

    }
}
