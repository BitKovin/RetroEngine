using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Localization
{
    public static class LocalizationManager
    {

        static LocalizationProfile localizationProfile = new LocalizationProfile();

        public static string GetStringFromKey(string key)
        {
            return localizationProfile.GetValue(key);
        }

    }

    struct LocalizationProfile
    {
        public string Name;
        public string Key;

        Dictionary<string, string> buildValues = new Dictionary<string, string>();

        List<(string key, string value)> table = new List<(string key, string value)>();

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
            foreach(var value in table) 
            {
                buildValues.Add(value.key, value.value);
            }

            built = true;

        }


    }

}
