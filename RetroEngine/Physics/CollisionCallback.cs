using BulletSharp;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Physics
{

    public delegate void CollisionEventHandler(CollisionObjectWrapper thisObject, CollisionObjectWrapper collidedObject, Entity collidedEntity, ManifoldPoint contactPoint);

    public class CollisionCallback : ContactResultCallback
    {

        public event CollisionEventHandler CollisionEvent;

        public override double AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {

            CollisionEvent?.Invoke(colObj0Wrap, colObj1Wrap, colObj1Wrap.CollisionObject.UserObject as Entity, cp);

            return 0;
        }
    }
}
