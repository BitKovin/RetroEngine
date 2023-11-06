using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework;
using RetroEngine;
using RetroEngine.Map;

namespace Engine
{
    public class Entity : IDisposable
    {
        public Vector3 Position;

        public Vector3 Rotation;

        public List<StaticMesh> meshes = new List<StaticMesh>();

        public Collision collision;

        public RigidBody body;

        public Entity()
        {
            collision = new Collision();

            //PhysicsBody = Physics.Physics.CreateBox(0, 0, 0, 0, this);

        }


        public virtual void Start()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void FromData(EntityData data)
        {

        }

        public virtual void AsyncUpdate()
        {

        }

        public virtual void LateUpdate()
        {

        }

        protected void UpdateCollision()
        {

        }

        public virtual void Destroy()
        {

        }

        public void Dispose()
        {
            AssetRegistry.RemoveReference(this);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }
}
