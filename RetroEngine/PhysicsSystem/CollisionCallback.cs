using BulletSharp;
using RetroEngine;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{

    public delegate void CollisionEventHandler(CollisionObjectWrapper thisObject, CollisionObjectWrapper collidedObject, Entity collidedEntity, ManifoldPoint contactPoint);

    public class CollisionCallback : ContactResultCallback
    {

        

        public event CollisionEventHandler CollisionEvent;

        public Entity owner;

        public List<Entity> ignore = new List<Entity>();

        public BodyType collidesWith = BodyType.GroupCollisionTest;

        private bool ShouldCollide(BodyType collidesWith, BodyType bodyType)
        {
            // Use bitwise AND to check if any flag is set in both collidesWith and bodyType
            return (collidesWith & bodyType) != 0;
        }

        public override bool NeedsCollision(BroadphaseProxy proxy0)
        {
            if (proxy0.ClientObject == null)
                return false;

            var colObj = (CollisionObject)proxy0.ClientObject;

            BodyType bodyType = colObj.GetBodyType();

            return base.NeedsCollision(proxy0) && ShouldCollide(collidesWith, bodyType);
        }

        public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {

            if (colObj0Wrap == null) return 0;
            if (colObj1Wrap == null) return 0;

            if (colObj0Wrap.CollisionObject == null) return 0;
            if (colObj1Wrap.CollisionObject == null) return 0;


            if (owner == null)
            {
                CollisionEvent?.Invoke(colObj0Wrap, colObj1Wrap, colObj1Wrap.CollisionObject.UserObject as Entity, cp);
                return 0;
            }

            if (colObj0Wrap.CollisionObject.UserObject == owner)
            {

                if (colObj1Wrap.CollisionObject.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.NoContactResponse)) return 0;

                if (ignore.Contains(colObj1Wrap.CollisionObject.UserObject as Entity))
                    return 0;

                CollisionEvent?.Invoke(colObj0Wrap, colObj1Wrap, colObj1Wrap.CollisionObject.UserObject as Entity, cp);
            }
            else 
            {
                if (colObj0Wrap.CollisionObject.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.NoContactResponse)) return 0;

                if (ignore.Contains(colObj0Wrap.CollisionObject.UserObject as Entity))
                    return 0;

                CollisionEvent?.Invoke(colObj1Wrap, colObj0Wrap, colObj0Wrap.CollisionObject.UserObject as Entity, cp);
            }

            return 0;
        }
    }
}
