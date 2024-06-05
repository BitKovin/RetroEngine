using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine;
using RetroEngine.Entities;
using RetroEngine.Map;
using RetroEngine.SaveSystem;
using RetroEngine.UI;
using SharpFont;
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

        internal int entityID = 0;

        internal List<int> DeletedIds = new List<int>();


        internal List<string> DeletedNames = new List<string>();

        List<StaticMesh> renderList = new List<StaticMesh>();

        List<Entity> pendingAddEntity = new List<Entity>();

        List<StaticMesh> allMeshes = new List<StaticMesh>();

        static string pendingLevelChange = null;

        public static bool ChangingLevel = true;

        public bool OcclusionCullingEnabled = false;

        Dictionary<string, int> LayerIds = new Dictionary<string, int>();

        List<int> renderLayers = new List<int>();

        public string Name = "";

        public Level()
        {

            entities = new List<Entity>();

            entities.Capacity = 200;
            renderList.Capacity = 400;

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
            //LoadingScreen.Draw();

            //StaticMesh.textures.Clear();

            if (name.EndsWith(".map") == false)
                name += ".map";

            if (force == false)
            {
                pendingLevelChange = name;
                ChangingLevel = true;
                return;
            }

            if (name == GetCurrent().Name) 
            {
                AssetRegistry.ConstantCache.Clear();
                AssetRegistry.ClearAllTextures();
                AssetRegistry.UnloadBanks();
                StaticMesh.ClearCache();
            }
            Time.DeltaTime = 0;

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

            AssetRegistry.WaitForAssetsToLoad();

            LoadingScreen.Update(0.85f);

            GameMain.Instance.curentLevel.StartEnities();


            AssetRegistry.WaitForAssetsToLoad();

            Navigation.RebuildConnectionsData();



            AssetRegistry.AllowGeneratingMipMaps = false;

            LoadingScreen.Update(0.9f);

            StaticMesh.loadedScenes.Clear();

            GameMain.Instance.OnLevelChanged();

            GameMain.Instance.curentLevel.LoadAssets();
            AssetRegistry.WaitForAssetsToLoad();
            GameMain.SkipFrames = 2;

            GC.Collect();

            LoadingScreen.Update(1f);

            ChangingLevel = false;

        }

        public bool TryAddLayerName(string name, int id)
        {
            return LayerIds.TryAdd(name, id);
        }

        public int TryGetLayerId(string name)
        {

            int id = 0;

            if (LayerIds.TryGetValue(name, out id))
            {
                return id;
            }

            return -1;

        }


        public void SetLayerVisibility(string name, bool value)
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
            foreach (Entity entity in pendingAddEntity)
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
            Entity[] list;
            lock (entities)
            {
                list = entities.ToArray();
            }
            foreach (Entity entity in list)
                if (entity.UpdateWhilePaused && GameMain.Instance.paused || GameMain.Instance.paused == false)
                    entity.Update();
        }

        public virtual void AsyncUpdate()
        {

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount - 1;

            lock (entities)
            {
                Entity[] list = entities.ToArray();
            }

            Parallel.ForEach(entities, options, entity =>
            {
                if (entity.UpdateWhilePaused && GameMain.Instance.paused || GameMain.Instance.paused == false)
                    entity.AsyncUpdate();
            });
        }

        Task visualUpdateTask;

        public virtual void StartVisualUpdate()
        {
            visualUpdateTask = Task.Factory.StartNew(VisualUpdate);
            //VisualUpdate();
        }

        public virtual void WaitForVisualUpdate()
        {

            Stats.StartRecord("WaitForVisualUpdate");

            if (visualUpdateTask != null)
                visualUpdateTask.Wait();

            Stats.StopRecord("WaitForVisualUpdate");

        }

        protected virtual void VisualUpdate()
        {
            if (GameMain.Instance.paused) return;
            Stats.StartRecord("VisualUpdate");

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount - 2;
            Entity[] list;

            lock (entities)
            {
                list = entities.ToArray();
            }

            Parallel.ForEach(list, options, entity =>
            {
                if (entity != null)
                    entity.VisualUpdate();
            });

            Stats.StopRecord("VisualUpdate");

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

            GameMain.Instance.render.EndOcclusionTest(Render.testedMeshes);

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount;

            Entity[] list = entities.ToArray();

            foreach(Entity entity in list)
            { 
                if (entity != null)
                    if (renderLayers.Contains(entity.Layer))
                    {

                        entity.FinalizeFrame();

                        foreach (StaticMesh mesh in entity.meshes)
                            if (entity is not null)
                            {
                                if(entity.loadedAssets)
                                if (mesh is not null)
                                {
                                    if (mesh.destroyed) continue;
                                    mesh.UpdateCulling();

                                    mesh.RenderPreparation();
                                }
                            }
                    }
            }

            renderList.Clear();


            List<StaticMesh> transperentMeshes = new List<StaticMesh>();

            allMeshes.Clear();

            foreach (Entity ent in entities)
            {
                if (renderLayers.Contains(ent.Layer) == false) continue;
                if (ent.loadedAssets == false) continue;
                if (ent.meshes != null)
                {
                    foreach (StaticMesh mesh in ent.meshes)
                    {

                        if (mesh.Visible == false) continue;

                        if (mesh.inFrustrum == false && mesh.CastShadows == false) continue;

                        if (mesh.Transperent)
                            transperentMeshes.Add(mesh);
                        else
                            renderList.Add(mesh);

                        allMeshes.Add(mesh);

                    }
                }
            }


            renderList = renderList.OrderBy(m => Vector3.Distance(m.GetClosestToCameraPosition(), Camera.position) + (m.Static ? 0 : 1000)).ToList();

            transperentMeshes = transperentMeshes.OrderByDescending(m => Vector3.Distance(m.GetClosestToCameraPosition(), Camera.position)).ToList();

            renderList.AddRange(transperentMeshes);

            LightManager.PrepareLightSources();
            LightManager.ClearPointLights();

        }

        internal List<StaticMesh> GetAllOpaqueMeshes()
        {

            List<StaticMesh> result = new List<StaticMesh>();

            foreach (StaticMesh mesh in allMeshes)
                if (mesh.Transperent == false)
                    result.Add(mesh);


            return result;

        }

        public void PerformOcclusionCheck()
        {
            if (OcclusionCullingEnabled)
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
            ent.Id = entityID;
            entityID += 1;
            return ent;
        }

        public Entity FindEntityByName(string name)
        {
            if (name == null || name == "")
                return null;

            lock (entities)
            {
                var list = entities.ToArray();
                foreach (Entity ent in entities)
                {
                    if(ent.name==name)
                        return ent;
                }
            }
            return null;
        }

        public Entity FindEntityById(int id)
        {
            lock (entities)
            {
                var list = entities.ToArray();
                foreach (Entity ent in entities)
                {
                    if (ent.Id == id)
                        return ent;
                }
            }
            return null;
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
