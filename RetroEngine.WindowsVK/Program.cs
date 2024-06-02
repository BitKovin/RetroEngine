using Microsoft.Xna.Framework;
using RetroEngine;
using RetroEngine.Audio;
using RetroEngine.Windows;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

internal class Program
{


    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr h, string m, string c, int type);

    private static void Main(string[] args)
    {
        SoundManager.nativeFmodLibrary = new FmodForFoxes.DesktopNativeFmodLibrary();
        Game game = new GameWindows();
#if RELEASE
        try
        {
            game.Run();
        }
        catch (Exception ex)
        {
            var stream = File.CreateText("_crash_" + DateTime.Now.ToString().Replace("/", ".").Replace(":", "-") + ".txt");
            stream.Write(ex.ToString());
            stream.Close();

            MessageBox((IntPtr)0, ex.Message + "\nAn error occurred during work of the engine. If problem appears on unmodified version of the game on supported hardware, please contact developer. \nFile with error callstack was created. \n\ncallstack: \n" + ex.ToString(), ex.Message, 0);


        }
#else
        game.Run();
#endif
    }

}