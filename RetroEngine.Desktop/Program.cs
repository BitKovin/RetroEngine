
using Microsoft.Xna.Framework;
using RetroEngine;
using RetroEngine.Audio;
using SharpDX.DirectInput;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

internal class Program
{

    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr h, string m, string c, int type);

    private static void Main(string[] args)
    {

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        SoundManager.nativeFmodLibrary = new FmodForFoxes.DesktopNativeFmodLibrary();

        RetroEngine.Render.UsesOpenGL = true;

        using var game = new RetroEngine.Game.Game();
        RetroEngine.GameMain.platform = RetroEngine.Platform.Desktop;
        Input.MouseMoveCalculatorObject = new WindowsInputCalculator();
        LightManager.MAX_POINT_LIGHTS = 6;
        Graphics.EnableSSR = false;

        if(args.Contains("compatibility") || args.Contains("-compatibility") || args.Contains("comp") || args.Contains("-comp"))
        {
            GameMain.CompatibilityMode = true;
        }

#if RELEASE
        try
        {
            game.Run();
        }
        catch (Exception ex)
        {
            ShowExeption(ex);


        }
#else
        game.Run();
#endif
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        ShowExeption((Exception)e.ExceptionObject);
    }

    static void ShowExeption(Exception ex)
    {
        var stream = File.CreateText("_crash_" + DateTime.Now.ToString().Replace("/", ".").Replace(":", "-") + ".txt");
        stream.Write(ex.ToString());
        stream.Close();

        MessageBox((IntPtr)0, ex.Message + "\nAn error occurred during work of the engine. If problem appears in unmodified version of the game on supported hardware, please contact developer. \nFile with error callstack was created. \n\ncallstack: \n" + ex.ToString(), ex.Message, 0);
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