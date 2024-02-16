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
using System.Threading;

namespace RetroEngine
{
    public class AssetRegistry
    {

        static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();

        static Dictionary<string, Effect> effects = new Dictionary<string, Effect>();

        static List<string> texturesHistory = new List<string>();

        static List<string> nullTextures= new List<string>();

        const string ROOT_PATH = "../../../../";

        const int MaxTexturesInMemory = 5000;

        public static List<object> ConstantCache = new List<object>();

        public static bool AllowGeneratingMipMaps = false;

        static bool loadingAssets = false;

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

        public static void ClearAllTextures()
        {
            foreach(Texture2D tex in textures.Values)
            {
                tex?.Dispose();
            }

            textures.Clear();
            texturesHistory.Clear();

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

        public static Effect GetShaderFromName(string path)
        {
            if(effects.ContainsKey(path))
            {
                return effects[path];
            }

            effects.Add(path, GameMain.content.Load<Effect>(path));

            return effects[path];

        }

        public static List<Effect> GetAllShaders()
        {
            return effects.Values.ToList();
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

                    textures[texturesHistory[0]]?.Dispose();


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

        public static void WaitForAssetsToLoad()
        {
            while (loadingAssets)
            {
                Thread.Sleep(1);
            }
        }

        static void AsyncAssetLoaderLoop()
        {

            
            int ticksWithoutLoading = 0;
            while (true)
            {
                bool loading = false;

                try
                {
                    if (Level.ChangingLevel == false)
                        if(GameMain.Instance.curentLevel.LoadAssets())
                        {
                            loading = true;
                        }
                }catch (Exception e) {}

                if(loading)
                {
                    ticksWithoutLoading=0;
                }else
                {
                    ticksWithoutLoading++;
                }

                if (ticksWithoutLoading > 10)
                    loadingAssets = false;
                else
                    loadingAssets = true;
            }

        }

    }

    

}
