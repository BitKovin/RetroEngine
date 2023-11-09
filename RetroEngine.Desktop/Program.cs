
internal class Program
{
    private static void Main(string[] args)
    {
        using var game = new RetroEngine.Game.Game();
        RetroEngine.GameMain.platform = RetroEngine.Platform.Desktop;
        game.Run();
    }
}