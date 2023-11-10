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

namespace RetroEngine
{
    public class Entity : IDisposable
    {
        public Vector3 Position;

        public Vector3 Rotation;

        public List<StaticMesh> meshes = new List<StaticMesh>();


        public RigidBody body;

        public bool UpdateWhilePaused = false;

        public List<string> Tags = new List<string>();

        public Entity()
        {

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
            GameMain.inst.curentLevel.entities.Remove(this);
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
