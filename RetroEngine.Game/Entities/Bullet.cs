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

        Delay destroyDelay =new Delay();

        public Bullet() 
        {
            mesh.LoadFromFile("models/weapons/bullet/bullet.obj");

            mesh.texture = AssetRegistry.LoadTextureFromFile("models/weapons/bullet/bullet.png");

            meshes.Add(mesh);

            body = Physics.CreateBox(this, new System.Numerics.Vector3(0.05f),0.03f);

            body.Gravity = new System.Numerics.Vector3(0,0,0);

            body.Friction = 1;

            body.CcdSweptSphereRadius = 0.01f;
            body.CcdMotionThreshold = 0.1f;
            body.Restitution = 0;
        }

        Vector3 startRotation;


        public override void Start()
        {
            base.Start();

            startRotation = Rotation;

            body.SetRotation(Physics.ToQuaternion(startRotation.ToNumerics()));


            collisionCallback.owner = this;
            collisionCallback.ignore = ignore;

            foreach(Entity e in ignore)
            {
                if(e is null) continue;
                if(e.body is null) continue;

                body.SetIgnoreCollisionCheck(e.body,true);
            }

            collisionCallback.CollisionEvent += Hit;

            destroyDelay.AddDelay(1);

        }

        private void Hit(BulletSharp.CollisionObjectWrapper thisObject, BulletSharp.CollisionObjectWrapper collidedObject, Entity collidedEntity, BulletSharp.ManifoldPoint contactPoint)
        {

            RigidBody hitBody = collidedObject.CollisionObject as RigidBody;

            if(hitBody != null)
            {
                hitBody.Activate();
                hitBody.ApplyCentralImpulse(startRotation.GetForwardVector().ToNumerics() * 10f);
            }

            Destroy();
        }

        public override void Update()
        {
            base.Update();

            if(!destroyDelay.Wait())
                Destroy();

            body.LinearVelocity = startRotation.GetForwardVector().ToNumerics() * 60f;

            Physics.PerformContactCheck(body, collisionCallback);


        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            mesh.Position = Position;
            mesh.Rotation = Rotation;
        }

        public override void LateUpdate()
        {

            base.LateUpdate();

        }

    }
}
