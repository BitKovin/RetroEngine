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

        string collideToTag = "player";

        List<Entity> entities = new List<Entity>();

        public override void Start()
        {
            base.Start();

            body.CollisionFlags = BulletSharp.CollisionFlags.NoContactResponse;
            body.UserIndex = (int)RayFlags.NoRayTest;

            collisionCallback.CollisionEvent += TriggerEntered;

            meshes[0].Transperent = true;
        }

        private void TriggerEntered(BulletSharp.CollisionObjectWrapper thisObject, BulletSharp.CollisionObjectWrapper collidedObject, Entity collidedEntity, BulletSharp.ManifoldPoint contactPoint)
        {
            if (collidedEntity is null) return;

            if (collidedEntity.Tags.Contains(collideToTag))
            {
                entities.Add(collidedEntity);
            }

        }

        public override void Update()
        {
            base.Update();

            List<Entity> oldEntities = new List<Entity>(entities);
            entities.Clear();
            Physics.PerformContactCheck(body, collisionCallback);

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

    }
}
