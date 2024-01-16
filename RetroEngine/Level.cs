using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine;
using RetroEngine.Entities;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Assimp.Metadata;

namespace RetroEngine
{
    public class Level : IDisposable
    {
        public List<Entity> entities;

        int entityID = 0;

        List<StaticMesh> renderList = new List<StaticMesh>();

        List<Entity> pendingAddEntity = new List<Entity>();

        static string pendingLevelChange = null;

        public static bool ChangingLevel = true;

        public bool OcclusionCullingEnabled = false;

        Dictionary<string, int> LayerIds = new Dictionary<string, int>();

        List<int> renderLayers = new List<int>();

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
            return GameMain.Instance.curentLevel;
        }

        public static bool LoadPendingLevel()
        {
            if (pendingLevelChange != null)
            {

                GameMain.Instance.WaitForFramePresent();

                LoadFromFile(pendingLevelChange, true);

                pendingLevelChange = null;
                return true;
            }
            return false;
        }

        public static void LoadFromFile(string name, bool force = false)
        {

            if(force == false)
            {
                pendingLevelChange = name;
                ChangingLevel = true;
                return;
            }

            List<Entity> list = new List<Entity>();
            list.AddRange(GameMain.Instance.curentLevel.entities);

            foreach (Entity entity in list)
            {
                entity.Destroy();
            }

            UiElement.Viewport.childs.Clear();


            Navigation.ClearNavData();
            NPCBase.ResetStaticData();
            
            AssetRegistry.AllowGeneratingMipMaps = true;

            GameMain.Instance.curentLevel = MapParser.MapParser.ParseMap(AssetRegistry.FindPathForFile(name)).GetLevel();

            GameMain.Instance.curentLevel.StartEnities();

            GameMain.Instance.curentLevel.LoadAssets();

            Navigation.RebuildConnectionsData();
            GameMain.Instance.OnLevelChanged();
            ChangingLevel = false;

            AssetRegistry.WaitForAssetsToLoad();

            AssetRegistry.AllowGeneratingMipMaps = false;

        }

        public bool TryAddLayerName(string name, int id)
        {
            return LayerIds.TryAdd(name, id);
        }

        public int TryGetLayerId(string name)
        {

            int id = 0;

            if(LayerIds.TryGetValue(name, out id))
            {
                return id;
            }
            
            return -1;

        }

        public void SetLayerVisibility(string name,bool value)
        {

            int id = TryGetLayerId(name);

            SetLayerVisibility(id, value);
            
        }

        public void SetLayerVisibility(int id, bool value)
        {
            if (value)
            {
                renderLayers.Add(id);
            }
            else
            {
                renderLayers.Remove(id);
            }
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
                if(entity.UpdateWhilePaused&&GameMain.Instance.paused|| GameMain.Instance.paused == false)
                    entity.Update();
        }

        public virtual void AsyncUpdate()
        {

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount*2;

            Entity[] list = entities.ToArray();

            Parallel.ForEach(list,options, entity =>
            {
                if (entity.UpdateWhilePaused && GameMain.Instance.paused || GameMain.Instance.paused == false)
                    entity.AsyncUpdate();
            });
        }

        public virtual void LateUpdate()
        {

            Entity[] list = entities.ToArray();

            foreach (Entity entity in list)
                if (entity.LateUpdateWhilePaused && GameMain.Instance.paused || GameMain.Instance.paused == false)
                    entity.LateUpdate();


            foreach (Entity entity in list)
                foreach (StaticMesh mesh in entity.meshes)
                {
                    if (mesh is null) continue;
                    mesh.UpdateCulling();

                }


        }

        public virtual void RenderPreparation()
        {

            Graphics.UpdateDirectionalLight();

            Entity[] list = entities.ToArray();


            Camera.finalizedView = Camera.view;
            Camera.finalizedProjection = Camera.projection;
            Camera.finalizedProjectionViewmodel = Camera.projectionViewmodel;

            Parallel.ForEach(entities, entity =>
            {

                if (renderLayers.Contains(entity.Layer))

                foreach (StaticMesh mesh in entity.meshes)
                    if(entity is not null)

                    if(mesh is not null) 
                        mesh.RenderPreparation();
            });

            renderList = new List<StaticMesh>();

            List<StaticMesh> transperentMeshes = new List<StaticMesh>();

            foreach (Entity ent in entities)
            {
                if (renderLayers.Contains(ent.Layer) == false) continue;
                if (ent.loadedAssets == false) continue;
                if (ent.meshes != null)
                {
                    foreach (StaticMesh mesh in ent.meshes)
                    {
                        if (mesh.Transperent)
                            transperentMeshes.Add(mesh);
                        else
                            renderList.Add(mesh);
                    }
                }
            }

            renderList = renderList.OrderBy(m => Vector3.Distance(m.useAvgVertexPosition ? m.avgVertexPosition : m.Position, Camera.position)).ToList();

            transperentMeshes = transperentMeshes.OrderByDescending(m => Vector3.Distance(m.useAvgVertexPosition? m.avgVertexPosition : m.Position, Camera.position)).ToList();

            renderList.AddRange(transperentMeshes);

            LightManager.PrepareLightSources();
            LightManager.ClearPointLights();

        }

        public void PerformOcclusionCheck()
        {
            if(OcclusionCullingEnabled)
                OcclusionCulling();
        }

        void OcclusionCulling()
        {
            GameMain.Instance.render.PerformOcclusionTest(renderList);
        }

        public bool LoadAssets()
        {

            bool loaded = false;

            List<Entity> list = new List<Entity>(entities);

            foreach (Entity ent in list)
            {
                if(ent is not null)
                if(ent.LoadAssetsIfNeeded())
                        loaded = true;
            }
            return loaded;
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
            entities.Add(ent);

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
