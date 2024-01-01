using RetroEngine;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace RetroEngine
{
    public class AssetRegistry
    {

        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();

        static List<string> texturesHistory = new List<string>();

        static List<string> nullTextures= new List<string>();

        const string ROOT_PATH = "../../../../";

        const int MaxTexturesInMemory = 10;

        public static List<object> ConstantCache = new List<object>();

        public static bool AllowGeneratingMipMaps = false;

        public static Texture2D LoadTextureFromFile(string path, bool ignoreErrors = false, bool generateMipMaps = true)
        {

            if (textures.ContainsKey(path))
                return textures[path];

            if(nullTextures.Contains(path))
                return null;

            if (GameMain.CanLoadAssetsOnThisThread() == false) 
            {
                Logger.Log($"THREAD ERROR:  attempted to load texture from not render thread. Texture: {path}");
                return GameMain.Instance.render.black;
            }

            string filePpath = FindPathForFile(path);

            try
            {
                using (FileStream stream = new FileStream(filePpath, FileMode.Open))
                {
                    if (generateMipMaps && AllowGeneratingMipMaps)
                    {
                        Texture2D tex = Texture2D.FromStream(GameMain.Instance.GraphicsDevice, stream);

                        textures.Add(path, GenerateMipMaps(tex));
                    }else
                    {
                        textures.Add(path, Texture2D.FromStream(GameMain.Instance.GraphicsDevice, stream));
                    }
                    texturesHistory.Add(path);

                    Console.WriteLine($"loaded texture. Current texture cache: {textures.Count}");

                    return textures[path];
                }
            }
            catch (Exception ex)
            {
                if(!ignoreErrors)
                    Console.WriteLine("Failed to load texture: " + ex.Message);
                nullTextures.Add(path);
                return null;
            }

        }

        static Texture2D GenerateMipMaps(Texture2D intermediateTexture)
        {

            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            Texture2D texture = null;
            RenderTarget2D renderTarget = new RenderTarget2D(graphicsDevice, intermediateTexture.Width, intermediateTexture.Height, mipMap: true, preferredFormat: SurfaceFormat.Color, preferredDepthFormat: DepthFormat.None);

            BlendState blendState = BlendState.Opaque;

            graphicsDevice.SetRenderTarget(renderTarget);
            using (SpriteBatch sprite = new SpriteBatch(graphicsDevice))
            {
                sprite.Begin(SpriteSortMode.Immediate, blendState,SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
                effect: null);
                sprite.Draw(intermediateTexture, new Vector2(0, 0), Color.White);
                sprite.End();
            }

            texture = (Texture2D)renderTarget;
            graphicsDevice.SetRenderTarget(null);
            intermediateTexture.Dispose();
            return texture;
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

                    SoundEffect soundEffect = SoundEffect.FromStream(stream);

                    Console.WriteLine($"sound duration: {soundEffect.Duration}");

                    sounds.Add(path, soundEffect);

                    return sounds[path];
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during texture loading
                Console.WriteLine("Failed to load sound: " + ex.Message);
                return null;
            }

        }

        public static void ClearTexturesIfNeeded()
        {
            if(texturesHistory.Count>MaxTexturesInMemory)
            {
                int numToRemove = texturesHistory.Count - MaxTexturesInMemory;

                for (int i = 0; i < numToRemove; i++)
                {
                    textures.Remove(texturesHistory[0]);
                    texturesHistory.RemoveAt(0);
                    
                }
            }
        }

        public static string FindPathForFile(string path)
        {

            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            if (File.Exists(ROOT_PATH + "GameData/" + path))
                return ROOT_PATH + "GameData/" + path;

            if (File.Exists(ROOT_PATH + "GameData/textures/" + path))
                return ROOT_PATH + "GameData/textures/" + path;

            if (File.Exists(ROOT_PATH + "GameData/textures/brushes/" + path))
                return ROOT_PATH + "GameData/textures/brushes/" + path;

            if (File.Exists(ROOT_PATH + "GameData/maps/" + path))
                return ROOT_PATH + "GameData/maps/" + path;


            return path;
        }

        public static void StartAsyncAssetLoader()
        {
            Task.Run(() => { AsyncAssetLoaderLoop(); });
        }

        static void AsyncAssetLoaderLoop()
        {
            while (true)
            {
                try
                {
                    if (Level.ChangingLevel == false)
                        GameMain.Instance.curentLevel.LoadAssets();
                }catch (Exception e) {}
            }
        }

    }

    

}
