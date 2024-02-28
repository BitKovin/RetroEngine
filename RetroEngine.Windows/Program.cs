using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine;
using RetroEngine.Windows;
using System.Threading;
using System.Windows.Forms;

internal class Program
{
    private static void Main(string[] args)
    {

        Game game = new GameWindows();
        game.Run();

    }

}