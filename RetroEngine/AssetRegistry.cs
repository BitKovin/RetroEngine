using Engine;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class AssetRegistry
    {

        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        static Dictionary<object, string> references = new Dictionary<object, string>();

        public static Texture2D LoadTextureFromFile(string path)
        {

            if(textures.ContainsKey(path))
                return textures[path];

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {

                    textures.Add(path, Texture2D.FromStream(GameMain.inst.GraphicsDevice, stream));

                    return textures[path];
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during texture loading
                Console.WriteLine("Failed to load texture: " + ex.Message);
                return null;
            }

        }

        public static void AddReference(object reference, string key)
        {
            references.Add(reference, key);
        }

        public static void RemoveReference(object reference)
        {
            references.Remove(reference);
        }

    }
}
