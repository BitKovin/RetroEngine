using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RetroEngine.SaveSystem
{
    public static class SaveManager
    {

        internal static LevelSaveData pendingLoadData;

        public static string ProfileName = "player";

        public static bool HasPendingLoad()
        {
            return pendingLoadData.levelName != null;
        }

        public static LevelSaveData GetCurrentLevelSaveData()
        {
            LevelSaveData saveData = new LevelSaveData();
            lock (Level.GetCurrent())
            {

                saveData.levelName = Level.GetCurrent().Name;

                saveData.entities = GetEntitySaveDatas();

                saveData.entityId = Level.GetCurrent().entityID;
                saveData.deletedIDs = new List<string>(Level.GetCurrent().DeletedIds);
                saveData.deletedNames = new List<string>(Level.GetCurrent().DeletedNames);
            }
            return saveData;

        }

        public static void SaveGame(string fileName = "save.sav")
        {

            var data = GetCurrentLevelSaveData();

            JsonSerializerOptions options = new JsonSerializerOptions();

            options.IncludeFields = true;

#if DEBUG
            options.WriteIndented = true;
#endif

            string save = JsonSerializer.Serialize(data, options);

            var stream = File.CreateText(GetProfilePath() + fileName);
            stream.Write(save);
            stream.Close();
            Logger.Log("saved game");

        }

        public static string GetProfilePath()
        {
            return AssetRegistry.ROOT_PATH + "/Profiles/" + ProfileName + "/";
        }

        static List<EntitySaveData> GetEntitySaveDatas()
        {
            
            List<EntitySaveData> saveData = new List<EntitySaveData>();

            lock(Level.GetCurrent().entities)
            {
                var list = Level.GetCurrent().entities.ToArray();

                foreach (Entity entity in list)
                {
                    lock(entity)
                    {
                        if (entity.SaveGame == false) continue;
                        EntitySaveData data = entity.GetSaveData();
                        saveData.Add(data);
                    }
                }
            }

            return saveData;
        }

        public static LevelSaveData GetLevelSaveDataFromFile(string path)
        {
            if(File.Exists(path) == false)
            {
                Logger.Log($"save file with location: {path} not found");
                return new LevelSaveData();
            }
            string text = File.ReadAllText(path);

            JsonSerializerOptions options = new JsonSerializerOptions();

            options.IncludeFields = true;

#if DEBUG
            options.WriteIndented = true;
#endif

            LevelSaveData levelSaveData = JsonSerializer.Deserialize<LevelSaveData>(text, options);

            return levelSaveData;

        }

        public static void LoadGameFromData(LevelSaveData levelSaveData)
        {

            Level.GetCurrent().entityID = levelSaveData.entityId;

            foreach(string id in levelSaveData.deletedIDs)
            {
                Entity targetEntity = Level.GetCurrent().FindEntityById(id);

                if (targetEntity == null) continue;

                if (targetEntity.SaveGame == false) continue;

                targetEntity.Destroy();

            }

            foreach (string name in levelSaveData.deletedNames)
            {
                Entity targetEntity = Level.GetCurrent().FindEntityByName(name);

                if (targetEntity == null) continue;

                if (targetEntity.SaveGame == false) continue;

                targetEntity.Destroy();

            }

            foreach (var entity in levelSaveData.entities)
            {
                Entity targetEntity = Level.GetCurrent().FindEntityByName(entity.Name);

                if(targetEntity==null)
                    targetEntity = Level.GetCurrent().FindEntityById(entity.id);

                if (targetEntity == null)
                {
                    targetEntity = LevelObjectFactory.CreateByTechnicalName(entity.className);
                    Level.GetCurrent().AddEntity(targetEntity);
                    targetEntity.Start();
                    targetEntity.LoadAssetsIfNeeded();

                }

                if (targetEntity == null)
                {
                    Logger.Log($"failed to find or create entity with name '{entity.Name} and id '{entity.id}''");
                    continue;
                }

                targetEntity.LoadData(entity);

            }
        }

        public static void LoadGameFromFile(string name)
        {
            LoadGameFromPath(GetProfilePath() + name);
        }

        public static void LoadGameFromPath(string path)
        {

            LevelSaveData levelSaveData = GetLevelSaveDataFromFile(path);

            pendingLoadData = levelSaveData;

            Level.LoadFromFile(levelSaveData.levelName, false);

        }

        internal static void LoadSaveIfPending()
        {
            if (pendingLoadData.levelName == "" || pendingLoadData.levelName == null)
            {
                return;
            }

            LoadGameFromData(pendingLoadData);

            pendingLoadData = new LevelSaveData();
        }

    }

    public struct EntitySaveData
    {
        public string Name = "";
        public string className = "";
        public string id = "";

        public string saveData ="";

        public EntitySaveData()
        {
        }
    }

    public struct LevelSaveData
    {
        public string levelName;
        public List<EntitySaveData> entities;
        public int entityId;
        public List<string> deletedIDs;
        public List<string> deletedNames;
    }

}
