
using Microsoft.Xna.Framework;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroEngine.Windows
{
    internal class GameWindows : RetroEngine.Game.Game
    {

        public GameWindows() 
        {
            AllowAsyncAssetLoading = true;
        }

        protected override void Update(GameTime gameTime)
        {

            if (Form.ActiveForm != null) //some times ActiveForm sets to null on alt tab and I have 0 idea why. It continues code(even with this check)
            {

                if (Form.ActiveForm != null && _graphics.IsFullScreen && Form.ActiveForm.FormBorderStyle != FormBorderStyle.None)
                {
                    Form.ActiveForm.FormBorderStyle = FormBorderStyle.None;
                    Window.Position = new Point(0, 0);
                }
                else if (Form.ActiveForm != null && _graphics.IsFullScreen == false && Form.ActiveForm.FormBorderStyle == FormBorderStyle.None)
                {
                    Form.ActiveForm.FormBorderStyle = FormBorderStyle.Sizable;
                }
            }

            base.Update(gameTime);
        }

        protected override void SetFullscreen()
        {
            base.SetFullscreen();

            if (Form.ActiveForm != null)//some times ActiveForm sets to null on alt tab and I have 0 idea why. It continues code(even with this check)
            {

                if (Form.ActiveForm != null && _graphics.IsFullScreen && Form.ActiveForm.FormBorderStyle != FormBorderStyle.None)
                {
                    Form.ActiveForm.FormBorderStyle = FormBorderStyle.None;
                    Window.Position = new Point(0, 0);
                }
                else if (Form.ActiveForm != null && _graphics.IsFullScreen == false && Form.ActiveForm.FormBorderStyle == FormBorderStyle.None)
                {
                    Form.ActiveForm.FormBorderStyle = FormBorderStyle.Sizable;
                }
            }
        }

        public override void GameInitialized()
        {
            base.GameInitialized();

            var calc = new WindowsInputCalculator();

            Input.MouseMoveCalculatorObject = calc;

            Window.Title = Window.Title + " Direct X";

#if DEBUG

            Window.Title = Window.Title + " (Debug)";

#endif

            AsyncGameThread = true;

        }

    }


    class WindowsInputCalculator : Input.MouseMoveCalculator
    {

        Mouse mouse;

        public WindowsInputCalculator() 
        {
            DirectInput directInput = new DirectInput();

            mouse = new Mouse(directInput);
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

            MouseState state = mouse.GetCurrentState();

            Vector2 delta = new Vector2(state.X, state.Y);

            return delta / 3f;
        }
    }
}
