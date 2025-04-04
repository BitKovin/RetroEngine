﻿using RetroEngine;
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
using RetroEngine.Graphic;
using RetroEngine.Audio;
using BulletSharp;
using FmodForFoxes;
using System.Runtime.InteropServices;
using FmodForFoxes.Studio;
using MonoGame.Extended.Text;
using MonoGame.Extended.Framework.Media;
using MonoGame.Extended.VideoPlayback;

namespace RetroEngine
{
    public class AssetRegistry
    {

        static Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        static Dictionary<string, SoundEffect> sounds = new Dictionary<string, SoundEffect>();
        static Dictionary<string, Sound> soundsFmod = new Dictionary<string, Sound>();
        static Dictionary<string, Bank> fmodBanks = new Dictionary<string, Bank>();


        static Dictionary<string, Shader> effects = new Dictionary<string, Shader>();
        static Dictionary<string, Shader> effectsPP = new Dictionary<string, Shader>();

        static List<string> texturesHistory = new List<string>();

        static List<string> nullAssets = new List<string>();

        public static string ROOT_PATH = "../../../../";

        const int MaxTexturesInMemory = 5000;

        public static List<object> ConstantCache = new List<object>();

        public static bool AllowGeneratingMipMaps = false;

        static bool loadingAssets = false;

        public static Texture2D LoadTextureFromFile(string path, bool ignoreErrors = false, bool generateMipMaps = true)
        {

            //if (path.Contains("engine") == false && ignoreErrors == false && path.Contains("_t.png") == false && path.Contains("_transperent.png") == false)
            //    path = "engine/textures/white.png";

            if (nullAssets.Contains(path))
                return null;

            if (textures.ContainsKey(path))
                return (Texture2D)textures[path];

            //if (Thread.CurrentThread == LoaderThread)
                //Thread.Sleep(1);

            if (GameMain.CanLoadAssetsOnThisThread() == false)
            {
                Logger.Log($"THREAD ERROR:  attempted to load texture from not render thread. Texture: {path}");
                return GameMain.Instance.render.black;
            }

            string filePath = FindPathForFile(path);

            if (File.Exists(filePath) == false)
            {
                nullAssets.Add(path);
                return null;
            }

            try
            {

                using (var asset = AssetRegistry.GetFileStreamFromPath(filePath))
                {

                    Stream stream = asset.FileStream;

                    if(stream == null)
                    {
                        lock (nullAssets)
                            nullAssets.Add(filePath);
                        return null;
                    }

                    if (generateMipMaps && AllowGeneratingMipMaps)
                    {
                        Texture2D tex = Texture2D.FromStream(GameMain.Instance.GraphicsDevice, stream);

                        lock (textures)
                            textures.Add(path, GenerateMipMaps(tex));

                    }
                    else
                    {
                        lock (textures)
                            textures.Add(path, Texture2D.FromStream(GameMain.Instance.GraphicsDevice, stream));
                    }

                    textures[path].Name = path;

                    lock (texturesHistory)
                        texturesHistory.Add(path);

                    Logger.Log($"loaded texture {path}. Current texture cache: {textures.Count}");

                    lock (textures)
                        return (Texture2D)textures[path];
                }

            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                    Logger.Log("Failed to load texture: " + ex.Message);

                lock (nullAssets)
                    nullAssets.Add(path);
                return null;
            }

        }

        static FontManager FontManager = new FontManager();
        static Dictionary<string, DynamicSpriteFont> loadedFonts = new Dictionary<string, DynamicSpriteFont>();
        public static DynamicSpriteFont LoadFontSpriteFromFile(string path)
        {
            lock (loadedFonts)
            {
                if (loadedFonts.ContainsKey(path))
                    return loadedFonts[path];

                path = FindPathForFile(path);

                if (loadedFonts.ContainsKey(path))
                    return loadedFonts[path];

                var stream = GetFileStreamFromPath(path);

                var bytes = default(byte[]);
                using (var memstream = new MemoryStream())
                {
                    stream.StreamReader.BaseStream.CopyTo(memstream);
                    bytes = memstream.ToArray();
                }

                var font = FontManager.LoadFont(path, bytes, 72, 0);

                Font fallback = null;
                if (GameMain.Instance.DefaultFallbackFont != null)
                    fallback = GameMain.Instance.DefaultFallbackFont.Font;

                DynamicSpriteFont dynamicSpriteFont = new DynamicSpriteFont(GameMain.Instance.GraphicsDevice, font, fallback);


                loadedFonts.Add(path, dynamicSpriteFont);

                return dynamicSpriteFont;
            }

        }

