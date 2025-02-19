using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetroEngine.UI
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

        bool clicked = false;

        public Button() : base()
        {
            tex = AssetRegistry.LoadTextureFromFile("engine/textures/white.png", false, false);
        }

        public override void Update()
        {
            base.Update();

            delay -= Time.DeltaTime;

            if (GameMain.platform == Platform.Desktop)
            {
                if (pressing == false)
                {
                    pressing = Input.GetAction("click").Pressed() && hovering;
                }else
                {
                    pressing = Mouse.GetState().LeftButton == ButtonState.Pressed && hovering;
                }
            }
            else if (GameMain.platform == Platform.Mobile)
            {
                pressing = hovering;
            }

            if (hovering == false)
                clicked = false;

            if (pressing != oldPressing)
            {
                if (pressing)
                {
                    clicked = true;

                    Console.WriteLine(clicked);

                    if (onClicked != null && delay <= 0)
                    {
                        onClicked.Invoke();
                    }
                    delay = 0;
                }

                if (!pressing)
                    if (onReleased != null && clicked)
                    {
                        onReleased.Invoke();
                    }

            }


            oldPressing = pressing;

        }


        public delegate void OnClicked();

        public delegate void OnReleased();


        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Rectangle mainRectangle = new Rectangle();
            mainRectangle.Location = new Point((int)position.X+ (int)offset.X, (int)position.Y+ (int)offset.Y);
            mainRectangle.Size = new Point((int)size.X, (int)size.Y);

            spriteBatch.Draw(tex, mainRectangle, hovering ? hoveringColor : baseColor);

            base.Draw(gameTime, spriteBatch);

        }

    }
}
