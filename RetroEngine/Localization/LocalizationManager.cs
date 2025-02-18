using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Localization
{
    public static class LocalizationManager
    {

        static string localizationDirectory = "Localization/";

        static LocalizationProfile localizationProfile = new LocalizationProfile();

        public static string GetStringFromKey(string key)
        {
            return localizationProfile.GetValue(key);
        }

        public static void CreateSampleLocalizationProfile()
        {
            LocalizationProfile localizationProfile = new LocalizationProfile();

            localizationProfile.Name = "sample";
            localizationProfile.Table.Add(new LocalizationEntry("key1", "value1"));
            localizationProfile.Table.Add(new LocalizationEntry("key2", "value2"));

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string text = JsonSerializer.Serialize(localizationProfile, options);

            string path = Path.Combine(AssetRegistry.ROOT_PATH + AssetRegistry.AssetsRoot, localizationDirectory + "sample.loc");

            File.WriteAllText(path, text);

        }

        public static void LoadLocalizationProfile(string name)
        {
            var file = AssetRegistry.GetFileStreamFromPath(AssetRegistry.FindPathForFile("Localization/" + name + ".loc"));

            string text = file.StreamReader.ReadToEnd();

            LocalizationProfile profile = JsonSerializer.Deserialize<LocalizationProfile>(text);

            profile.Build();

            localizationProfile = profile;

        }

    }

    public class LocalizationEntry
    {
        [JsonInclude]
        public string Key { get; set; }

        [JsonInclude]
        public string Value { get; set; }

        public LocalizationEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    public class LocalizationProfile
    {
        public string Name { get; set; }
        public List<LocalizationEntry> Table { get; set; } = new List<LocalizationEntry>();


        Dictionary<string, string> buildValues = new Dictionary<string, string>();
        bool built = false;

        public LocalizationProfile()
        {
        }

        public string GetValue(string key)
        {

            if (built == false)
                Build();

            if(buildValues.TryGetValue(key, out var value))
            {
                return value;
            }

            Logger.Log($"localization key [{key}] not found");

            return $"localization key [{key}] not found";

        }

        public void Build()
        {
            foreach(var value in Table) 
            {
                buildValues.Add(value.Key, value.Value);
            }

            built = true;

        }


    }

}
