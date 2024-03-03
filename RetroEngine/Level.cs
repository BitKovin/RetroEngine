using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine;
using RetroEngine.Entities;
using RetroEngine.Map;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
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

        public string Name = "";

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
            LoadingScreen.Progress = 0;
            LoadingScreen.Draw();

            AssetRegistry.ConstantCache.Clear();
            AssetRegistry.ClearAllTextures();
            //StaticMesh.textures.Clear();

            if(force == false)
            {
                pendingLevelChange = name;
                ChangingLevel = true;
                return;
            }

            Time.deltaTime = 0;

            Navigation.WaitForProcess();

            string path = AssetRegistry.FindPathForFile(name);

            if (File.Exists(path) == false)
            {
                Logger.Log($"failed to find level {path}");
                pendingLevelChange = null;
                return;
            }

            List<Entity> list = new List<Entity>();
            list.AddRange(GameMain.Instance.curentLevel.entities);

            foreach (Entity entity in list)
            {
                entity.Destroy();
            }

            Physics.Update();

            Physics.ResetWorld();
            Physics.Update();

            UiElement.Viewport.childs.Clear();


            Navigation.ClearNavData();
            NPCBase.ResetStaticData();
            
            AssetRegistry.AllowGeneratingMipMaps = true;

            LoadingScreen.Update(0.1f);

            MapData mapData = MapParser.MapParser.ParseMap(path);

            LoadingScreen.Update(0.2f);

            GameMain.Instance.curentLevel = mapData.GetLevel();

            GameMain.Instance.curentLevel.Name = name;

            LoadingScreen.Update(0.7f);

            
            Physics.Update();

            GameMain.Instance.curentLevel.LoadAssets();

            LoadingScreen.Update(0.8f);

            GameMain.Instance.curentLevel.StartEnities();  

            Navigation.RebuildConnectionsData();
            GameMain.Instance.OnLevelChanged();
            ChangingLevel = false;

            LoadingScreen.Update(0.85f);

            AssetRegistry.WaitForAssetsToLoad();

            AssetRegistry.AllowGeneratingMipMaps = false;

            LoadingScreen.Update(0.9f);

            StaticMesh.loadedScenes.Clear();

            GameMain.SkipFrames = 2;

            GC.Collect();

            LoadingScreen.Update(1f);

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
        }

        public virtual void RenderPreparation()
        {

            Graphics.UpdateDirectionalLight();

            Camera.finalizedView = Camera.view;
            Camera.finalizedProjection = Camera.projection;
            Camera.finalizedProjectionViewmodel = Camera.projectionViewmodel;

            Camera.finalizedPosition = Camera.position;
            Camera.finalizedForward = Camera.rotation.GetForwardVector();

            Entity[] list = entities.ToArray();
            try
            {
                Parallel.ForEach(list, entity =>
                {

                    if (renderLayers.Contains(entity.Layer))

                        foreach (StaticMesh mesh in entity.meshes)
                            if (entity is not null)

                                if (mesh is not null)
                                {
                                    mesh.UpdateCulling();

                                    mesh.RenderPreparation();
                                }
                });
            }catch (Exception) { }
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
                OcclusionCullingStart();
        }

        public void EndOcclusionCheck()
        {
            OcclusionCullingEnd();
        }

        void OcclusionCullingStart()
        {

            GameMain.Instance.WaitForFramePresent();

            GameMain.Instance.render.PerformOcclusionTest(renderList);
        }
        void OcclusionCullingEnd()
        {
            GameMain.Instance.render.EndOcclusionTest(renderList);
        }

        static internal int LoadedAssetsThisFrame = 0;

        public bool LoadAssets()
        {

            bool loaded = false;

            List<Entity> list = new List<Entity>(entities);

            foreach (Entity ent in list)
            {
                if (LoadedAssetsThisFrame < 1)
                    if (ent is not null)
                        if (ent.LoadAssetsIfNeeded())
                        {
                            loaded = true;

                        }
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

        [ConsoleCommand("level.layer.visible")]
        public static void SetLayerVisible(int id, bool visible)
        {
            Level.GetCurrent().SetLayerVisibility(id, visible);
            Logger.Log($"Changed layer {id} visibility to {visible}");
        }

        [ConsoleCommand("level.load")]
        public static void LoadLevelFromFile(string name)
        {
            LoadFromFile(name);
        }

        [ConsoleCommand("level.reload")]
        public static void ReloadLevel()
        {
            LoadFromFile(Level.GetCurrent().Name);
        }

    }
}
