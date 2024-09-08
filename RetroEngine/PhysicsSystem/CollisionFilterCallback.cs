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

        public override bool NeedBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
        {

            if(proxy0==null || proxy1 == null)
                return false;

            // Retrieve user indices from collision objects
            var colObj0 = (CollisionObject)proxy0.ClientObject;
            var colObj1 = (CollisionObject)proxy1.ClientObject;

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

        private bool ShouldCollide(BodyType collidesWith, BodyType bodyType)
        {
            // Use bitwise AND to check if any flag is set in both collidesWith and bodyType
            return (collidesWith & bodyType) != 0;
        }

    }
}
