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
using RetroEngine.PhysicsSystem;
using MonoGame.Extended.Text;

namespace RetroEngine
{

    public enum Platform
    {
        Desktop,
        Mobile
    }

    public class GameMain : Microsoft.Xna.Framework.Game
    {
        public DynamicSpriteFont DefaultFont;

        public string DefaultShader = "";

        public GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;
        public static ContentManager content;
        static public GameMain Instance;

        public Level curentLevel;

        public int ScreenHeight;
        public int ScreenWidth;

        public static Platform platform;

        public static UiElement UIManger = new UiViewport();

        public static GameTime time;

        public Render render;
        internal ImGuiRenderer ImGuiRenderer;

        public DevMenu devMenu;

        public bool paused = false;

        Task gameTask;

        int tick = 0;
        

        bool pendingGraphicsUpdate = false;
        bool wasFocused = true;

        public static Thread RenderThread;
        public static Thread GameThread;

        public static bool AsyncGameThread = true;

        public static bool AllowAsyncAssetLoading = false;

        public bool DevMenuEnabled = false;

        Stopwatch stopwatch = new Stopwatch();

        public static float MaxFPS = 0;

        ImFontPtr font;

        public static int SkipFrames = 0;

        public static float ReservedTaskMinTime = 0.000f;

        public static float ReservedTaskPresentMinTime = 0.000f;

        public static List<IDisposable> pendingDispose = new List<IDisposable>();

        public static bool CompatibilityMode = false;

        public static string LaunchArguments = "";

        public new GraphicsDevice GraphicsDevice;

        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
            curentLevel = new Level();
            UiElement.Viewport = UIManger;
            if (CompatibilityMode)
            {
                _graphics.GraphicsProfile = GraphicsProfile.Reach;
            }
            else
            {
                _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            }
            RenderThread = Thread.CurrentThread;

        }

        protected override void Initialize()
        {
            

            base.Initialize();

            if (GraphicsDevice == null)
                GraphicsDevice = base.GraphicsDevice;

            content = Content;

            FocusGameWindow();

            Window.ClientSizeChanged += Window_ClientSizeChanged;

            Input.AddAction("click").LMB = true;

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

            _graphics.HardwareModeSwitch = false;

            _graphics.PreferMultiSampling = false;

            //if (platform == Platform.Mobile)
            //_graphics.IsFullScreen = true;
            _graphics.ApplyChanges();


            render = new Render();

            new DrawDebug(GraphicsDevice);

#if DEBUG
            DrawDebug.Enabled = true;
#endif

            LevelObjectFactory.InitializeTypeCache();
            ParticleSystemFactory.InitializeTypeCache();

            if (AllowAsyncAssetLoading)
                AssetRegistry.StartAsyncAssetLoader();

            GameTotalTime.Start();

            SoundManager.Init();

        }

        public virtual void CheckWindowFullscreenStatus()
        {

        }

        public static bool CanLoadAssetsOnThisThread()
        {
            if (AllowAsyncAssetLoading)
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


            if (GraphicsDevice == null)
                GraphicsDevice = base.GraphicsDevice;

            
                ImGuiRenderer = new ImGuiRenderer(this);
                ImGui.GetIO().Fonts.AddFontDefault();
                font = ImGui.GetIO().Fonts.AddFontFromFileTTF("calibri.ttf", 16);
                ImGuiRenderer.RebuildFontAtlas();
                ImGui.StyleColorsDark();
                ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here

            DefaultFont = AssetRegistry.LoadFontSpriteFromFile("Fonts/dos.ttf");// Content.Load<SpriteFont>("Fonts/Font");

            curentLevel.Start();

            this.Exiting += Game1_Exiting;

        }

        protected override void EndRun()
        {
            base.EndRun();

            Environment.Exit(Environment.ExitCode);

        }

