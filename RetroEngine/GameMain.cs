﻿#define isDesktop

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Engine.UI;
using RetroEngine;
using MonoGame.ImGuiNet;

namespace Engine
{

    public enum Platform
    {
        Desktop,
        Mobile
    }

    public class GameMain : Game
    {
        SpriteFont font;


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

        protected DevMenu devMenu;

        public GameMain()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            inst = this;
            curentLevel = new Level();
            UiElement.main = UiManger;

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            

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
        }



        protected override void LoadContent()
        {
            ImGuiRenderer = new ImGuiRenderer(this);
            ImGuiRenderer.RebuildFontAtlas();

            SpriteBatch = new SpriteBatch(GraphicsDevice);
            content = Content;
            // TODO: use this.Content to load your game content here

            font = Content.Load<SpriteFont>("Font"); // Use the name of your sprite font file here instead of 'Score'.

            curentLevel.Start();
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            // Exit();

            time = gameTime;

            this.Exiting += Game1_Exiting;

            Time.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ScreenHeight = GraphicsDevice.PresentationParameters.Bounds.Height;

            ScreenWidth = GraphicsDevice.PresentationParameters.Bounds.Width;

            Input.Update();

            curentLevel.Update();

            curentLevel.AsyncUpdate();

            curentLevel.LateUpdate();


            Camera.Update();


            foreach (UiElement elem in UiElement.main.childs)
                elem.Update();
            // TODO: Add your update logic here

            base.Update(gameTime);
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
            GraphicsDevice.Clear(Color.CornflowerBlue);



            RenderTarget2D frame =  render.StartRenderLevel(curentLevel);

            GraphicsDevice.SetRenderTarget(null);


            SpriteBatch.Begin();

            // Draw the render target to the screen
            SpriteBatch.Draw(frame, Vector2.Zero, Color.White);

            SpriteBatch.End();


            SpriteBatch.Begin(transformMatrix: Camera.UiMatrix);

            UiManger.Draw(gameTime,SpriteBatch);

            SpriteBatch.End();

            SpriteBatch.Begin();

            SpriteBatch.DrawString(font, $"FPS: {(1f/Time.deltaTime).ToString()}", new Vector2(100, 100), Color.Red);
            SpriteBatch.DrawString(font, $"Camera Position: {Camera.position.ToString()}", new Vector2(100, 200), Color.Red);
            SpriteBatch.DrawString(font, $"Camera Vector: {Camera.rotation.GetForwardVector()}", new Vector2(100, 300), Color.Red);


            SpriteBatch.End();

            ImGuiRenderer.BeforeLayout(gameTime);

            if(devMenu is not null)
                devMenu.Update();

            ImGuiRenderer.AfterLayout();

            //SetupFullViewport();
            base.Draw(gameTime);
        }

        public object GetView(System.Type type)
        {
            return this.Services.GetService(type);
        }

    }
}