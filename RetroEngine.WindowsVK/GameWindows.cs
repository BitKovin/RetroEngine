
using Microsoft.Xna.Framework;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

            CheckWindowFullscreenStatus();
            base.Update(gameTime);
        }


        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private static bool IsWindowFocused(IntPtr handle)
        {
            IntPtr focusedWindow = GetForegroundWindow();
            return handle == focusedWindow;
        }

        static Form form1;
        public override void CheckWindowFullscreenStatus()
        {
            base.CheckWindowFullscreenStatus();


            try
            {
                var form = Form.ActiveForm;

                if (form != null)
                {
                    form1 = form;
                }
                else
                {
                    form = form1;
                }

                bool focused = IsWindowFocused(Window.Handle);

                if (_isFullscreen && _graphics.IsFullScreen && focused == false)
                {
                    _graphics.IsFullScreen = false;
                    _graphics.ApplyChanges();
                    Window.

                }
                else if (_isFullscreen && _graphics.IsFullScreen == false && focused)
                {
                    _graphics.IsFullScreen = false;
                    _graphics.ApplyChanges();
                    form.Focus();
                    Thread.Sleep(100);
                    form.Focus();
                    Thread.Sleep(100);
                    form.Focus();
                    Thread.Sleep(100);
                    form.Focus();
                    _graphics.IsFullScreen = true;
                    _graphics.ApplyChanges();
                }

                if (form != null) //some times ActiveForm sets to null on alt tab and I have 0 idea why. It continues code(even with this check)
                {

                    if (form != null && _graphics.IsFullScreen && form.FormBorderStyle != FormBorderStyle.None)
                    {
                        form.FormBorderStyle = FormBorderStyle.None;
                        Window.Position = new Point(0, 0);
                    }
                    else if (form != null && _graphics.IsFullScreen == false && form.FormBorderStyle == FormBorderStyle.None)
                    {
                        form.FormBorderStyle = FormBorderStyle.Sizable;
                    }
                }
            }
            catch (Exception ex) { }

        }

        protected override void SetFullscreen()
        {

            base.SetFullscreen();

            CheckWindowFullscreenStatus();
        }

        protected override void UnsetFullscreen()
        {
            base.UnsetFullscreen();

            CheckWindowFullscreenStatus();
        }

        public override void GameInitialized()
        {
            base.GameInitialized();

            var calc = new WindowsInputCalculator();

            Input.MouseMoveCalculatorObject = calc;

            Window.Title = Window.Title + " DXVK";

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

            if (Input.LockCursor && GameMain.Instance.IsActive)
            {
                Input.PendingCenterCursor = true;
            }

            MouseState state = mouse.GetCurrentState();

            Vector2 delta = new Vector2(state.X, state.Y);

            return delta / 3f;
        }
    }
}
