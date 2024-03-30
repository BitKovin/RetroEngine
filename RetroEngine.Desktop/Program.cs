
using Microsoft.Xna.Framework;
using RetroEngine;
using SharpDX.DirectInput;

internal class Program
{
    private static void Main(string[] args)
    {
        using var game = new RetroEngine.Game.Game();
        RetroEngine.GameMain.platform = RetroEngine.Platform.Desktop;
        Input.MouseMoveCalculatorObject = new WindowsInputCalculator();
        Graphics.EnableSSR = false;
        game.Run();
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

        public override Vector2 GetMouseDelta()
        {
            mouse.Poll();

            if (Input.LockCursor && GameMain.Instance.IsActive)
            {
                Microsoft.Xna.Framework.Input.Mouse.SetPosition((int)Input.windowCenter.X, (int)Input.windowCenter.Y);
            }

            MouseState state = mouse.GetCurrentState();

            Vector2 delta = new Vector2(state.X, state.Y);

            return delta / 3f;
        }
    }

}