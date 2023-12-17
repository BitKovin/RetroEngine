using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using RetroEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace RetroEngine.UI
{

    public enum Origin
    {
        Top,
        Bottom,
        Left,
        Right,
        CenterH,
        CenterV,
    }

    public static class Origins
    {
        public static Vector2 Top = new Vector2();
        public static Vector2 Bottom = new Vector2(0, Constants.ResoultionY);
        public static Vector2 Left = new Vector2();
        public static Vector2 Right = new Vector2(Constants.ResoultionY*Camera.HtW, 0);
        public static Vector2 CenterH = new Vector2(Constants.ResoultionY*Camera.HtW/2,0);
        public static Vector2 CenterV = new Vector2(0, Constants.ResoultionY/2);

        public static Vector2 Get(Origin origin)
        {

        Right = new Vector2(Constants.ResoultionY * Camera.HtW, 0);
        CenterH = new Vector2(Constants.ResoultionY * Camera.HtW / 2, 0);
        CenterV = new Vector2(0, Constants.ResoultionY / 2);

            switch (origin)
            {
                default:
                    return new Vector2();

                case Origin.Top:
                    return Top;

                case Origin.Bottom:
                    return Bottom;

                case Origin.Left:
                    return Left;

                case Origin.Right:
                    return Right;

                case Origin.CenterH:
                    return CenterH;

                case Origin.CenterV:
                    return CenterV;

            }

        }

    }

    public class UiElement
    {
        public List<UiElement> childs = new List<UiElement>();

        public static UiElement main;

        public bool hovering;
        public Collision2D col = new Collision2D();

        public Vector2 size = new Vector2(1,1);

        public Vector2 position = new Vector2();
        public float rotation = 0;

        public Origin originH;
        public Origin originV;

        public Vector2 relativeOrigin = new Vector2();

        protected Vector2 origin;

        public UiElement()
        {
        }

        public virtual void Update()
        {
            float ScaleY = GameMain.Instance.Window.ClientBounds.Height / Constants.ResoultionY;
            float HtV = ((float)GameMain.Instance.Window.ClientBounds.Width) / ((float)GameMain.Instance.Window.ClientBounds.Height);
            Origins.Right = new Vector2(Constants.ResoultionY * HtV, 0);


            origin = Origins.Get(originH) + Origins.Get(originV);

            if (GameMain.platform == Platform.Desktop)
            {

                col.size = new Point((int)size.X, (int)size.Y);
                col.position = new Vector2((int)position.X + (int)origin.X, (int)position.Y + (int)origin.Y);
                Collision2D mouseCol = new Collision2D();
                mouseCol.size = new Point(2, 2);
                mouseCol.position = new Vector2((int)Input.MousePos.X, (int)Input.MousePos.Y);
                hovering = Collision2D.MakeCollionTest(col, mouseCol);
            }
            else if (GameMain.platform == Platform.Mobile)
            {
                hovering = false;
                var touchCol = TouchPanel.GetState();
                Vector2 pos;
                foreach (var touch in touchCol)
                {
                    pos = touch.Position / ScaleY;

                    col.size = new Point((int)size.X, (int)size.Y);
                    col.position = new Vector2((int)position.X+ (int)origin.X, (int)position.Y+(int)origin.Y);
                    Collision2D mouseCol = new Collision2D();
                    mouseCol.size = new Point(2, 2);
                    mouseCol.position = new Vector2((int)pos.X, (int)pos.Y);
                    if (Collision2D.MakeCollionTest(col, mouseCol))
                        hovering = true;

                }
            }

        }



        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (UiElement element in childs)
                element.Draw(gameTime, spriteBatch);
        }
    }

}