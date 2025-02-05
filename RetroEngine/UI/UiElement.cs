using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using RetroEngine;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Linq;

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
        public static Vector2 Bottom = new Vector2(0, 1);
        public static Vector2 Left = new Vector2();
        public static Vector2 Right = new Vector2(1, 0);
        public static Vector2 CenterH = new Vector2(0.5f,0);
        public static Vector2 CenterV = new Vector2(0, 0.5f);

        public static Vector2 Get(Origin origin)
        {

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
        protected List<UiElement> childs = new List<UiElement>();

        internal List<UiElement> finalizedChilds = new List<UiElement>();

        public static UiElement Viewport;

        public bool hovering;
        protected Collision2D col = new Collision2D();

        public Vector2 size = new Vector2(1,1);

        public Vector2 position = new Vector2();
        public float rotation = 0;

        public Vector2 Origin = new Vector2();

        public Vector2 Pivot = new Vector2();

        public Vector2 relativeOrigin = new Vector2();

        public Vector2 offset;

        protected Vector2 ParrentTopLeft;
        protected Vector2 ParrentBottomRight;

        public UiElement parrent { get; private set; }

        public bool Visible = true;

        public bool DrawBorder = false;

        public static bool DrawAllBorder = false;

        public Vector2 TopLeft {  get; private set; }   
        public Vector2 BottomRight { get; private set; }

        public UiElement()
        {
        }

        [ConsoleCommand("ui.border")]
        public static void SetDrawDebugBorder(bool value)
        {
            DrawAllBorder = value;
        }

        public void UpdateOffsets()
        {

            var origin = GetOrigin();
            var size = GetSize();

            offset = origin - size * Pivot;

            if(float.IsInfinity(offset.X) || float.IsNaN(offset.X))
            {
                throw new Exception("ui is broken fix it");
            }

            TopLeft = position + offset;
            BottomRight = position + offset + GetSize();
        }

        public void UpdateChildren()
        {
            lock (childs)
            {
                var list = childs.ToArray();

                foreach (UiElement element in list)
                {
                    lock (element)
                    {
                        element.ParrentTopLeft = TopLeft;
                        element.ParrentBottomRight = BottomRight;
                        element.parrent = this;
                        element.Update();
                    }
                }
            }
        }

        public void UpdateChildrenOffsets()
        {
            lock (childs)
            {
                var list = childs.ToArray();

                foreach (UiElement element in list)
                {
                    lock (element)
                    {
                        element.ParrentTopLeft = TopLeft;
                        element.ParrentBottomRight = BottomRight;
                        element.parrent = this;
                        element.UpdateOffsets();
                    }
                }
            }
        }

        internal void FinalizeChilds()
        {
            finalizedChilds = childs.ToList();
            

            foreach (UiElement element in finalizedChilds)
                element.FinalizeChilds();

        }

        public virtual void Update()
        {

            UpdateChildrenOffsets();
            UpdateOffsets();
            UpdateChildrenOffsets();
            UpdateOffsets();
            UpdateChildrenOffsets();
            UpdateOffsets();
            UpdateChildrenOffsets();
            UpdateOffsets();

            UpdateChildren();

            UpdateChildrenOffsets();
            UpdateOffsets();


            float ScaleY = GameMain.Instance.Window.ClientBounds.Height / UiViewport.GetViewportHeight();
            //float HtV = ((float)GameMain.Instance.Window.ClientBounds.Width) / ((float)GameMain.Instance.Window.ClientBounds.Height);

            if (GameMain.platform == Platform.Desktop)
            {

                col.size = new Point((int)GetSize().X, (int)GetSize().Y);
                col.position = new Vector2((int)position.X + (int)offset.X, (int)position.Y + (int)offset.Y);
                Collision2D mouseCol = new Collision2D();
                mouseCol.size = new Point(2, 2);
                mouseCol.position = new Vector2(Input.MousePos.X, Input.MousePos.Y) / ScaleY;
                mouseCol.position -= new Vector2(1,1);
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

                    col.size = new Point((int)GetSize().X, (int)GetSize().Y);
                    col.position = new Vector2((int)position.X+ (int)offset.X, (int)position.Y+(int)offset.Y);
                    Collision2D mouseCol = new Collision2D();
                    mouseCol.size = new Point(2, 2);
                    mouseCol.position = new Vector2((int)pos.X, (int)pos.Y);
                    if (Collision2D.MakeCollionTest(col, mouseCol))
                        hovering = true;

                }
            }

        }

        public virtual Vector2 GetOrigin()
        {
            return new Vector2(
                MathHelper.Lerp(ParrentTopLeft.X, ParrentBottomRight.X, Origin.X),
                MathHelper.Lerp(ParrentTopLeft.Y, ParrentBottomRight.Y, Origin.Y));
        }

        public virtual Vector2 GetSize()
        {
            return size;
        }

        public virtual void AddChild(UiElement child)
        {
            lock(childs)
            {
                child.parrent = this;
                childs.Add(child);
            }
        }

        public virtual void RemoveChild(UiElement child)
        {
            lock(childs)
            {
                childs.Remove(child);
            }
        }

        public virtual void ClearChild()
        {
            lock(childs)
            {
                childs.Clear();
            }
        }

        public static Vector2 WorldToScreenSpace(Vector3 pos)
        {
            Vector4 position = new Vector4(pos, 1);
            Matrix ViewProjection = Camera.CalculateView() * Camera.projection;
            Vector4 projectedPosition = Vector4.Transform(position, ViewProjection);

            // Normalize the projected position
            Vector2 screenSpacePosition = new Vector2(
                projectedPosition.X / projectedPosition.W,
                projectedPosition.Y / projectedPosition.W);

            // Map to the screen coordinates
            float halfScreenWidth = UiViewport.GetViewportHeight() * Camera.HtW / 2f;
            float halfScreenHeight = UiViewport.GetViewportHeight() / 2f;

            screenSpacePosition.X = halfScreenWidth + halfScreenWidth * screenSpacePosition.X;
            screenSpacePosition.Y = halfScreenHeight - halfScreenHeight * screenSpacePosition.Y;

            return screenSpacePosition;
        }

        public static Vector2 WorldToScreenSpace(Vector3 pos, out bool inScreen)
        {
            Vector4 position = new Vector4(pos, 1);
            Matrix ViewProjection = Camera.CalculateView() * Camera.projection;
            Vector4 projectedPosition = Vector4.Transform(position, ViewProjection);

            // Normalize the projected position
            Vector2 screenSpacePosition = new Vector2(
                projectedPosition.X / projectedPosition.W,
                projectedPosition.Y / projectedPosition.W);

            // Map to the screen coordinates
            float halfScreenWidth = UiViewport.GetViewportHeight() * Camera.HtW / 2f;
            float halfScreenHeight = UiViewport.GetViewportHeight() / 2f;

            screenSpacePosition.X = halfScreenWidth + halfScreenWidth * screenSpacePosition.X;
            screenSpacePosition.Y = halfScreenHeight - halfScreenHeight * screenSpacePosition.Y;

            inScreen = projectedPosition.Z > 0;

            return screenSpacePosition;
        }


        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            foreach (UiElement element in finalizedChilds)
                if(element.Visible)
                    element.Draw(gameTime, spriteBatch);


            if (DrawBorder || DrawAllBorder)
            {
                Rectangle mainRectangle = new Rectangle();
                mainRectangle.Location = new Point((int)position.X + (int)offset.X, (int)position.Y + (int)offset.Y);
                mainRectangle.Size = new Point((int)GetSize().X, (int)GetSize().Y);

                spriteBatch.Draw(AssetRegistry.LoadTextureFromFile("engine/textures/border.png"), mainRectangle, new Color(1, 0, 0, 0.3f));
            }

        }
    }

}