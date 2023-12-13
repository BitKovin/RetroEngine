using RetroEngine;
using SharpDX.DirectInput;

internal class Program
{
    private static void Main(string[] args)
    {
        LightManager.MAX_POINT_LIGHTS = 5;
        using var game = new RetroEngine.Game.Game();
        game.Run();
    }
}