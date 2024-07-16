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

        static List<Vector2> MouseDeltas = new List<Vector2>();
        static int MaxDeltas = 0;

        static Dictionary<string, InputAction> actions = new Dictionary<string, InputAction>();

        public static bool LockCursor = false;

        public static float sensitivity = 0.2f;

        public static Vector2 windowCenter;

        public static bool PendingCenterCursor = false;

        public static MouseMoveCalculator MouseMoveCalculatorObject;

        public static void Update()
        {
            UpdateActions();

            if (PendingCenterCursor)
                CenterCursor();

            windowCenter = new Vector2(GameMain.Instance.GraphicsDevice.Viewport.Width / 2, GameMain.Instance.GraphicsDevice.Viewport.Height / 2);

            GameMain.Instance.IsMouseVisible = !LockCursor;

        }

        public static void UpdateMouse()
        {

            

            Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
           

            

            if (MouseMoveCalculatorObject != null)
            {

                MouseDelta = MouseMoveCalculatorObject.GetMouseDelta();

                AddMouseInput(MouseDelta);
                MouseDelta *= sensitivity;

                MousePos = mousePos;

                return;
            }

            Vector2 delta = mousePos - MousePos;

            AddMouseInput(delta);

            MouseDelta *= sensitivity;


            if (LockCursor&&GameMain.Instance.IsActive)
                if (Vector2.Distance(windowCenter, mousePos) > 0)
                {

                    Mouse.SetPosition((int)windowCenter.X, (int)windowCenter.Y);
                    MousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                }
                else
                {
                    MousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                }
            else { MousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y); }
            
        }

        public static void AddMouseInput(Vector2 delta)
        {
            if (MouseDeltas.Count > MaxDeltas)
                MouseDeltas.RemoveAt(0);

            MouseDeltas.Add(delta);

            Vector2 vector = new Vector2(0);

            foreach(Vector2 v in MouseDeltas)
            {
                vector += v;
            }

            MouseDelta = vector / MouseDeltas.Count;

        }

        public static void CenterCursor()
        {
            if(GameMain.CanLoadAssetsOnThisThread())
            {
                Mouse.SetPosition((int)windowCenter.X, (int)windowCenter.Y);
                MousePos = windowCenter;
                MouseDelta = new Vector2();
                MouseDeltas.Clear();
                PendingCenterCursor = false;
            }else
            {
                PendingCenterCursor = true;
            }
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

        public class MouseMoveCalculator
        {
            public virtual Vector2 GetMouseDelta()
            {
                return new Vector2();
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

        public InputAction AddKeyboardKey(Keys key) { keys.Add(key); return this; }

        public InputAction RemoveKeyboardKey(Keys key) { keys.Remove(key); return this; }

        public InputAction AddButton(Buttons button) { buttons.Add(button); return this; }

        public InputAction RemoveButton(Buttons button) { buttons.Remove(button); return this; }

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

            bool oldPressing = pressing;

            pressed = released = pressing = false;

            if (LMB)
            {
                if(newLmb)
                    pressing = true;
            }

            if (RMB)
            {
                if (newRmb)
                    pressing = true;
            }
            if (MMB)
            {
                if(newMmb)
                    pressing = true;
            }
            //keyboard

            Keys[] keysNow = Keyboard.GetState().GetPressedKeys();

            foreach(Keys key in keysNow)
            {
                if(keys.Contains(key))
                    pressing = true;
            }

            keysOld = keysNow;

            //gamepad

            foreach(Buttons button in buttons)
            {
                bool buttonDown = GamePad.GetState(0).IsButtonDown(button);

                if(buttonDown&& !pressing)
                {
                    pressing = true;
                }

                if(!buttonDown && pressing)
                {

                }

            }


            if(pressing && !oldPressing)
            {
                pressed = true;
            }else if (!pressing & oldPressing)
            {
                released = true;
            }

        }

    }

}
