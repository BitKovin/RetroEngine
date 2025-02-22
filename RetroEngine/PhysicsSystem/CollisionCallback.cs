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
        public override float AddSingleResult(ManifoldPoint cp,
            CollisionObjectWrapper colObj0Wrap, int partId0, int index0,
            CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {
            // Early exit if wrappers or their collision objects are null.
            if (colObj0Wrap == null || colObj1Wrap == null)
                return 0;

            var collisionObject0 = colObj0Wrap.CollisionObject;
            var collisionObject1 = colObj1Wrap.CollisionObject;
            if (collisionObject0 == null || collisionObject1 == null)
                return 0;

            // Cache the user objects and their associated entities.
            var rbData0 = (RigidbodyData)collisionObject0.UserObject;
            var rbData1 = (RigidbodyData)collisionObject1.UserObject;
            var entity0 = rbData0.Entity;
            var entity1 = rbData1.Entity;

            // If there's no owner, immediately fire the collision event using colObj1's entity.
            if (owner == null)
            {
                CollisionEvent?.Invoke(colObj0Wrap, colObj1Wrap, entity1, cp);
                return 0;
            }

            // Depending on which entity matches the owner, check for flags and ignore conditions.
            if (entity0 == owner)
            {
                if (collisionObject1.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.NoContactResponse))
                    return 0;

                if (ignore.Contains(entity1))
                    return 0;

                CollisionEvent?.Invoke(colObj0Wrap, colObj1Wrap, entity1, cp);
            }
            else
            {
                if (collisionObject0.CollisionFlags.HasFlag(BulletSharp.CollisionFlags.NoContactResponse))
                    return 0;

                if (ignore.Contains(entity0))
                    return 0;

                CollisionEvent?.Invoke(colObj1Wrap, colObj0Wrap, entity0, cp);
            }

            return 0;
        }
    }
}
