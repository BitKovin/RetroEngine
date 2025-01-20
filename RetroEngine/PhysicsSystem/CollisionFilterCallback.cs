using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.PhysicsSystem
{

    

    internal class CollisionFilterCallback : OverlapFilterCallback
    {

        internal static Dictionary<BroadphaseProxy, CollisionObject> savedValues = new Dictionary<BroadphaseProxy, CollisionObject>();
        internal static HashSet<BroadphaseProxy> savedProxies = new HashSet<BroadphaseProxy>();

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

            BodyType collidesWith1 = (BodyType)colObj0.UserIndex;
            BodyType bodyType1 = (BodyType)colObj0.UserIndex2;

            BodyType collidesWith2 = (BodyType)colObj1.UserIndex;
            BodyType bodyType2 = (BodyType)colObj1.UserIndex2;

            bool test1 = ShouldCollide(collidesWith1, bodyType2);
            bool test2 = ShouldCollide(collidesWith2, bodyType1);

            return test1 && test2;
        }

        CollisionObject GetCollisionObjectFromProxy(BroadphaseProxy proxy)
        {

            if(savedProxies.Contains(proxy)) return savedValues[proxy];

            savedValues.TryAdd(proxy, (CollisionObject)proxy.ClientObject);
            savedProxies.Add(proxy);
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
            savedProxies.Clear();
            savedProxies.EnsureCapacity(10000);
        }

    }
}
