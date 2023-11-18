using RetroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class Level : IDisposable
    {
        public List<Entity> entities;

        int entityID = 0;

        List<StaticMesh> renderList = new List<StaticMesh>();

        List<Entity> pendingAddEntity = new List<Entity>();

        public Level()
        {
            entities = new List<Entity>();
        }

        public virtual void Start()
        {
            Physics.Start();
        }

        public static Level GetCurrent()
        {
            return GameMain.inst.curentLevel;
        }

        public static void LoadFromFile(string name)
        {

            List<Entity> list = new List<Entity>();
            list.AddRange(GameMain.inst.curentLevel.entities);

            foreach (Entity entity in list)
            {
                entity.Destroy();
            }

            Navigation.ClearNavData();

            GameMain.inst.curentLevel = MapParser.MapParser.ParseMap(AssetRegistry.FindPathForFile(name)).GetLevel();
            GameMain.inst.curentLevel.StartEnities();
            Navigation.RebuildConnectionsData();

            GameMain.inst.OnLevelChanged();
        }

        public void UpdatePending()
        {
            foreach(Entity entity in pendingAddEntity)
            {
                entities.Add(entity);
            }
            pendingAddEntity.Clear();
        }

        public virtual void StartEnities()
        {
            Entity[] list = entities.ToArray();
            foreach (Entity entity in list)
            {
                entity.Start();
            }
        }

        public virtual void Update()
        {
            

            Entity[] list = entities.ToArray();

            foreach (Entity entity in list)
                if(entity.UpdateWhilePaused&&GameMain.inst.paused|| GameMain.inst.paused == false)
                    entity.Update();
        }

        public virtual void AsyncUpdate()
        {

            Entity[] list = entities.ToArray();

            Parallel.ForEach(list, entity =>
            {
                if (entity.UpdateWhilePaused && GameMain.inst.paused || GameMain.inst.paused == false)
                    entity.AsyncUpdate();
            });
        }

        public virtual void LateUpdate()
        {

            Entity[] list = entities.ToArray();

            foreach (Entity entity in list)
                if (entity.LateUpdateWhilePaused && GameMain.inst.paused || GameMain.inst.paused == false)
                    entity.LateUpdate();
        }

        public virtual void RenderPreparation()
        {
            Parallel.ForEach(entities, entity =>
            {
                foreach (StaticMesh mesh in entity.meshes)
                    mesh.RenderPreparation();
            });

            renderList = new List<StaticMesh>();

            foreach (Entity ent in entities)
            {
                if (ent.meshes != null)
                {
                    foreach (StaticMesh mesh in ent.meshes)
                    {
                        if (mesh.Transperent)
                            renderList.Add(mesh);
                        else
                            renderList.Insert(0, mesh);
                    }
                }
            }

        }

        public virtual List<StaticMesh> GetMeshesToRender()
        {
            return renderList;
        }

        public int GetNextEntityID()
        {
            entityID++;
            return entityID;
        }

        public Entity AddEntity(Entity ent)
        {
            pendingAddEntity.Add(ent);

            return ent;
        }

        public void Dispose()
        {
            foreach (Entity ent in entities)
                ent.Dispose();

            entities.Clear();
            entities = null;
        }
    }
}
