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

        public Bullet() 
        {
            mesh.LoadFromFile("models/weapons/bullet/bullet.obj");

            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            meshes.Add(mesh);

            body = Physics.CreateSphere(this, 0.06f,0.03f);

            body.Gravity = new System.Numerics.Vector3(0,0,0);


            body.CcdSweptSphereRadius = 0.02f;
            body.CcdMotionThreshold = 0.1f;

        }

        Vector3 startRotation;


        public override void Start()
        {
            base.Start();

            startRotation = Rotation;

            body.SetRotation(Quaternion.CreateFromYawPitchRoll(startRotation.X, startRotation.Y, startRotation.Z));

            body.ApplyCentralImpulse(Rotation.GetForwardVector().ToNumerics() * 5f);


            collisionCallback.owner = this;
            collisionCallback.ignore = ignore;

            foreach(Entity e in ignore)
            {
                body.SetIgnoreCollisionCheck(e.body,true);
            }

            collisionCallback.CollisionEvent += Hit;

        }

        private void Hit(BulletSharp.CollisionObjectWrapper thisObject, BulletSharp.CollisionObjectWrapper collidedObject, Entity collidedEntity, BulletSharp.ManifoldPoint contactPoint)
        {
            if (thisObject == collidedObject)
                Console.WriteLine("wtf");

            Console.WriteLine($"me: {(thisObject.CollisionObject.UserObject as Entity).GetType()}   other: {(collidedObject.CollisionObject.UserObject as Entity).GetType()}");
            Destroy();
        }

        public override void Update()
        {
            base.Update();

            Physics.PerformContactCheck(body, collisionCallback);

        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            mesh.Position = Position;
            mesh.Rotation = Rotation;

            body.SetRotation(Physics.ToQuaternion(startRotation.ToNumerics()));

        }

    }
}