        protected override void Update(GameTime gameTime)
        {

            Render.DestroyPending();

            Stats.StopRecord("frame change");
            Stats.StopRecord("frame total");
            Stats.StartRecord("frame total");

            //checkAppRegainedFocus();

            if (tick == 2)
                GameInitialized();

            

            time = gameTime;

            //CheckWindowFullscreenStatus();


            AssetRegistry.ClearTexturesIfNeeded();

            ScreenHeight = GraphicsDevice.PresentationParameters.Bounds.Height;

            ScreenWidth = GraphicsDevice.PresentationParameters.Bounds.Width;

            var physicsTask = Task.Factory.StartNew((Action)(() => { Physics.Simulate(); }));

            curentLevel.WaitForVisualUpdate();
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

            

            lock (pendingDispose)
            {

                var list = pendingDispose.ToArray();

                foreach (IDisposable disposable in list)
                {
                    disposable?.Dispose();
                }

                pendingDispose.Clear();

            }
            

            Camera.ViewportUpdate();

            Input.Update();

            UiElement.Viewport.Update();

            PerformReservedTimeTasks();

            LimitFrameRate();





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
            float newDeltaTime = (float)Math.Min(gameTime.ElapsedGameTime.TotalSeconds, 0.05d);

            //Time.DeltaTime = newDeltaTime;

            if (SkipFrames > 0)
                newDeltaTime = 0.001f;

            Time.AddFrameTime(newDeltaTime);

            if (!paused)
                Time.gameTime += Time.DeltaTime;
        }
        void GameLogic()
        {

            GameThread = Thread.CurrentThread;

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

            if (Graphics.LowLatency)
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

        public virtual bool IsGameWindowFocused()
        {
            return IsActive;
        }

        public virtual void FocusGameWindow()
        {

        }

        public virtual void FlashWindow()
        {

        }

        void PerformReservedTimeTasks()
        {
            Stopwatch sw = Stopwatch.StartNew();
            ReservedTimeTasks();
            if (ReservedTaskMinTime > 0)
                while (sw.Elapsed.TotalSeconds < ReservedTaskMinTime)
                {
                }
        }

        void ReservedTimeTasks()
        {
            if (AllowAsyncAssetLoading == false)
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

            if (pendingGraphicsUpdate)
                _graphics.ApplyChanges();

            DrawSplashIfNeed();

            if (SkipFrames > 0)
            {
                SkipFrames--;
                LoadingScreen.Draw();
                return;
            }

            

            //Level.GetCurrent().EndOcclusionCheck();


            Stats.StartRecord("frame change");

            if (GameMain.SkipFrames > 0) return;

            if (IsGameWindowFocused() || _isFullscreen == false)
            {
                //CheckWindowFullscreenStatus();
                if (AllowAsyncAssetLoading && Render.AsyncPresent)
                {

                    presentingFrame = true;
                    presentFrameTask = Task.Factory.StartNew(() => { PresentFrame(); });
                }
                else
                {
                    PresentFrame();
                }
            }

        }



        Task presentFrameTask;

        bool presentingFrame = false;

        void PresentFrame()
        {
            presentingFrame = true;
            GraphicsDevice.SetRenderTarget(null);
            Stats.StartRecord("Frame Present");

            GraphicsDevice.Present();

            Stats.StopRecord("Frame Present");
            presentingFrame = false;
            Stats.StopRecord("Render");
            Level.LoadedAssetsThisFrame = 0;
        }

        public void WaitForFramePresent()
        {
            if (presentingFrame == false) return;

            Stats.StartRecord("WaitForFramePresent");
            while (presentingFrame)
            {
                if (presentFrameTask == null) return;
                if (presentFrameTask.IsCompletedSuccessfully) return;
                if (presentFrameTask.IsCanceled) return;
                if (presentFrameTask.IsFaulted) return;
            }
            Stats.StopRecord("WaitForFramePresent");

        }

        protected override void EndDraw()
        {

            
        }


        Stopwatch GameTotalTime = new Stopwatch();

        internal virtual void DrawSplashIfNeed()
        {
#if RELEASE
            if (GameTotalTime.Elapsed.TotalSeconds < 3)
                DrawSplash();
#endif
        }

        

        protected virtual void DrawSplash()
        {
            if (GameMain.CanLoadAssetsOnThisThread() == false) return;

            GameMain.Instance.GraphicsDevice.Clear(Color.Black);

            SpriteBatch SpriteBatch = GameMain.Instance.SpriteBatch;

            SpriteBatch.Begin();

            int offsetX = GraphicsDevice.Viewport.Width - GraphicsDevice.Viewport.Height;

            Rectangle screenRectangle = new Rectangle(offsetX/2, 0, GameMain.Instance.GraphicsDevice.Viewport.Height, GameMain.Instance.GraphicsDevice.Viewport.Height);

            Texture2D background = AssetRegistry.LoadTextureFromFile("engine/textures/splash.png", false, false);

            Point SizeSubtract = new Point((int)MathHelper.Lerp(-10,30, (float)GameTotalTime.Elapsed.TotalSeconds));

            screenRectangle.Location += new Point(SizeSubtract.X / 2, SizeSubtract.Y / 2);
            screenRectangle.Size -= SizeSubtract;

            // Draw the render target to the screen
            SpriteBatch.Draw(background, screenRectangle, Color.White);

            SpriteBatch.End();
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
        protected bool _isFullscreen = false;
        bool _isBorderless = false;
        int _width = 0;
        int _height = 0;
        public void ToggleFullscreen()
        {
            bool oldIsFullscreen = _isFullscreen;

            if (_isBorderless)
            {
                _isBorderless = false;
            }
            else
            {
                _isFullscreen = !_isFullscreen;
            }

            ApplyFullscreenChange(oldIsFullscreen);
        }
        public void ToggleBorderless()
        {
            bool oldIsFullscreen = _isFullscreen;

            _isBorderless = !_isBorderless;
            _isFullscreen = _isBorderless;

            ApplyFullscreenChange(oldIsFullscreen);
        }

        private void ApplyFullscreenChange(bool oldIsFullscreen)
        {
            if (_isFullscreen)
            {
                if (oldIsFullscreen)
                {
                    ApplyHardwareMode();
                }
                else
                {
                    SetFullscreen();
                }
            }
            else
            {
                UnsetFullscreen();
            }
        }
        private void ApplyHardwareMode()
        {
            _graphics.IsFullScreen = false;
            _graphics.HardwareModeSwitch = !_isBorderless;
            _graphics.ApplyChanges();
        }
        protected virtual void SetFullscreen()
        {

            FocusGameWindow();

            _width = Graphics.Resolution.X;
           _height = Graphics.Resolution.Y;

            _graphics.PreferredBackBufferWidth = _width;
            _graphics.PreferredBackBufferHeight = _height;
            _graphics.ApplyChanges();



            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }
        protected virtual void UnsetFullscreen()
        {

            _width = Graphics.Resolution.X;
            _height = Graphics.Resolution.Y;

            _graphics.PreferredBackBufferWidth = _width;
            _graphics.PreferredBackBufferHeight = _height;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
        }


        public static void AddLaunchArguments(string[] args)
        {
            foreach (string arg in args)
            {
                LaunchArguments += " " + arg;
                Console.WriteLine(arg);
            }
        }

        public virtual void GameInitialized()
        {

            Graphics.Resolution = new Point(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);

            Logger.Log("Launch Arguments: " + LaunchArguments);

            if (LaunchArguments.StartsWith("-"))
                LaunchArguments = LaunchArguments.Replace("-", " -");

            var commands = LaunchArguments.Split(" -");

            foreach (string command in commands)
            {
                ConsoleCommands.ProcessCommand(command);
                Console.WriteLine(command);
            }

            

        }

        private void LimitFrameRate()
        {
            if (MaxFPS<1) return;

            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

            while (elapsedSeconds < 1f / MaxFPS)
            {
                elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            }

            stopwatch.Restart();
        }


        public virtual void OnLevelChanged()
        {
            
            render.shadowPassRenderDelay = new Delay();

            SaveSystem.SaveManager.LoadSaveIfPending();

        }



        public void DoGameInitialized()
        {
            Initialize();
        }

        public void DoLoadContent()
        {
            LoadContent();
        }

        public void DoUnloadContent()
        {
            UnloadContent();
        }

        public void DoUpdate(GameTime gameTime)
        {
            Update(gameTime);
        }

        public void DoDraw(GameTime gameTime)
        {
            Draw(gameTime);
        }

        [ConsoleCommand("g.maxfps")]
        public static void SetMaxFPS(int value)
        {
            MaxFPS = value;
        }

        [ConsoleCommand("g.defaultshader")]
        public static void SetDefaultShader(string name)
        {
            Instance.DefaultShader = name;
        }

    }
}
