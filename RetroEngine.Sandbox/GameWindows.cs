
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RetroEngine.Entities;
using SharpDX.DirectInput;
using SkeletalMeshEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RetroEngine.Sandbox
{
    internal class GameWindows : RetroEngine.Game.Game
    {

        public GameWindows() 
        {
            AllowAsyncAssetLoading = true;

        }


        public override void GameInitialized()
        {
            base.GameInitialized();

            LevelObjectFactory.InitializeTypeCache();

            var calc = new WindowsInputCalculator();

            devMenu = new SandboxMenu();
            devMenu.Init();

            

            MaxFPS = 140;

            //Graphics.LightDistanceMultiplier = 0.5f;

            //Graphics.DisableBackFaceCulling = true;

            CreateInputActions();

            Input.MouseMoveCalculatorObject = calc;

            Window.Title = "Sandbox";

#if DEBUG

            Window.Title = Window.Title + " (Debug)";

#endif

            AsyncGameThread = false;

            Level.LoadFromFile("empty");

        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            DevMenuEnabled = true;

        }

        public override void OnLevelChanged()
        {
            base.OnLevelChanged();

            Level.GetCurrent().AddEntity(new FreeCamera());

        }

        void CreateInputActions()
        {
            Input.AddAction("pause").AddKeyboardKey(Keys.Escape);

            Input.AddAction("space").AddKeyboardKey(Keys.Space).AddButton(Buttons.A);
            Input.AddAction("lmb").AddButton(Buttons.RightTrigger).LMB = true;
            Input.AddAction("rmb").AddButton(Buttons.LeftTrigger).RMB = true;

            Input.AddAction("r").AddKeyboardKey(Keys.R);

            Input.AddAction("moveForward").AddKeyboardKey(Keys.W).AddButton(Buttons.LeftThumbstickUp);
            Input.AddAction("moveBackward").AddKeyboardKey(Keys.S).AddButton(Buttons.LeftThumbstickDown);
            Input.AddAction("moveLeft").AddKeyboardKey(Keys.A).AddButton(Buttons.LeftThumbstickLeft);
            Input.AddAction("moveRight").AddKeyboardKey(Keys.D).AddButton(Buttons.LeftThumbstickRight);

            Input.AddAction("fullscreen").AddKeyboardKey(Keys.F11);
        }

    }


    class WindowsInputCalculator : Input.MouseMoveCalculator
    {

        SharpDX.DirectInput.Mouse mouse;

        public WindowsInputCalculator() 
        {
            DirectInput directInput = new DirectInput();

            mouse = new SharpDX.DirectInput.Mouse(directInput);
            mouse.Acquire();

        }

        Vector2 oldPos = new Vector2();

        public override Vector2 GetMouseDelta()
        {
            mouse.Poll();

            if(Input.LockCursor && GameMain.Instance.IsActive)
            {
                Input.PendingCenterCursor = true;
            }

            SharpDX.DirectInput.MouseState state = mouse.GetCurrentState();

            Vector2 delta = new Vector2(state.X, state.Y);

            return delta / 3f;
        }
    }
}