        static Dictionary<string, Video> loadedVideos = new Dictionary<string, Video>();

        public static Video LoadVideoFromFile(string path)
        {
            lock (loadedVideos)
            {
                if (loadedVideos.ContainsKey(path))
                    return loadedVideos[path];

                Logger.Log("reading file:" + path);
                path = FindPathForFile(path);

                if (loadedVideos.ContainsKey(path))
                    return loadedVideos[path];

                var video = VideoHelper.LoadFromFile(path);

                //loadedVideos.Add(path, video);

                return video;
            }
        }

        public static void UnloadVideos()
        {
            lock (loadedVideos)
            {
                foreach (var v in loadedVideos.Values)
                {
                    v?.Dispose();
                }

                loadedVideos.Clear();
            }
        }

        static bool SaveAssetLoadHistory = false;
        static List<string> AssetLoadHistory = new List<string>();

        public static AssetFileStreamReference GetFileStreamFromPath(string path)
        {
            Logger.Log("reading file:" + path);

            if (File.Exists(path) == false)
                return null;

            AssetLoadHistory.Add(path);

            return new AssetFileStreamReference(path, File.OpenRead(path));
        }

        public static TextureCube LoadCubeTextureFromFile(string path, bool ignoreErrors = false, bool generateMipMaps = true)
        {

            if (textures.ContainsKey(path))
                return (TextureCube)textures[path];

            if (nullAssets.Contains(path))
                return null;

            if (GameMain.CanLoadAssetsOnThisThread() == false)
            {
                Logger.Log($"THREAD ERROR:  attempted to load texture from not render thread. Texture: {path}");
                return null;
            }

            string filePpath = FindPathForFile(path);

            try
            {

                Texture2D top = LoadTextureFromFile(filePpath.Replace(".", "_top."));
                Texture2D bottom = LoadTextureFromFile(filePpath.Replace(".", "_bottom."));
                Texture2D left = LoadTextureFromFile(filePpath.Replace(".", "_left."));
                Texture2D right = LoadTextureFromFile(filePpath.Replace(".", "_right."));
                Texture2D forward = LoadTextureFromFile(filePpath.Replace(".", "_front."));
                Texture2D backward = LoadTextureFromFile(filePpath.Replace(".", "_back."));

                RenderTargetCube cube = new RenderTargetCube(GameMain.Instance.GraphicsDevice, top.Height, false, SurfaceFormat.Color, DepthFormat.None);

                GenerateCubeFace(cube, top, CubeMapFace.PositiveY);
                GenerateCubeFace(cube, bottom, CubeMapFace.NegativeY);

                GenerateCubeFace(cube, left, CubeMapFace.NegativeX);
                GenerateCubeFace(cube, right, CubeMapFace.PositiveX);

                GenerateCubeFace(cube, forward, CubeMapFace.PositiveZ);
                GenerateCubeFace(cube, backward, CubeMapFace.NegativeZ);

                textures.Add(path, cube);
                return (TextureCube)textures[path];
            }
            catch (Exception ex)
            {
                if (!ignoreErrors)
                    Logger.Log("Failed to load texture: " + ex.Message);
                nullAssets.Add(path);
                return null;
            }

        }

        public static void UnloadTexture(Texture texture)
        {
            if (textures.ContainsValue(texture))
            {

                foreach (string key in textures.Keys)
                {
                    if (textures[key] == texture)
                    {
                        textures.Remove(key);
                        break;
                    }
                }

                texture.Dispose();
            }
        }


        public static void ClearAllTextures()
        {

            var texturesCopy = new Dictionary<string, Texture>(textures);

            foreach(string key in texturesCopy.Keys)
            {
                if (ShouldUnloadAsset(key) == false) continue;

                textures[key]?.Dispose();

                textures.Remove(key);
                texturesHistory.Remove(key);
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
                sprite.Begin(SpriteSortMode.Immediate, blendState, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
                effect: null);
                sprite.Draw(intermediateTexture, new Vector2(0, 0), Color.White);
                sprite.End();
            }

            texture = (Texture2D)renderTarget;
            graphicsDevice.SetRenderTarget(null);
            intermediateTexture.Dispose();
            return texture;
        }

