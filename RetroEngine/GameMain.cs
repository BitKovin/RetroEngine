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
using System.Collections.Generic;

namespace RetroEngine
{

    public enum Platform
    {
        Desktop,
        Mobile
    }

    public class GameMain : Game
    {
        public SpriteFont DefaultFont;


        public GraphicsDeviceManager _graphics;
        public SpriteBatch SpriteBatch;
        public static ContentManager content;
        static public GameMain inst;

        public Level curentLevel;

        public int ScreenHeight;
        public int ScreenWidth;

        public static Platform platform;

        public UiElement UiManger = new UiElement();

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


        public bool DevMenuEnabled = true;

        List<uint> renderedFrames = new List<uint>();
        uint frameId = 0;
        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            inst = this;
            curentLevel = new Level();
            UiElement.main = UiManger;

            _graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs> (graphics_PreparingDeviceSettings);
        }

        private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Window.ClientSizeChanged += Window_ClientSizeChanged;

            render = new Render();

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

            RenderThread = Thread.CurrentThread;
            

        }

        public static bool IsOnRenderThread()
        {
            return RenderThread == Thread.CurrentThread;
        }

        protected override void LoadContent()
        {
            if (DevMenuEnabled)
            {
                ImGuiRenderer = new ImGuiRenderer(this);
                ImGuiRenderer.RebuildFontAtlas();
                ImGui.StyleColorsDark();
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

            checkAppRegainedFocus();

            if (tick == 100)
                GameInitialized();

            time = gameTime;

            Thread.CurrentThread.Priority = ThreadPriority.Highest;

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
                    gameTask.Wait();
                    UpdateTime(gameTime);
                }
            }
            else
            {
                UpdateTime(gameTime);
            }

            //curentLevel.UpdatePending();
            curentLevel.LoadAssets();

            curentLevel.RenderPreparation();

            Input.Update();

            bool changedLevel = Level.LoadPendingLevel();
            if (AsyncGameThread && changedLevel == false)
            {
                gameTask = Task.Factory.StartNew(() => { GameLogic(); });
            }
            else
            {
                GameLogic();
            }

            frameId++;

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
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            Physics.Update();

            curentLevel.Update();

            curentLevel.AsyncUpdate();

            curentLevel.LateUpdate();

            SoundManager.Update();

            Camera.Update();


            SoundManager.Update();

            foreach (UiElement elem in UiElement.main.childs)
                elem.Update();

            tick++;
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
            if(renderedFrames.Contains(frameId))
            {
                Logger.Log("trying to render same frame twice");
                return;
            }

            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (pendingGraphicsUpdate)
                _graphics.ApplyChanges();

            RenderTarget2D frame =  render.StartRenderLevel(curentLevel);

            GraphicsDevice.SetRenderTarget(null);


            SpriteBatch.Begin();

            // Draw the render target to the screen
            SpriteBatch.Draw(frame, Vector2.Zero, Color.White);

            SpriteBatch.End();


            SpriteBatch.Begin(transformMatrix: Camera.UiMatrix, blendState: BlendState.AlphaBlend);

            UiManger.Draw(gameTime,SpriteBatch);

            SpriteBatch.DrawString(DefaultFont, (1f / Time.deltaTime).ToString(), new Vector2(100, 100), Color.White);

            SpriteBatch.End();

            if(DevMenuEnabled)
                ImGuiRenderer.BeforeLayout(gameTime);

            if (DevMenuEnabled)
                if (devMenu is not null)
                devMenu.Update();

            if (DevMenuEnabled)
                ImGuiRenderer.AfterLayout();

            

            //SetupFullViewport();
            base.Draw(gameTime);

            if(_graphics.IsFullScreen != Fullscreen)
            {
                _graphics.IsFullScreen = Fullscreen;
                pendingGraphicsUpdate = true;
            }

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

            _graphics.ApplyChanges();

            Fullscreen = true;
        }

        public virtual void GameInitialized()
        {

        }

        public virtual void OnLevelChanged()
        {
            Time.gameTime = 0;
        }

    }
}
