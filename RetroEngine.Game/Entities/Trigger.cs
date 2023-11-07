using Engine;
using Engine.Entities;
using RetroEngine.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{

    [LevelObject("trigger")]
    public class Trigger : Entity
    {

        CollisionCallback collisionCallback = new CollisionCallback();

        public override void Start()
        {
            base.Start();

            body.CollisionFlags = BulletSharp.CollisionFlags.NoContactResponse;

            collisionCallback.CollisionEvent += OnTriggerEntered;

        }

        private void OnTriggerEntered(BulletSharp.CollisionObjectWrapper thisObject, BulletSharp.CollisionObjectWrapper collidedObject, Entity collidedEntity, BulletSharp.ManifoldPoint contactPoint)
        {
            if (collidedEntity is null) return;

            if(collidedEntity.GetType() == typeof(Player)) 
            {

                Console.WriteLine("trigger");

            }

        }

        public override void Update()
        {
            base.Update();

            Physics.Physics.PerformContactCheck(body, collisionCallback);

        }

    }
}
