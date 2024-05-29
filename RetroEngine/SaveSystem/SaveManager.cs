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
                        EntitySaveData data = entity.GetSaveData();
                        saveData.Add(data);
                    }
                }
            }

            return saveData;
        }

    }

    public struct EntitySaveData
    {
        public string Name;
        public string className;
        public int id;
    }

    public struct LevelSaveData
    {
        public string levelName;
        public List<EntitySaveData> entities;
    }

}
