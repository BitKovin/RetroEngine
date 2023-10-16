
internal class Program
{
    private static void Main(string[] args)
    {
        using var game = new RetroEngine.Game.Game();
        Engine.GameMain.platform = Engine.Platform.Desktop;
        game.Run();
    }
}