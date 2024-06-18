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
                return true;

            // Retrieve user indices from collision objects
            var colObj0 = (CollisionObject)proxy0.ClientObject;
            var colObj1 = (CollisionObject)proxy1.ClientObject;

            if (colObj0 == null || colObj1 == null)
                return true;

            if (colObj0.UserIndex == -1)
                colObj0.UserIndex = (int)BodyType.CollisionTest;

            if (colObj1.UserIndex == -1)
                colObj1.UserIndex = (int)BodyType.CollisionTest;

            BodyType collidesWith1 = (BodyType)colObj0.UserIndex;
            BodyType bodyType1 = (BodyType)colObj0.UserIndex2;

            BodyType collidesWith2 = (BodyType)colObj1.UserIndex;
            BodyType bodyType2 = (BodyType)colObj1.UserIndex2;

            // Use the custom collision logic to determine if the objects should collide
            return ShouldCollide(collidesWith1, bodyType2) && ShouldCollide(collidesWith2, bodyType1);
        }

        private bool ShouldCollide(BodyType collidesWith, BodyType bodyType)
        {
            // Use bitwise AND to check if any flag is set in both collidesWith and bodyType
            return (collidesWith & bodyType) != 0;
        }
    }
}
