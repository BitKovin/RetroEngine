using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public static class Input
    {
        public static Vector2 MousePos;
        public static Vector2 MouseDelta;
        static List<Keys> oldKeys = new List<Keys>();
        public static List<Keys> pressedKeys = new List<Keys>();
        public static List<Keys> releasedKeys = new List<Keys>();

        public static bool LockCursor = true;

        public static void Update()
        {

            float ScaleY = (float)GameMain.inst.Window.ClientBounds.Height / Constants.ResoultionY;

            Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            MouseDelta = mousePos - MousePos;
            MouseDelta /= ScaleY;

            Vector2 windowCenter = new Vector2(GameMain.inst.GraphicsDevice.Viewport.Width / 2, GameMain.inst.GraphicsDevice.Viewport.Height / 2);

            if (LockCursor)
                if (Vector2.Distance(windowCenter, mousePos) > 1)
                {
                    Mouse.SetPosition((int)windowCenter.X, (int)windowCenter.Y);
                }

            MousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            


            pressedKeys.Clear();
            releasedKeys.Clear();

            Keys[] keysNow = Keyboard.GetState().GetPressedKeys();
            List<Keys> keysOld = oldKeys;
            List<Keys> pressingKeys = new List<Keys>();

            foreach (Keys key in keysNow)
            {
                pressingKeys.Add(key);
            }


            pressedKeys.Clear();
            foreach(Keys key in keysNow)
            {
                if (!oldKeys.Contains(key))
                    pressedKeys.Add(key);
            }

            foreach (Keys key in keysOld)
            {
                if (!pressingKeys.Contains(key))
                {
                    releasedKeys.Add(key);
                }
            }

            oldKeys.Clear();
            foreach (Keys key in Keyboard.GetState().GetPressedKeys())
                oldKeys.Add(key);

        }

    }
}
