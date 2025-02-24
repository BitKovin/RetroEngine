﻿using BulletSharp;
using RetroEngine.Map;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    public class TriggerBase : Entity
    {

        CollisionCallback collisionCallback = new CollisionCallback();

        public string collideToTag = "player";

        List<Entity> entities = new List<Entity>();

        public override void Start()
        {
            base.Start();

            foreach (RigidBody body in bodies)
            {

                body.CollisionFlags = BulletSharp.CollisionFlags.NoContactResponse;
                body.SetBodyType(PhysicsSystem.BodyType.NoRayTest | PhysicsSystem.BodyType.World);
            }
            collisionCallback.CollisionEvent += TriggerEntered;
            collisionCallback.owner = this;
            collisionCallback.collidesWith = BodyType.CharacterCapsule;
            //meshes[0].Transperent = true;
        }

        private void TriggerEntered(BulletSharp.CollisionObjectWrapper thisObject, BulletSharp.CollisionObjectWrapper collidedObject, Entity collidedEntity, BulletSharp.ManifoldPoint contactPoint)
        {

            if (collidedEntity is null) return;

            if (collidedEntity.Tags.Contains(collideToTag))
            {
                entities.Add(collidedEntity);
            }

        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            List<Entity> oldEntities = new List<Entity>(entities);
            entities.Clear();

            foreach (RigidBody body in bodies)
            {

                Physics.PerformContactCheck(body, collisionCallback);
            }

            foreach (var entity in oldEntities)
            {
                if (entity is null) continue;

                if (entities.Contains(entity) == false)
                    OnTriggerExit(entity);
            }

            foreach (Entity entity in entities)
            {
                if (entity is null) continue;

                if (oldEntities.Contains(entity) == false)
                    OnTriggerEnter(entity);

            }
        }

        public virtual void OnTriggerEnter(Entity entity)
        {
        }

        public virtual void OnTriggerExit(Entity entity)
        {

        }

    }
}
