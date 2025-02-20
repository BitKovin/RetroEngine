using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework;
using RetroEngine;
using RetroEngine.Map;
using RetroEngine.PhysicsSystem;
using static Assimp.Metadata;

namespace RetroEngine
{
    public class Entity : IDisposable
    {

        public bool Static = false;

        public bool DisablePhysicsInterpolation = false;

        [JsonInclude]
        public Vector3 Position;

        [JsonInclude]
        public Vector3 Rotation;

        public List<StaticMesh> meshes = new List<StaticMesh>();


        public List<RigidBody> bodies = new List<RigidBody>();

        public bool UpdateWhilePaused = false;
        public bool LateUpdateWhilePaused = false;

        [JsonInclude]
        public string name = "";

        [JsonInclude]
        public List<string> Tags = new List<string>();

        public double SpawnTime = 0;

        Delay destroyDelay = new Delay();

        bool pendingDestroy = false;

        public string Id = "";

        [JsonInclude]
        public float Health = 0;

        public bool loadedAssets = false;

        public int Layer = 0;

        public bool mergeBrushes = true;

        public bool SaveGame = false;
        public bool SaveAsUnique = false;

        public string ClassName = "";

        public int StartOrder = 0;

        public bool ConvexBrush = true;

        public bool Destroyed = false;

        public bool Visible = true;

        public Vector3 PhysicalVelocity = Vector3.Zero;

        public bool LazyStarted = false;

        public bool AffectNavigation = true;

        [JsonInclude]
        public string OwnerId = "";
        Entity owner;

        public Entity()
        {
            System.Reflection.MemberInfo info = this.GetType();
            object[] attributes = info.GetCustomAttributes(true);

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is LevelObjectAttribute)
                {
                    ClassName = ((LevelObjectAttribute)attributes[i]).TechnicalName;
                    break;
                }
            }
        }

        public Entity GetOwner()
        {
            return owner;
        }
        public void SetOwner(Entity owner)
        {
            this.owner = owner;
            OwnerId = owner.Id;
        }

        public virtual void Start()
        {
            SpawnTime = Time.GameTime;
        }

        public bool HasTag(string tag)
        {
            lock(Tags)
                return Tags.Contains(tag);
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
            name = data.GetPropertyString("targetname");
            SaveAsUnique = data.GetPropertyBool("unique",SaveAsUnique);

        }

        public virtual void AsyncUpdate()
        {

        }

        public virtual void LazyStart()
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

        public virtual void OnAction(string action)
        {

        }

        public SaveSystem.EntitySaveData GetSaveData()
        {

            SaveSystem.EntitySaveData saveData = new SaveSystem.EntitySaveData();

            if(Level.GetCurrent().FindEntityByName(name)==this)
            saveData.Name = name;

            saveData.id = Id;
            saveData.className = "";

            saveData = SaveData(saveData);

            

            JsonSerializerOptions options = new JsonSerializerOptions();

            foreach(var conv in Helpers.JsonConverters.GetAll())
                options.Converters.Add(conv);

            saveData.className = ClassName;

            saveData.saveData = JsonSerializer.Serialize(this,this.GetType(), options);

            

            return saveData;

        }

        protected virtual SaveSystem.EntitySaveData SaveData(SaveSystem.EntitySaveData baseData)
        {
            return baseData;
        }

        public virtual void LoadData(SaveSystem.EntitySaveData Data)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();

            foreach (var conv in Helpers.JsonConverters.GetAll())
                options.Converters.Add(conv);
            
            Entity ent = JsonSerializer.Deserialize(Data.saveData, this.GetType(), options) as Entity;
            if(ent == null)
            {
                Logger.Log("failed to deserialize entity");
                return;
            }

            // Copy data from ent to this, only if the field/property has [JsonInclude] attribute
            Type type = this.GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<JsonIncludeAttribute>() != null)
                {
                    var value = field.GetValue(ent);
                    field.SetValue(this, value);
                }
            }


            owner = Level.GetCurrent().FindEntityById(OwnerId);

        }

        public virtual void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            Health -= damage;
        }

        public virtual void OnPointDamage(float damage, Vector3 point, Vector3 direction, Entity causer = null, Entity weapon = null)
        {
            OnDamaged(damage, causer, weapon);
        }

        public virtual void OnLevelEvent(Level level, string name, string payload)
        {

        }


        public virtual void Destroy()
        {

            Destroyed = true;

            foreach(RigidBody rigidBody in bodies)
            {
                Physics.Remove(rigidBody);
            }

            foreach(StaticMesh mesh in meshes)
            {
                mesh.Destroyed();
            }

            meshes.Clear();

            if (SaveGame)
            {

                if (name != null && name != "" && SaveAsUnique)
                {

                    lock (Level.GetCurrent().DeletedNames)
                    {
                        Level.GetCurrent().DeletedNames.Add(name);
                        Logger.Log("Deleted entity with name " + name);
                    }

                }
                else
                {

                    lock (Level.GetCurrent().DeletedIds)
                    {
                        Level.GetCurrent().DeletedIds.Add(Id);
                        Logger.Log("Deleted entity with id " + Id.ToString());
                    }
                }
            }
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

        public bool LoadAssetsIfNeeded(bool allowAnyThread = false)
        {
            if(loadedAssets) return false;

            if (allowAnyThread == false)
                if (GameMain.CanLoadAssetsOnThisThread() == false)
                    return false;

            LoadAssets();

            loadedAssets = true;

            return true;
        }

        protected virtual void LoadAssets()
        {

        }

        public virtual void DrawDevUi()
        {

        }

        public static void CallActionOnEntsWithName(string name, string eventName)
        {
            var ents = Level.GetCurrent().FindAllEntitiesWithName(name);

            foreach (var ent in ents)
            {
                ent.OnAction(eventName);
            }
        }

        public override string ToString()
        {
            return $"[{GetType().Name}] {name} {Id}";
        }
        [ConsoleCommand("ent.spawn")]
        public static void Console_EntSpawn(string name)
        {
            var entity = LevelObjectFactory.CreateByTechnicalName(name);

            if (entity == null)
            {
                Logger.Log("invalid entity name: " + name);
                return;
            }

            var hit = Physics.SphereTrace(Camera.position, Camera.position + Camera.Forward * 10, 0.5f, bodyType: BodyType.World);

            Vector3 spawnPos = hit.End;

            if(hit.HasHit)
            {
                spawnPos = hit.HitShapeLocation;
            }

            entity.Position = spawnPos;
            entity.Start();
            Level.GetCurrent().AddEntity(entity);
            

        }

    }
}