        static void GenerateCubeFace(RenderTargetCube cube, Texture2D intermediateTexture, CubeMapFace face)
        {

            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;


            BlendState blendState = BlendState.Opaque;

            graphicsDevice.SetRenderTarget(cube, face);
            using (SpriteBatch sprite = new SpriteBatch(graphicsDevice))
            {
                sprite.Begin(SpriteSortMode.Immediate, blendState, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,
                effect: null);
                sprite.Draw(intermediateTexture, new Vector2(0, 0), Color.White);
                sprite.End();
            }

            UnloadTexture(intermediateTexture);


            graphicsDevice.SetRenderTarget(null);
        }

        static List<string> nullShaders = new List<string>();

        public static Effect GetShaderFromName(string path)
        {

            path = "Shaders/" + path;

            if (nullShaders.Contains(path))
                return null;

            if (effects.ContainsKey(path))
            {
                return effects[path];
            }
            Effect effect = null;
            try
            {
                effect = GameMain.content.Load<Effect>(path);
            }

            catch (Exception e) { }

            if (effect == null)
            {
                nullShaders.Add(path);
                return null;
            }

            if (effect.GraphicsDevice == null)
            {
                Logger.Log("Error: shader misses graphics devise   " + path);
                return null;
            }

            lock(effects)
                effects.Add(path, new Shader(effect));
            

            return effects[path];

        }

        public static Shader GetPostProcessShaderFromName(string path)
        {
            if (effectsPP.ContainsKey(path))
            {
                return effectsPP[path];
            }

            var effect = GameMain.content.Load<Effect>("Shaders/" + path);

            lock(effectsPP)
                effectsPP.Add(path, new Shader(effect));

            effect.Dispose();

            return effectsPP[path];

        }

        internal static List<Shader> GetAllShaders()
        {
            return effects.Values.ToList();
        }


        public static AudioClip LoadSoundFromFile(string path, bool legacyOnly = false)
        {

            if (SoundManager.UseFmod && legacyOnly == false)
                return LoadSoundFmodFromFile(path);

            if (sounds.ContainsKey(path))
                return new Audio.AudioClipLegacy(sounds[path]);

            string filePath = FindPathForFile(path);

            try
            {
                using (var asset = GetFileStreamFromPath(filePath))
                {

                    SoundEffect soundEffect = SoundEffect.FromStream(asset.FileStream);

                    sounds.Add(path, soundEffect);

                    return new Audio.AudioClipLegacy(sounds[path]);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during texture loading
                Logger.Log("Failed to load sound: " + ex.Message);
                return null;
            }

        }

        public static Sound LoadSoundFmodNativeFromFile(string path)
        {

            if (SoundManager.UseFmod == false)
            {
                Logger.Log("tried to load FMOD sound " + path);
                return null;
            }



            Sound sound;

            if (soundsFmod.ContainsKey(path))
            {
                sound = soundsFmod[path];
            }
            else
            {
                string filePath = FindPathForFile(path);

                using (var asset = AssetRegistry.GetFileStreamFromPath(filePath))
                {
                    sound = CoreSystem.LoadSoundFromStream(asset.FileStream);
                }
                soundsFmod.TryAdd(path, sound);
            }
            sound.Volume = 0;


            return sound;
        }


        public static AudioClipFmod LoadSoundFmodFromFile(string path)
        {

            if (SoundManager.UseFmod == false)
            {
                Logger.Log("tried to load FMOD sound " + path);
                return null;
            }

            return new AudioClipFmod(LoadSoundFmodNativeFromFile(path));


        }

        public static Bank LoadFmodBankIntoMemory(string path)
        {
            if (fmodBanks.ContainsKey(path))
                return fmodBanks[path];

            string filePath = FindPathForFile(path);
            Bank bank;
            using (var asset = AssetRegistry.GetFileStreamFromPath(filePath))
            {
                bank = StudioSystem.LoadBankFromStream(asset.FileStream, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL);
            }

            lock(fmodBanks)
            fmodBanks.Add(path, bank);

            return bank;

        }

        public static void UnloadBanks()
        {
            lock (fmodBanks)
            {

                List<string> unloaded = new List<string>();

                foreach (string name in fmodBanks.Keys)
                {
                    if (name.ToLower().Contains("master")) continue;

                    if (ShouldUnloadAsset(name) == false) continue;

                    fmodBanks[name].Unload();
                    unloaded.Add(name);
                }

                foreach (string name in unloaded)
                    fmodBanks.Remove(name);

            }
        }

        public static void ClearTexturesIfNeeded()
        {
            if (texturesHistory.Count > MaxTexturesInMemory)
            {
                int numToRemove = texturesHistory.Count - MaxTexturesInMemory;

                for (int i = 0; i < numToRemove; i++)
                {

                    textures[texturesHistory[0]]?.Dispose();


                    texturesHistory.RemoveAt(0);

                }
            }
        }

        public const string AssetsRoot = "GameData/";

        public static string FindPathForFile(string path)
        {

            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            if (File.Exists(ROOT_PATH + AssetsRoot + path))
                return ROOT_PATH + AssetsRoot + path;

            if (File.Exists(ROOT_PATH + AssetsRoot + "textures/" + path))
                return ROOT_PATH + AssetsRoot + "textures/" + path;

            if (File.Exists(ROOT_PATH + AssetsRoot + "textures/brushes/" + path))
                return ROOT_PATH + AssetsRoot + "textures/brushes/" + path;

            if (File.Exists(ROOT_PATH + AssetsRoot + "maps/" + path))
                return ROOT_PATH + AssetsRoot + "maps/" + path;


            return path;
        }

        static Thread LoaderThread;


        public static void StartAsyncAssetLoader()
        {
            LoaderThread = new Thread(AsyncAssetLoaderLoop);
            LoaderThread.Name = "Asset Loader";
            LoaderThread.IsBackground = true;
            LoaderThread.Priority = ThreadPriority.BelowNormal;
            LoaderThread.Start();
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


                if (Level.ChangingLevel == false)
                    if (GameMain.Instance.curentLevel.LoadAssets())
                    {
                        loading = true;
                    }



                if (loading)
                {
                    ticksWithoutLoading = 0;
                }
                else
                {
                    ticksWithoutLoading++;
                }

                if (ticksWithoutLoading > 10)
                    loadingAssets = false;
                else
                    loadingAssets = true;
            }

        }

