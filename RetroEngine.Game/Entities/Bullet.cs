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

        public float ImpactForce = 1;

        public Bullet() 
        {
            mesh.EmissionPower = 1;
            mesh.CastShadows = false;

            meshes.Add(mesh);

        }

        Vector3 startRotation;

        protected override void LoadAssets()
        {
            //return;
            base.LoadAssets();

            mesh.LoadFromFile("models/weapons/bullet/bullet.obj");
            ParticleSystem.Preload("bulletTrail");
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
            //trail.StopAll();
            trail.Destroy(1.4f);

        }

        long WairingForSimulationTick = -1;

        public override void Update()
        {
            base.Update();



            MyClosestRayResultCallback hit;

            if (WairingForSimulationTick>0 && WairingForSimulationTick<=Physics.SimulationTicks)
            {

                Console.WriteLine("a");

                var physHit = Physics.SphereTrace(OldPos.ToNumerics(), Position.ToNumerics(),0.05f, ignoreObjects, PhysicsSystem.BodyType.GroupHitTest);
                if (physHit.HasHit)
                {

                    Entity ent = ((RigidbodyData)physHit.HitCollisionObject.UserObject).Entity;

                    if (ent == null) return;

                    Console.WriteLine(((RigidbodyData)physHit.HitCollisionObject.UserObject).HitboxName);

                    physHit.HitCollisionObject?.Activate();
                    RigidBody.Upcast(physHit.HitCollisionObject)?.ApplyCentralImpulse(startRotation.GetForwardVector().ToNumerics() * Damage / 2f * ImpactForce);

                    
                }

                Destroy();

                return;

            }
            else if(WairingForSimulationTick > 0)
            {
                return;
            }

            if (!destroyDelay.Wait())
            {
                Destroy();
                return;
            }

            hit = Physics.LineTrace(OldPos.ToNumerics(), Position.ToNumerics(), ignoreObjects, PhysicsSystem.BodyType.GroupHitTest);


            if (hit.HasHit)
            {

                WairingForSimulationTick = Physics.SimulationTicks + 1;

                Entity ent = ((RigidbodyData)hit.CollisionObject.UserObject).Entity;

                if (ent == null) return;

                ent.OnPointDamage(Damage, hit.HitPointWorld, Rotation.GetForwardVector(), this, this);

                OldPos -= Rotation.GetForwardVector() * Speed * 0.03f;
                Position += Rotation.GetForwardVector() * Speed * 0.03f;
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
