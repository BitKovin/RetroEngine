﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class LoadingScreen
    {

        public static float Progress = 0;

        public static void Draw()
        {

            GameMain.Instance.GraphicsDevice.Clear(Color.Blue);

            SpriteBatch SpriteBatch = GameMain.Instance.SpriteBatch;

            SpriteBatch.Begin();

            Rectangle screenRectangle = new Rectangle(0, 0, GameMain.Instance.GraphicsDevice.Viewport.Width, GameMain.Instance.GraphicsDevice.Viewport.Height);

            Texture2D background = AssetRegistry.LoadTextureFromFile("cat.png", false, false);

            // Draw the render target to the screen
            SpriteBatch.Draw(background, screenRectangle, Color.White);

            Texture2D white = AssetRegistry.LoadTextureFromFile("engine/textures/white.png", false, false);

            Rectangle loadingBar = new Rectangle(0, GameMain.Instance.GraphicsDevice.Viewport.Height - 20, (int)((float)GameMain.Instance.GraphicsDevice.Viewport.Width * Progress), 20);

            SpriteBatch.Draw(white, loadingBar, Color.Red);

            SpriteBatch.End();

            GameMain.Instance.GraphicsDevice.Present();

        }

        public static void Update(float progress)
        {
            Progress = progress;
            Draw();
        }

    }
}