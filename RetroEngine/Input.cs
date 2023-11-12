using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetroEngine
{
    public static class Input
    {
        public static Vector2 MousePos;
        public static Vector2 MouseDelta;

        static Dictionary<string, InputAction> actions = new Dictionary<string, InputAction>();

        public static bool LockCursor = true;

        public static float sensitivity = 0.5f;


        public static void Update()
        {

            Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            MouseDelta = mousePos - MousePos;

            MouseDelta *= sensitivity;

            Vector2 windowCenter = new Vector2(GameMain.inst.GraphicsDevice.Viewport.Width / 2, GameMain.inst.GraphicsDevice.Viewport.Height / 2);

            GameMain.inst.IsMouseVisible = !LockCursor;

            if (LockCursor) 
                if (Vector2.Distance(windowCenter, mousePos) > 1)
                {
                    Mouse.SetPosition((int)windowCenter.X, (int)windowCenter.Y);
                }

            MousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            UpdateActions();

        }

        public static void CenterCursor()
        {
            Vector2 windowCenter = new Vector2(GameMain.inst.GraphicsDevice.Viewport.Width / 2, GameMain.inst.GraphicsDevice.Viewport.Height / 2);
            Mouse.SetPosition((int)windowCenter.X, (int)windowCenter.Y);
            MouseDelta = new Vector2();
        }

        static void UpdateActions()
        {
            foreach (string key in actions.Keys)
            {
                actions[key].Update();
            }
        }

        public static InputAction GetAction(string actionName)
        {
            if (actions.ContainsKey(actionName) == false) return new InputAction();

            return actions[actionName];

        }

        public static InputAction AddAction(string actionName)
        {
            if (actions.ContainsKey(actionName) == false)
            {

                InputAction action = new InputAction();

                actions.Add(actionName, action);

                return action;
            }
            else
            {
                return actions[actionName];
            }
        }
    }

    public class InputAction
    {

        List<Keys> keys = new List<Keys>();
        List<Buttons> buttons = new List<Buttons>();

        bool lmbOld = false;
        bool rmbOld = false;
        bool mmbOld = false;

        public bool LMB = false;
        public bool RMB = false;
        public bool MMB = false;

        bool pressing;
        bool released;
        bool pressed;
        Keys[] keysOld = new Keys[0];
        public InputAction()
        {

        }

        public void AddKeyboardKey(Keys key) { keys.Add(key); }

        public void RemoveKeyboardKey(Keys key) { keys.Remove(key); }

        public void AddButton(Buttons button) { buttons.Add(button); }

        public void RemoveButton(Buttons button) { buttons.Remove(button); }

        public bool Pressed()
        {
            return pressed;
        }

        public bool Released()
        {
            return released;
        }

        public bool Holding()
        {
            return pressing;
        }

        public void Update()
        {
            bool newLmb = Mouse.GetState().LeftButton == ButtonState.Pressed;
            bool newRmb = Mouse.GetState().RightButton == ButtonState.Pressed;
            bool newMmb = Mouse.GetState().MiddleButton == ButtonState.Pressed;

            pressed = released = false;

            if (LMB)
            {
                //LMB
                if (!lmbOld && newLmb)
                {
                    lmbOld = newLmb;
                    pressed = true;
                    released = false;
                    pressing = true;
                }
                else if (lmbOld && !newLmb)
                {
                    lmbOld = newLmb;
                    pressed = false;
                    released = true;
                    pressing = false;
                }
            }

            if (RMB)
            {
                //RMB
                if (!rmbOld && newRmb)
                {
                    rmbOld = newRmb;
                    pressed = true;
                    released = false;
                    pressing = true;
                }
                else if (rmbOld && !newRmb)
                {
                    rmbOld = newRmb;
                    pressed = false;
                    released = true;
                    pressing = false;
                }
            }
            if (MMB)
            {
                //MMB
                if (!mmbOld && newMmb)
                {
                    mmbOld = newMmb;
                    pressed = true;
                    released = false;
                    pressing = true;
                }
                else if (mmbOld && !newMmb)
                {
                    mmbOld = newMmb;
                    pressed = false;
                    released = true;
                    pressing = false;
                }
            }
            //keyboard

            Keys[] keysNow = Keyboard.GetState().GetPressedKeys();

            foreach (Keys key in keysNow)
            {
                if (!keysOld.Contains(key))
                {
                    if (keys.Contains(key) == false) continue;

                    pressed = true;
                    pressing = true;
                    released = false;

                }
            }

            foreach (Keys key in keysOld)
            {
                if (!keysNow.Contains(key))
                {
                    if (keys.Contains(key) == false) continue;

                    pressed = false;
                    pressing = false;
                    released = true;
                }
            }

            keysOld = keysNow;

            //gamepad

            foreach(Buttons button in buttons)
            {
                bool buttonDown = GamePad.GetState(0).IsButtonDown(button);

                if(buttonDown&& !pressing)
                {
                    pressed = true;
                    pressing = true;
                    released = false;
                }

                if(!buttonDown && pressing)
                {
                    pressed = false;
                    pressing = false;
                    released = true;
                }

            }

        }

    }

}