        static string[] levelReferencedAssets = new string[0];

        public static bool ShouldUnloadAsset(string assetPath)
        {

            if (SaveAssetLoadHistory)
                return true;

            foreach (string path in levelReferencedAssets) {
                if (path.Contains(assetPath) || assetPath.Contains(path))
                {

                    Console.WriteLine("not unloading " + assetPath);

                    return false;
                }
            }

            return true;
        }

        [ConsoleCommand("AssetRegistry.StartRecord")]
        public static void StartRecordAssetReferences()
        {
            SaveAssetLoadHistory = true;
            AssetLoadHistory.Clear();
        }

        [ConsoleCommand("AssetRegistry.StopRecord")]
        public static void StopRecordAssetReferences()
        {
            SaveAssetLoadHistory = false;

            string referenceFilePath = ROOT_PATH + "GameData/" + "maps/" + Level.LoadingLevel + ".references";

            File.Create(referenceFilePath).Close();

            TextWriter writer = new StreamWriter(referenceFilePath);

            foreach(string assetPath in AssetLoadHistory)
            {
                writer.WriteLine(assetPath);
            }

            writer.Close();

        }

        public static bool LoadLevelReferences()
        {

            if (SaveAssetLoadHistory)
                return false;

            string referenceFilePath = ROOT_PATH + "GameData/" + "maps/" + Level.LoadingLevel + ".references";

            if (File.Exists(referenceFilePath) == false)
                return false;

            TextReader reader = new StreamReader(referenceFilePath);

            List<string> references = new List<string>();

            
            while (true)
            {
                string s = reader.ReadLine();

                if (s == null)
                    break;

                references.Add(s);

            }

            levelReferencedAssets = references.ToArray();

            return true;


        }

        public class AssetFileStreamReference : IDisposable
        {
            public string path;

            public Stream FileStream;

            public StreamReader StreamReader;

            public bool disposed = false;

            public void Dispose()
            {
                FileStream?.Close();
                FileStream?.Dispose();

                disposed = true;

            }

            public AssetFileStreamReference(string path, Stream fileStream)
            {
                this.path = path;
                this.FileStream = fileStream;
                this.StreamReader = new StreamReader(fileStream);
            }

            ~AssetFileStreamReference()
            {
                FileStream?.Close();
                FileStream?.Dispose();
                StreamReader.Close();
                disposed = true;
            }

        }

    }



}
