using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.ParticleSystem;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        ParticleSystemEnt trail;

        public float ImpactForce = 10;

        bool hited = false;

        public Entity weapon;

        public Bullet() 
        {
            mesh.EmissionPower = 1;
            mesh.CastShadows = false;

            //meshes.Add(mesh);

        }

        Vector3 startRotation;

        protected override void LoadAssets()
        {
            //return;
            base.LoadAssets();

            mesh.LoadFromFile("models/weapons/bullet/bullet.obj");
            ParticleSystemEnt.Preload("bulletTrail");
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

            trail = ParticleSystemEnt.Create("bulletTrail");
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
            //trail.StopAll();
            trail.Destroy(1.4f);

        }

        public override void Update()
        {
            base.Update();



            MyClosestRayResultCallback hit;

            if (hited) return;

            if (!destroyDelay.Wait())
            {
                Destroy();
                return;
            }

            hit = Physics.LineTrace(OldPos.ToNumerics(), Position.ToNumerics(), ignoreObjects, PhysicsSystem.BodyType.GroupHitTest);


            if (hit.HasHit)
            {

                Entity ent = ((RigidbodyData)hit.CollisionObject.UserObject).Entity;

                if (ent == null) return;

                ent.OnPointDamage(Damage, hit.HitPointWorld, Rotation.GetForwardVector(), this, weapon);

                hit.CollisionObject?.Activate();

                hited = true;

                Destroy();


                //trail.StopAll();

                Speed = 0;
                meshes.Clear();

                if (hit.CollisionObject == null)
                {
                    return;
                }

                RigidBody rigidBody = (RigidBody)hit.CollisionObject;

                var data = rigidBody.GetData();

                if (data != null)
                {
                    if(data.Value.Surface == "default")
                    {
                        GlobalParticleSystem.EmitAt("hitDust", hit.HitPointWorld, MathHelper.FindLookAtRotation(Vector3.Zero, hit.HitNormalWorld), new Vector3(0, 0, float.Max(Damage / 10f,1)));
                    }
                }

                rigidBody?.ApplyCentralImpulse(startRotation.GetForwardVector().ToNumerics() * Damage / 2f * ImpactForce);

                return;

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
