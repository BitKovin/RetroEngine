using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{
    public class Bullet : Entity
    {

        StaticMesh mesh = new StaticMesh();

        CollisionCallback collisionCallback = new CollisionCallback();

        public List<Entity> ignore = new List<Entity>();

        List<CollisionObject> ignoreObjects = new List<CollisionObject>();

        Delay destroyDelay = new Delay();

        public float Speed = 60;
        public float LifeTime = 1;
        public float Damage = 15;

        Vector3 OldPos = new Vector3();

        ParticleSystem trail;

        public Bullet() 
        {
            mesh.EmissionPower = 2.0f;
            mesh.CastShadows = false;

            meshes.Add(mesh);

        }

        Vector3 startRotation;

        protected override void LoadAssets()
        {

            base.LoadAssets();

            mesh.LoadFromFile("models/weapons/bullet/bullet.obj");
            ParticleSystem.Preload("trail");
            mesh.texture = AssetRegistry.LoadTextureFromFile("models/weapons/bullet/bullet.png");
            mesh.emisssiveTexture = AssetRegistry.LoadTextureFromFile("models/weapons/bullet/bullet_em.png");

        }

        public override void Start()
        {

            base.Start();

            startRotation = Rotation;


            collisionCallback.owner = this;
            collisionCallback.ignore = ignore;

            foreach(Entity e in ignore)
            {
                if(e is null) continue;
                if(e.bodies is null) continue;

                foreach(RigidBody body in e.bodies)
                {
                    ignoreObjects.Add(body);
                }
            }

            collisionCallback.CollisionEvent += Hit;

            destroyDelay.AddDelay(LifeTime);

            OldPos = Position;

            trail = ParticleSystem.Create("bulletTrail");
            trail.Position = Position;
            trail.Start();

        }

        private void Hit(BulletSharp.CollisionObjectWrapper thisObject, BulletSharp.CollisionObjectWrapper collidedObject, Entity collidedEntity, BulletSharp.ManifoldPoint contactPoint)
        {

            RigidBody hitBody = collidedObject.CollisionObject as RigidBody;

            if(hitBody != null)
            {
                if (ignoreObjects.Contains(collidedObject.CollisionObject)) return;
                hitBody.Activate();
                hitBody.ApplyCentralImpulse(startRotation.GetForwardVector().ToNumerics() * 10f);

                collidedEntity.OnPointDamage(10, Position, startRotation.GetForwardVector());

            }

            Position = contactPoint.PositionWorldOnB;

            Destroy();
        }

        public override void Destroy()
        {
            base.Destroy();

            trail.Position = Position;
            trail.Destroy(1);

        }

        public override void Update()
        {
            base.Update();

            if (!destroyDelay.Wait())
            {
                Destroy();
                return;
            }

            var hit = Physics.LineTrace(OldPos.ToNumerics(), Position.ToNumerics(), ignoreObjects, PhysicsSystem.BodyType.GroupHitTest);

            DrawDebug.Sphere(0.5f,Position, Vector3.UnitX);

            if (hit.HasHit)
            {

                Entity ent = hit.CollisionObject.UserObject as Entity;

                if (ent == null) return;

                hit.CollisionObject?.Activate();
                RigidBody.Upcast(hit.CollisionObject)?.ApplyCentralImpulse(startRotation.GetForwardVector().ToNumerics() * Damage / 2f);

                ent.OnPointDamage(Damage, hit.HitPointWorld, Rotation.GetForwardVector(), this, this);

                Destroy();
            }

            trail.Position = Position;

            OldPos = Position;
            Position += Rotation.GetForwardVector() * Speed * Time.DeltaTime;

        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            mesh.Position = Position;
            mesh.Rotation = startRotation;
            mesh.Scale = new Vector3((float)(1d-(Time.gameTime - SpawnTime) / LifeTime));
        }

        public override void LateUpdate()
        {

            base.LateUpdate();

        }

    }
}
