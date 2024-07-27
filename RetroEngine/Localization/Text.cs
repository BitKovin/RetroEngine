using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Localization
{
    public struct Text
    {

        public string Key = "";

        public Dictionary<string, string> ReplaceValues = new Dictionary<string, string>();

        public string LitteralString = "";

        public Text(string key) 
        {
            Key = key;
        }

        public Text()
        {

        }

        // Implicit operator to convert a string to a Text struct
        public static implicit operator Text(string literal)
        {
            return new Text
            {
                LitteralString = literal == null ? "" : literal,
                ReplaceValues = new Dictionary<string, string>()
            };
        }

        public override string ToString()
        {

            if(LitteralString != "")
            {
                return LitteralString;
            }

            if (Key == "")
                return "";

            string value = LocalizationManager.GetStringFromKey(Key);

            foreach(var replace in ReplaceValues) 
            {

                value = value.Replace(replace.Key, replace.Value);

            }

            return value;
        }

    }
}
