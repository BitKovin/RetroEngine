using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine;
using RetroEngine.Windows;
using System.Runtime.InteropServices;
using System;
using System.Threading;
using System.Windows.Forms;
using System.IO;

internal class Program
{

    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr h, string m, string c, int type);
    private static void Main(string[] args)
    {

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

            MessageBox((IntPtr)0, ex.Message + "\nAn error occurred during work of the engine. If problem appears on unmodified version of the game on supported hardware, please contact developer. \nFile with error callstack was created. \n\ncallstack: \n" + ex.ToString(), "Platform is not supported", 0);


        }
#else
        game.Run();
#endif

    }

}