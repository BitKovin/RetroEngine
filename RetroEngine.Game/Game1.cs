using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine.Entities;
using RetroEngine.Game.Entities;

namespace RetroEngine.Game
{
    public class Game : Engine.GameMain
    {
        protected override void LoadContent()
        {

            devMenu = new GameDevMenu();

            base.LoadContent();

            Player player = new Player();

            curentLevel.entities.Add(player);

            Engine.Camera.position = player.Position = new Vector3(0, -3, 0);

            Box box = new Box();
            box.Position = new Vector3(0, 0, 0);
            curentLevel.entities.Add(box);

            box.Start();

            BoxDynamic boxDynamic = new BoxDynamic();
            boxDynamic.Position = new Vector3(0,3, 0);
            curentLevel.entities.Add(boxDynamic);

            boxDynamic.Start();

        }
    }
}