﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.UI
{
    public class Button : UiElement
    {

        Texture2D tex;

        Color baseColor = Color.White;
        Color hoveringColor = Color.Gray;

        public bool pressing;

        bool oldPressing;


        public event OnClicked onClicked;

        public event OnReleased onReleased;

        float delay;

        public Button() : base()
        {
            tex = new Texture2D(GameMain.inst.GraphicsDevice, 1, 1);
            tex.SetData(new Color[] { Color.White });
        }

        public override void Update()
        {
            base.Update();

            delay -= Time.deltaTime;

            if (GameMain.platform == Platform.Desktop)
            {
                pressing = Mouse.GetState().LeftButton == ButtonState.Pressed && hovering;
            }
            else if (GameMain.platform == Platform.Mobile)
            {
                pressing = hovering;
            }

            if (pressing != oldPressing)
            {
                if (pressing)
                {
                    if (onClicked != null&&delay<=0)
                        onClicked.Invoke();
                    delay = 0;
                }
                if (!pressing)
                    if (onReleased != null)
                        onReleased.Invoke();
            }

            oldPressing = pressing;

        }


        public delegate void OnClicked();

        public delegate void OnReleased();


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Rectangle mainRectangle = new Rectangle();
            mainRectangle.Location = new Point((int)position.X+ (int)origin.X, (int)position.Y+ (int)origin.Y);
            mainRectangle.Size = new Point((int)size.X, (int)size.Y);

            spriteBatch.Draw(tex, mainRectangle, hovering ? hoveringColor : baseColor);

            base.Draw(gameTime, spriteBatch);

        }

    }
}
