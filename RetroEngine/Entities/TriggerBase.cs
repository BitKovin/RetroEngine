using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Helpers;
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

        int disableUpdateTicks = 2;

        public TriggerBase() : base() 
        {
            ManualBounds = true;
        }

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

            List<Vector3> verts = new List<Vector3>();

            foreach (var mesh in meshes)
            {
                foreach (var vert in mesh.GetMeshVertices())
                    verts.Add(vert);
            }

            Bounds = BoundingBox.CreateFromPoints(verts);

        }

        private void TriggerEntered(BulletSharp.CollisionObjectWrapper thisObject, BulletSharp.CollisionObjectWrapper collidedObject, Entity collidedEntity, BulletSharp.ManifoldPoint contactPoint)
        {

            if (collidedEntity is null) return;

            if (collidedEntity.Tags.Contains(collideToTag))
            {
                if(entities.Contains(collidedEntity) == false)
                    entities.Add(collidedEntity);
            }

        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            if (CheckBoundsCollisionToTargetEntities())
                disableUpdateTicks = 3;

            //DrawDebug.Box(Bounds.Min, Bounds.Max, Vector3.UnitY, 0.01f);
            //DrawDebug.Text(Bounds.GetCenter(), disableUpdateTicks.ToString(), 0.01f);

            disableUpdateTicks--;



            if(disableUpdateTicks == 0)
            {
                foreach (var entity in entities)
                {
                    OnTriggerExit(entity);
                }

                entities.Clear();
            }


            if (disableUpdateTicks <= 0) return;

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


        protected bool CheckBoundsCollisionToTargetEntities()
        {
            var entities = Level.GetCurrent().GetEntities();

            foreach (var entity in entities)
            {

                if (entity.Bounds.Contains(Bounds) != ContainmentType.Disjoint)
                {
                    if (entity.Tags.Contains(collideToTag))
                        return true;
                }

            }
            return false;
        }
    }
}
