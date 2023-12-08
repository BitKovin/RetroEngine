using BulletSharp;
using Microsoft.Xna.Framework;
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

        public Bullet() 
        {
            mesh.EmissionPower = 0.45f;
            mesh.CastShadows = false;

            meshes.Add(mesh);

        }

        Vector3 startRotation;

        protected override void LoadAssets()
        {

            base.LoadAssets();

            mesh.LoadFromFile("models/weapons/bullet/bullet.obj");

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

            Destroy();
        }

        public override void Update()
        {
            base.Update();

            if (!destroyDelay.Wait())
            {
                Destroy();
                return;
            }

            var hit = Physics.LineTrace(OldPos.ToNumerics(), Position.ToNumerics(), ignoreObjects);

            if (hit.HasHit)
            {

                Entity ent = hit.CollisionObject.UserObject as Entity;

                if (ent == null) return;

                hit.CollisionObject?.Activate();
                RigidBody.Upcast(hit.CollisionObject)?.ApplyCentralImpulse(startRotation.GetForwardVector().ToNumerics() * Damage / 2f);

                ent.OnPointDamage(Damage, hit.HitPointWorld, Rotation.GetForwardVector(), this, this);

                Logger.Log(ent.name);

                Destroy();
            }

            OldPos = Position;
            Position += Rotation.GetForwardVector() * Speed * Time.deltaTime;

        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            mesh.Position = Position;
            mesh.Rotation = startRotation;
            mesh.Scale = new Vector3(1f-(Time.gameTime - SpawnTime) / LifeTime);
        }

        public override void LateUpdate()
        {

            base.LateUpdate();

        }

    }
}
