using RetroEngine;
using Microsoft.Xna.Framework.Audio;
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

        static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();

        static Dictionary<object, string> references = new Dictionary<object, string>();

        const string ROOT_PATH = "../../../../";

        public static Texture2D LoadTextureFromFile(string path)
        {
            if (textures.ContainsKey(path))
                return textures[path];

            string filePpath = FindPathForFile(path);

            try
            {
                using (FileStream stream = new FileStream(filePpath, FileMode.Open))
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

        public static SoundEffect LoadSoundFromFile(string path)
        {
            if (sounds.ContainsKey(path))
                return sounds[path];

            string filePpath = FindPathForFile(path);

            try
            {
                using (FileStream stream = new FileStream(filePpath, FileMode.Open))
                {

                    sounds.Add(path, SoundEffect.FromStream(stream));

                    return sounds[path];
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during texture loading
                Console.WriteLine("Failed to load texture: " + ex.Message);
                return null;
            }

        }

        public static string FindPathForFile(string path)
        {

            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            if (File.Exists(ROOT_PATH + "GameData/" + path))
                return ROOT_PATH + "GameData/" + path;

            if (File.Exists(ROOT_PATH + "GameData/brushes/" + path))
                return ROOT_PATH + "GameData/brushes/" + path;

            if (File.Exists(ROOT_PATH + "GameData/maps/" + path))
                return ROOT_PATH + "GameData/maps/" + path;


            return path;
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
