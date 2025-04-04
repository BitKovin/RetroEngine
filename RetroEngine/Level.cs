﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine;
using RetroEngine.Entities;
using RetroEngine.Map;
using RetroEngine.NavigationSystem;
using RetroEngine.PhysicsSystem;
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

        internal List<string> DeletedIds = new List<string>();


        internal List<string> DeletedNames = new List<string>();

        List<StaticMesh> renderList = new List<StaticMesh>();

        List<Entity> pendingAddEntity = new List<Entity>();

        List<StaticMesh> allMeshes = new List<StaticMesh>();

        static string pendingLevelChange = null;

        public static string LoadingLevel = "";

        public static bool ChangingLevel = true;

        public bool OcclusionCullingEnabled = false;

        Dictionary<string, int> LayerIds = new Dictionary<string, int>();

        List<int> renderLayers = new List<int>();

        public string Name = "";

        public delegate void LevelEvent(Level level, string name, string payload);

        public event LevelEvent OnLevelEvent;

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

        public Entity[] GetEntities()
        {
            return entities.ToArray();
        }

        public static Level GetCurrent()
        {
            return GameMain.Instance.curentLevel;
        }

        public void TriggerLevelEvent(string name, string payload)
        {
            OnLevelEvent?.Invoke(this, name, payload);
        }

        internal static bool LoadPendingLevel()
        {
            if (pendingLevelChange != null)
            {


                GameMain.Instance.WaitForFramePresent();


                Stats.StartRecord("waiting for physics");
                GameMain.physicsTask.Wait();
                Stats.StopRecord("waiting for physics");

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

            LoadingLevel = name;

            string path = AssetRegistry.FindPathForFile(name);

            if (File.Exists(path) == false)
            {
                Logger.Log($"failed to find level {path}");
                pendingLevelChange = null;
                ChangingLevel = false;
                return;
            }

            MapData.ClearMeshData();

            bool rebuild = false;

            AssetRegistry.LoadLevelReferences();


            if (name != GetCurrent().Name) 
            {
                AssetRegistry.ConstantCache.Clear();
                AssetRegistry.ClearAllTextures();
                AssetRegistry.UnloadBanks();
                AssetRegistry.UnloadVideos();
                StaticMesh.ClearCache();

                rebuild = true;
            }
#if RELEASE
#else
            rebuild = true;
#endif

            Time.DeltaTime = 0.001f;

            Navigation.WaitForProcess();

            LoadingScreen.Update(0.02f);

            GameMain.Instance.paused = false;

            List<Entity> list = new List<Entity>();
            list.AddRange(GameMain.Instance.curentLevel.entities);

            foreach (Entity entity in list)
            {
                entity.Destroy();
            }

            NavigationSystem.Recast.RemoveAllObstacles();

            Physics.Simulate();

            Physics.ResetWorld();
            Physics.Simulate();

            UiElement.Viewport.ClearChild();


            Navigation.ClearNavData();
            NPCBase.ResetStaticData();

            AssetRegistry.AllowGeneratingMipMaps = true;

            LoadingScreen.Update(0.1f);


            MapData mapData = MapParser.MapParser.ParseMap(path);

            LoadingScreen.Update(0.2f);

            GameMain.Instance.curentLevel = mapData.GetLevel();

            GameMain.Instance.curentLevel.Name = name;

            LoadingScreen.Update(0.7f);


            Physics.Simulate();

            GameMain.Instance.curentLevel.LoadAssets();

            LoadingScreen.Update(0.8f);

            AssetRegistry.WaitForAssetsToLoad();

            LoadingScreen.Update(0.85f);

            Level.GetCurrent().RenderPreparation();

            GameMain.Instance.curentLevel.StartEnities();


            AssetRegistry.WaitForAssetsToLoad();

            Navigation.RebuildConnectionsData();


            if (rebuild)
                NavigationSystem.Recast.BuildNavigationData();

            AssetRegistry.AllowGeneratingMipMaps = false;

            LoadingScreen.Update(0.9f);

            GameMain.Instance.OnLevelChanged();

            GameMain.Instance.curentLevel.LoadAssets();
            AssetRegistry.WaitForAssetsToLoad();
            GameMain.SkipFrames = 5;

            LoadingScreen.Update(0.95f);

            Level.GetCurrent().LazyStartEnities(true);

            GC.Collect();

            LoadingScreen.Update(1f);

            GC.Collect();

            GetCurrent().RenderPreparation();

            if(GameMain.Instance.IsGameWindowFocused() == false)
                GameMain.Instance.FlashWindow();

            GC.Collect();

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
            Entity[] list = entities.OrderBy(e=> e.StartOrder).ToArray();
            foreach (Entity entity in list)
            {
                entity.Start();
            }
        }

        public virtual void LazyStartEnities(bool all = false)
        {
            Entity[] list = entities.ToArray();
            foreach (Entity entity in list)
            {
                if(entity.LazyStarted == false)
                {
                    entity.LazyStarted = true;
                    entity.LazyStart();

                    if(all == false)
                    return;
                }
            }
        }

        public virtual void Update()
        {

            Entity[] list;

            list = entities.ToArray();

            foreach (Entity entity in list)
                if (entity.UpdateWhilePaused && GameMain.Instance.paused || GameMain.Instance.paused == false)
                    entity.Update();
        }

        public virtual void AsyncUpdate()
        {

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount / 2;
            Entity[] list;
            lock (entities)
            {
                list = entities.ToArray();
            }

            list = list.OrderBy(e => e.AsyncUpdateOrder).ToArray();

            Parallel.ForEach(list, options, entity =>
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
            options.MaxDegreeOfParallelism = Environment.ProcessorCount / 3;
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

            DrawDebug.FinalizeCommands();

            //Camera.finalizedPosition = Camera.position;
            //Camera.finalizedRotation = Camera.rotation;
            GameMain.Instance.render.EndOcclusionTest(Render.testedMeshes);

            Graphics.UpdateDirectionalLight();

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = Environment.ProcessorCount;

            Entity[] list = entities.ToArray();

            foreach(Entity entity in list)
            { 
                if (entity != null)
                    if (renderLayers.Contains(entity.Layer) || Level.ChangingLevel || GameMain.SkipFrames>0 || entity.AlwaysFinalizeFrame)
                    {

                        entity.FinalizeFrame();

                        foreach (StaticMesh mesh in entity.meshes.ToArray())
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
            List<StaticMesh> maskedMeshes = new List<StaticMesh>();

            allMeshes.Clear();

            foreach (Entity ent in list)
            {
                if(ent == null) continue;
                if (renderLayers.Contains(ent.Layer) == false && Level.ChangingLevel == false && GameMain.SkipFrames == 0) continue;
                if (ent.loadedAssets == false || ent.Visible == false) continue;
                if (ent.meshes != null)
                {
                    foreach (StaticMesh mesh in ent.meshes)
                    {

                        if (mesh.Visible == false) continue;

                        allMeshes.Add(mesh);

                        if (mesh.inFrustrum == false && mesh.CastShadows == false) continue;

                        if(mesh.Masked)
                        {
                            maskedMeshes.Add(mesh);
                        }
                        else if (mesh.Transperent)
                            transperentMeshes.Add(mesh);
                        else
                            renderList.Add(mesh);

                        

                    }
                }
            }


            renderList = renderList.OrderBy(m => Vector3.Distance(m.GetClosestToCameraPosition(), Camera.position) + (m.Static ? 0 : 1000) - m.DistanceSortingRadius).ToList();

            transperentMeshes = transperentMeshes.OrderByDescending(m => Vector3.Distance(m.GetClosestToCameraPosition(), Camera.position) - m.DistanceSortingRadius).ToList();
            maskedMeshes = maskedMeshes.OrderByDescending(m => Vector3.Distance(m.GetClosestToCameraPosition(), Camera.position) - m.DistanceSortingRadius).ToList();

            renderList.AddRange(transperentMeshes);

            maskedMeshes.AddRange(renderList);

            renderList = maskedMeshes;

            LightManager.PrepareLightSources();
            LightManager.ClearPointLights();

        }

        internal List<StaticMesh> GetAllOpaqueMeshes()
        {
            return allMeshes.Where(m => m.Transperent == false).ToList();

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

            Entity[] list;

            list = entities.ToArray();

            foreach (Entity ent in list)
            {
                if (LoadedAssetsThisFrame < 1)
                    if (ent != null)
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
            if (ent == null) return null;
            entities.Add(ent);
            ent.Id = GetIdForNewEntity(ent) ;
            entityID += 1;

            OnLevelEvent += (Level level, string name, string payload)=> { ent.OnLevelEvent(level, name, payload); };

            return ent;
        }

        Dictionary<Type, int> typeCounter = new Dictionary<Type, int>();

        string GetIdForNewEntity(Entity entity)
        {

            Type type = entity.GetType();

            if(typeCounter.ContainsKey(type))
            {
                typeCounter[type]++;
                return entity.ClassName + "-" + entity.name + "-" + typeCounter[type].ToString();
            }

            typeCounter.Add(type, 0);
            return entity.ClassName + "-" + entity.name + "-" + typeCounter[type].ToString();

        }

        public Entity FindEntityByName(string name)
        {
            var ents = FindAllEntitiesWithName(name);

            if(ents.Length!=1)
                return null;

            return ents[0];
        }

        public Entity[] FindAllEntitiesWithName(string name)
        {
            if (name == null || name == "")
                return new Entity[] { };

            List<Entity> result = new List<Entity>();

            Entity[] list;
            lock (entities)
            {
                list = entities.ToArray();
            }

            foreach (Entity ent in list)
            {
                if (ent.name == name)
                    result.Add(ent);
            }

            return result.ToArray();
        }

        public Entity FindEntityById(string id)
        {
            
                var list = entities.ToArray();
                foreach (Entity ent in list)
                {
                    if (ent.Id == id)
                        return ent;
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
