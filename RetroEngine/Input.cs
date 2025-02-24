﻿using ImGuiNET;
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

        internal static MouseState mouseState;
        internal static KeyboardState keyboardState;
        internal static GamePadState gamePadState;

        public static void Update()
        {

            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);

            

            UpdateActions();

            if (PendingCenterCursor)
                CenterCursor();

            windowCenter = new Vector2(GameMain.Instance.GraphicsDevice.Viewport.Width / 2, GameMain.Instance.GraphicsDevice.Viewport.Height / 2);

            GameMain.Instance.IsMouseVisible = !LockCursor;

        }

        static void JoystickCamera()
        {
            MouseDelta += gamePadState.ThumbSticks.Right*Time.DeltaTime*1500 * new Vector2(1,-1);
        }

        public static void UpdateMouse()
        {

            Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

            if (MouseMoveCalculatorObject != null)
            {

                MousePos = mousePos;

                

                MouseDelta = MouseMoveCalculatorObject.GetMouseDelta();

                AddMouseInput(MouseDelta);

                JoystickCamera();

                MouseDelta *= sensitivity;



                return;
            }

            Vector2 delta = mousePos - MousePos;

            AddMouseInput(delta);

            JoystickCamera();

            MouseDelta *= sensitivity;


            if (LockCursor&&GameMain.Instance.IsActive)
                if (Vector2.Distance(windowCenter, mousePos) > 0)
                {

                    Mouse.SetPosition((int)windowCenter.X, (int)windowCenter.Y);
                    MousePos = new Vector2(mouseState.X, mouseState.Y);
                }
                else
                {
                    MousePos = new Vector2(mouseState.X, mouseState.Y);
                }
            else { MousePos = new Vector2(mouseState.X, mouseState.Y); }
            
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
            if(GameMain.CanLoadAssetsOnThisThread() && GameMain.Instance.IsGameWindowFocused())
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

        public bool LMB = false;
        public bool RMB = false;
        public bool MMB = false;

        bool pressing;
        bool released;
        bool pressed;

        double pressedTime = 0f;

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

        public bool PressedBuffered(float bufferLength = 0.2f)
        {
            return pressedTime + bufferLength >= Time.GameTime;
        }

        public void Update()
        {
            bool newLmb = Input.mouseState.LeftButton == ButtonState.Pressed;
            bool newRmb = Input.mouseState.RightButton == ButtonState.Pressed;
            bool newMmb = Input.mouseState.MiddleButton == ButtonState.Pressed;

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

            Keys[] keysNow = Input.keyboardState.GetPressedKeys();

            foreach(Keys key in keysNow)
            {
                if(keys.Contains(key))
                    pressing = true;
            }

            //gamepad

            foreach(Buttons button in buttons)
            {
                bool buttonDown = Input.gamePadState.IsButtonDown(button);

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

                pressedTime = Time.GameTime;

            }
            else if (!pressing & oldPressing)
            {
                released = true;
            }

        }

    }

}
