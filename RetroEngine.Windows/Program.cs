
using RetroEngine;

LightManager.MAX_POINT_LIGHTS = 2;
GameMain.AsyncGameThread = false;
using var game = new RetroEngine.Game.Game();
game.Run();
