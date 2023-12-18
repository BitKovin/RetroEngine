
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


        public override void GameInitialized()
        {
            base.GameInitialized();

            var calc = new WindowsInputCalculator();

            Input.MouseMoveCalculatorObject = calc;

            Window.Title = Window.Title + "   Direct X";

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

            if(Input.LockCursor)
            {
                Microsoft.Xna.Framework.Input.Mouse.SetPosition((int)Input.windowCenter.X, (int)Input.windowCenter.Y);
            }

            MouseState state = mouse.GetCurrentState();

            Vector2 delta = new Vector2(state.X, state.Y);

            return delta / 3f;
        }
    }
}
