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
using static Assimp.Metadata;

namespace RetroEngine
{
    public class Entity : IDisposable
    {

        public bool Static = false;

        public Vector3 Position;

        public Vector3 Rotation;

        public List<StaticMesh> meshes = new List<StaticMesh>();


        public List<RigidBody> bodies = new List<RigidBody>();

        public bool UpdateWhilePaused = false;
        public bool LateUpdateWhilePaused = false;

        public string name = "";

        public List<string> Tags = new List<string>();

        public double SpawnTime = 0;

        Delay destroyDelay = new Delay();

        bool pendingDestroy = false;

        public int Id;

        public float Health = 0;

        public bool loadedAssets = false;

        public int Layer = 0;

        public bool mergeBrushes = false;


        public Entity()
        {

        }


        public virtual void Start()
        {
            SpawnTime = Time.gameTime;
        }

        public virtual void Update()
        {
            if(pendingDestroy)
            if (destroyDelay.Wait() == false)
                Destroy();
        }

        public virtual void FromData(EntityData data)
        {
            Layer = (int)data.GetPropertyFloat("_tb_layer");
            name = data.GetPropertyString("name");


        }

        public virtual void AsyncUpdate()
        {

        }

        public virtual void LateUpdate()
        {

        }

        public virtual void FinalizeFrame()
        {

        }

        public virtual void VisualUpdate()
        {

        }

        public SaveSystem.EntitySaveData GetSaveData()
        {

            SaveSystem.EntitySaveData saveData = new SaveSystem.EntitySaveData();

            saveData.Name = name;
            saveData.id = Id;
            saveData.className = "";
            System.Reflection.MemberInfo info = this.GetType();
            object[] attributes = info.GetCustomAttributes(true);

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is LevelObjectAttribute)
                {
                    saveData.className = ((LevelObjectAttribute)attributes[i]).TechnicalName;
                    break;
                }
            }

            saveData = SaveData(saveData);

            return saveData;

        }

        protected virtual SaveSystem.EntitySaveData SaveData(SaveSystem.EntitySaveData baseData)
        {
            return baseData;
        }

        public virtual void LoadData(SaveSystem.EntitySaveData Data)
        {

        }

        public virtual void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            Health -= damage;
        }

        public virtual void OnPointDamage(float damage, Vector3 point, Vector3 direction, Entity causer = null, Entity weapon = null)
        {
            OnDamaged(damage, causer, weapon);
        }

        protected void UpdateCollision()
        {

        }

        public virtual void Destroy()
        {

            foreach(RigidBody rigidBody in bodies)
            {
                Physics.Remove(rigidBody);
            }

            foreach(StaticMesh mesh in meshes)
            {
                mesh.Destroyed();
            }

            meshes.Clear();

            GameMain.Instance.curentLevel.entities.Remove(this);
            Dispose();
        }

        public virtual void Destroy(float delay)
        {
            destroyDelay.AddDelay(delay);
            pendingDestroy = true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool LoadAssetsIfNeeded()
        {
            if(loadedAssets) return false;

            if (GameMain.CanLoadAssetsOnThisThread() == false)
                return false;

            LoadAssets();

            loadedAssets = true;

            return true;
        }

        protected virtual void LoadAssets()
        {

        }

    }
}
