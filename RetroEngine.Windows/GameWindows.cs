
using Microsoft.Xna.Framework;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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


            if (_graphics.HardwareModeSwitch)
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

                    if (form != null) //some times ActiveForm sets to null on alt tab and I have 0 idea why. It continues code(even with this check)
                    {

                        if (_isFullscreen)
                        {
                            if (Level.ChangingLevel == true)
                            {
                                //form.TopMost = true;
                                form.Focus();
                                form.WindowState = FormWindowState.Maximized;
                            }
                            else
                            {
                                //form.TopMost = false;
                            }
                        }

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

                    bool focused = IsWindowFocused(Window.Handle);

                    if (_isFullscreen && _graphics.IsFullScreen && focused == false)
                    {
                        //form.TopMost = false;
                        _graphics.IsFullScreen = false;
                        form.FormBorderStyle = FormBorderStyle.Sizable;
                        form.WindowState = FormWindowState.Minimized;
                        _graphics.ApplyChanges();

                    }
                    else if (_isFullscreen && _graphics.IsFullScreen == false && focused && form.WindowState != FormWindowState.Minimized)
                    {

                        int _width = Graphics.Resolution.X;
                        int _height = Graphics.Resolution.Y;

                        form.FormBorderStyle = FormBorderStyle.None;
                        Window.Position = new Point(0, 0);

                        _graphics.PreferredBackBufferWidth = _width;
                        _graphics.PreferredBackBufferHeight = _height;
                        _graphics.HardwareModeSwitch = true;
                        _graphics.IsFullScreen = false;
                        _graphics.ApplyChanges();
                        form.WindowState = FormWindowState.Maximized;
                        form.Focus();
                        Thread.Sleep(100);
                        form.Focus();
                        _graphics.PreferredBackBufferWidth = _width;
                        _graphics.PreferredBackBufferHeight = _height;
                        _graphics.ApplyChanges();
                        Thread.Sleep(100);
                        form.WindowState = FormWindowState.Maximized;
                        form.Focus();
                        Thread.Sleep(100);
                        _graphics.PreferredBackBufferWidth = _width;
                        _graphics.PreferredBackBufferHeight = _height;
                        form.Focus();
                        _graphics.IsFullScreen = true;
                        _graphics.ApplyChanges();
                    }
                    return;
                    
                }
                catch (Exception ex) { }

        }

        protected override void SetFullscreen()
        {

            base.SetFullscreen();

            GraphicsDevice.Present();

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
