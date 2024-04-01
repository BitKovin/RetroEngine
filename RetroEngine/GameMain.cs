#define isDesktop

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using RetroEngine.UI;
using RetroEngine;
using MonoGame.ImGuiNet;
using RetroEngine.Audio;
using ImGuiNET;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace RetroEngine
{

    public enum Platform
    {
        Desktop,
        Mobile
    }

    public class GameMain : Microsoft.Xna.Framework.Game
    {
        public SpriteFont DefaultFont;

        public Effect DefaultShader = null;

        public GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;
        public static ContentManager content;
        static public GameMain Instance;

        public Level curentLevel;

        public int ScreenHeight;
        public int ScreenWidth;

        public static Platform platform;

        public static UiElement UIManger = new UiElement();

        public static GameTime time;

        public Render render;
        ImGuiRenderer ImGuiRenderer;

        public DevMenu devMenu;

        public bool paused = false;

        Task gameTask;

        public bool Fullscreen = false;

        int tick = 0;

        bool pendingGraphicsUpdate = false;
        bool wasFocused = true;

        public static Thread RenderThread;

        public static bool AsyncGameThread = true;

        public static bool AllowAsyncAssetLoading = false;

        public bool DevMenuEnabled = true;

        Stopwatch stopwatch = new Stopwatch();
        public bool LimitFPS = false;

        ImFontPtr font;

        internal static int SkipFrames = 0;

        public static float ReservedTaskMinTime = 0.000f;

        public static float ReservedTaskPresentMinTime = 0.001f;

        internal static List<IDisposable> pendingDispose = new List<IDisposable>();

        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
            curentLevel = new Level();
            UiElement.Viewport = UIManger;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            RenderThread = Thread.CurrentThread;

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Window.ClientSizeChanged += Window_ClientSizeChanged;

            stopwatch.Start();

            this.Window.AllowUserResizing = true;
            if (platform == Platform.Desktop)
            {
                _graphics.PreferredBackBufferWidth = 1280;  // set this value to the desired width of your window
                _graphics.PreferredBackBufferHeight = 720;   // set this value to the desired height of your window
            }
            this.IsFixedTimeStep = false;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 200d);

            _graphics.SynchronizeWithVerticalRetrace = false;

            //if (platform == Platform.Mobile)
            //_graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            
            
            render = new Render();

            LevelObjectFactory.InitializeTypeCache();
            ParticleSystemFactory.InitializeTypeCache();

            if(AllowAsyncAssetLoading)
                AssetRegistry.StartAsyncAssetLoader();
        }

        public static bool CanLoadAssetsOnThisThread()
        {
            if(AllowAsyncAssetLoading)
                return true;

            return RenderThread == Thread.CurrentThread;
        }

        protected override void UnloadContent()
        {
            content.Unload();

            Console.WriteLine("unload");

        }

        protected override void LoadContent()
        {

            if (DevMenuEnabled)
            {
                ImGuiRenderer = new ImGuiRenderer(this);
                ImGui.GetIO().Fonts.AddFontDefault();
                font = ImGui.GetIO().Fonts.AddFontFromFileTTF("calibri.ttf", 16);
                ImGuiRenderer.RebuildFontAtlas();
                ImGui.StyleColorsDark();
                ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            }
            SoundManager.Init();

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            content = Content;
            // TODO: use this.Content to load your game content here

            DefaultFont = Content.Load<SpriteFont>("Font"); // Use the name of your sprite font file here instead of 'Score'.

            curentLevel.Start();

            this.Exiting += Game1_Exiting;

        }

        protected override void Update(GameTime gameTime)
        {

            Render.DestroyPending();

            Stats.StopRecord("frame change");
            Stats.StopRecord("frame total");
            Stats.StartRecord("frame total");

            checkAppRegainedFocus();

            if (tick == 2)
                GameInitialized();

            LimitFrameRate();

            time = gameTime;


            AssetRegistry.ClearTexturesIfNeeded();

            ScreenHeight = GraphicsDevice.PresentationParameters.Bounds.Height;

            ScreenWidth = GraphicsDevice.PresentationParameters.Bounds.Width;


            if (AsyncGameThread)
            {
                if (gameTask is null)
                {
                    UpdateTime(gameTime);
                }
                else
                {
                    Stats.StartRecord("waiting game task");
                    gameTask.Wait();
                    Stats.StopRecord("waiting game task");
                    UpdateTime(gameTime);
                }
            }
            else
            {
                UpdateTime(gameTime);
            }

            foreach(IDisposable disposable in pendingDispose)
            {
                disposable.Dispose();
            }
            pendingDispose.Clear();

            var physicsTask = Task.Factory.StartNew(() => { Physics.Simulate(); });
            curentLevel.WaitForVisualUpdate();

            PerformReservedTimeTasks();
            

            foreach (UiElement elem in UiElement.Viewport.childs)
                elem.Update();

            Camera.ViewportUpdate();

            Input.Update();

            Stats.StartRecord("waiting for physics");
            physicsTask.Wait();
            Stats.StopRecord("waiting for physics");

            bool changedLevel = Level.LoadPendingLevel();
            if (AsyncGameThread && changedLevel == false)
            {
                Stats.StartRecord("starting game task");
                gameTask = Task.Factory.StartNew(() => { GameLogic(); });
                Stats.StopRecord("starting game task");
            }
            else
            {
                GameLogic();
            }
            curentLevel.StartVisualUpdate();
            base.Update(gameTime);
        }

        void UpdateTime(GameTime gameTime)
        {
            float newDeltaTime = (float)Math.Min(gameTime.ElapsedGameTime.TotalSeconds, 0.04d);

            Time.deltaTime = newDeltaTime;

            Time.AddFrameTime(newDeltaTime);

            if (!paused)
                Time.gameTime += Time.deltaTime;
        }
        void GameLogic()
        {



            Stats.StartRecord("GameLogic");

            Stats.StartRecord("Physics Update");
            Physics.Update();
            Stats.StopRecord("Physics Update");

            Navigation.Update();

            Stats.StartRecord("Level Update");
            curentLevel.Update();
            Stats.StopRecord("Level Update");

            Stats.StartRecord("Level AsyncUpdate");
            curentLevel.AsyncUpdate();
            Stats.StopRecord("Level AsyncUpdate");

            if(Graphics.LowLatency)
            WaitForFramePresent();

            Input.UpdateMouse();

            Stats.StartRecord("Level LateUpdate");
            curentLevel.LateUpdate();
            Stats.StopRecord("Level LateUpdate");

            Camera.Update();

            Stats.StartRecord("Sound Update");
            SoundManager.Update();
            Stats.StopRecord("Sound Update");

            tick++;

            Stats.StopRecord("GameLogic");

        }

        void PerformReservedTimeTasks()
        {
            Stopwatch sw = Stopwatch.StartNew();
            ReservedTimeTasks();
            if(ReservedTaskMinTime > 0)
            while(sw.Elapsed.TotalSeconds<ReservedTaskMinTime)
            {
            }
        }

        void ReservedTimeTasks()
        {
            if(AllowAsyncAssetLoading == false)
                curentLevel?.LoadAssets();

            Stats.StartRecord("render preparation");
            curentLevel?.RenderPreparation();
            Stats.StopRecord("render preparation");
        }

        private void Game1_Exiting(object sender, System.EventArgs e)
        {
            Environment.Exit(0);
        }

        public void SetupFullViewport()
        {
            var vp = new Viewport();
            vp.X = vp.Y = 0;
            vp.Width = ScreenWidth;
            vp.Height = ScreenHeight;
            GraphicsDevice.Viewport = vp;
        }

        protected override void Draw(GameTime gameTime)
        {

            //WaitForFramePresent();


            Stats.StartRecord("Render");

            

            RenderTarget2D frame = render.StartRenderLevel(curentLevel);

            

            GraphicsDevice.SetRenderTarget(null);

            

            Rectangle screenRectangle = new Rectangle(0, 0, (int)(render.GetScreenResolution().X), (int)(render.GetScreenResolution().Y));

            float dif = _graphics.PreferredBackBufferHeight / (float)(frame.Height);

            SpriteBatch.Begin(transformMatrix: Matrix.CreateScale(dif), samplerState: SamplerState.PointWrap);

            // Draw the render target to the screen
            SpriteBatch.Draw(frame, Vector2.Zero, screenRectangle, Color.White);

            SpriteBatch.End();

            SpriteBatch.Begin(transformMatrix: Camera.UiMatrix, blendState: BlendState.AlphaBlend);

            UIManger.Draw(gameTime, SpriteBatch);


            SpriteBatch.End();


            if (DevMenuEnabled)
            {
                ImGuiRenderer.BeginLayout(gameTime);

                ImGui.PushFont(font);

                if (devMenu is not null)
                    devMenu.Update();

                ImGuiRenderer.EndLayout();
            }
            //base.Draw(gameTime);

            //SetupFullViewport();


            if (_graphics.IsFullScreen != Fullscreen)
            {
                _graphics.IsFullScreen = Fullscreen;
                pendingGraphicsUpdate = true;
            }

            if (pendingGraphicsUpdate)
                _graphics.ApplyChanges();

            if(SkipFrames>0)
            {
                SkipFrames--;
                LoadingScreen.Draw();
                return;
            }

            

            if (AllowAsyncAssetLoading)
            {
                presentingFrame = true;
                presentFrameTask = Task.Factory.StartNew(() => { PresentFrame(); });
            }else
            {
                PresentFrame();
            }


                //Level.GetCurrent().EndOcclusionCheck();


            Stats.StartRecord("frame change");

            

        }

        Task presentFrameTask;

        bool presentingFrame = false;

        void PresentFrame()
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Present();
            presentingFrame = false;
            Stats.StopRecord("Render");
            Level.LoadedAssetsThisFrame = 0;
        }

        public void WaitForFramePresent()
        {
            if (presentingFrame == false) return;

            while (presentingFrame) 
            {
                if (presentFrameTask == null) return;
                if (presentFrameTask.IsCompletedSuccessfully) return;
                if (presentFrameTask.IsCanceled) return;
                if (presentFrameTask.IsFaulted) return;
            }

        }

        protected override void EndDraw()
        {
            
        }

        public object GetView(System.Type type)
        {
            return this.Services.GetService(type);
        }


        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            // Handle window resizing here
            // You can update your graphics settings, camera, etc. based on the new window size
            // For example, you can set the new resolution and adjust the viewport

            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;

            pendingGraphicsUpdate = true;
        }

        void checkAppRegainedFocus()
        {
            if (!this.IsActive)
            {
                wasFocused = false;
            }
            else if (!wasFocused && _graphics.IsFullScreen)
            {
                wasFocused = true;
                _graphics.ToggleFullScreen();
                _graphics.ToggleFullScreen();
            }
        }

        public void MakeFullscreen()
        {
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            //_graphics.ApplyChanges();

            pendingGraphicsUpdate = true;

            Fullscreen = true;
        }

        public virtual void GameInitialized()
        {

        }

        private void LimitFrameRate()
        {
            if (LimitFPS == false) return;

            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

            while(elapsedSeconds < 1f/20f)
            {
                elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            }

            stopwatch.Restart();
        }


        public virtual void OnLevelChanged()
        {
            Time.gameTime = 0;
            render.shadowPassRenderDelay = new Delay();
        }

    }
}
