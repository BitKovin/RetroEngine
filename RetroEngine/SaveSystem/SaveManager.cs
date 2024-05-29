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
        public static LevelSaveData GetCurrentLevelSaveData()
        {

            LevelSaveData saveData = new LevelSaveData();

            saveData.levelName = Level.GetCurrent().Name;

            saveData.entities = GetEntitySaveDatas();

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

            var stream = File.CreateText(AssetRegistry.ROOT_PATH + fileName);
            stream.Write(save);
            stream.Close();
            Logger.Log("saved game");

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
            foreach(var entity in levelSaveData.entities)
            {
                Entity targetEntity = Level.GetCurrent().FindEntityByName(entity.Name);

                if(targetEntity==null)
                    targetEntity = Level.GetCurrent().FindEntityById(entity.id);

                if(targetEntity == null)
                {
                    Logger.Log($"failed to find entity with name '{entity.Name} and id '{entity.id}''");
                    continue;
                }

                targetEntity.LoadData(entity);

            }
        }

        public static void LoadGameFromPath(string path)
        {

            LevelSaveData levelSaveData = GetLevelSaveDataFromFile(path);

            if (levelSaveData.levelName == "")
            {

                Logger.Log("failed to load save game");
                return;
            }

            LoadGameFromData(levelSaveData);

        }

    }

    public struct EntitySaveData
    {
        public string Name;
        public string className;
        public int id;

        public string saveData;

    }

    public struct LevelSaveData
    {
        public string levelName;
        public List<EntitySaveData> entities;
    }

}
