
using Microsoft.Xna.Framework;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RetroEngine.Windows
{
    internal class GameWindows : RetroEngine.Game.Game
    {

        public override void GameInitialized()
        {
            base.GameInitialized();

            var calc = new WindowsInputCalculator();

            Input.MouseMoveCalculatorObject = calc;
            
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

            MouseState state = mouse.GetCurrentState();

            Vector2 delta = new Vector2(state.X, state.Y);

            return delta / 3f;
        }
    }
}
