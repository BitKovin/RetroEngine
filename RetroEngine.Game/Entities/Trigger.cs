using RetroEngine;
using RetroEngine.Entities;
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

            meshes[0].Transperent = true;
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

            Physics.PerformContactCheck(body, collisionCallback);

        }

    }
}
