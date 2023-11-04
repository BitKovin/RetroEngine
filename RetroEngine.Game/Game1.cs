using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine.Entities;
using RetroEngine.Game.Entities;
using System;

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

            player.Start();

            Engine.Camera.position = player.Position = new Vector3(0, 3, -2);

            Box box = new Box();
            box.size = new Vector3(5, 5, 5);
            box.Position = new Vector3(0, -4, 0);
            curentLevel.entities.Add(box);

            box.Start();

            Box box2 = new Box();
            box2.Position = new Vector3(0, -6, 0);
            box2.size = new Vector3(100, 1, 100);
            curentLevel.entities.Add(box2);

            box2.Start();

            Box box3 = new Box();
            box3.size = new Vector3(7, 5, 5);
            box3.Position = new Vector3(0, -1, 0);
            curentLevel.entities.Add(box3);

            box3.Start();

            for (float i = 1; i < 1000; i++)
            {
                BoxDynamic boxDynamic = new BoxDynamic();
                boxDynamic.Position = new Vector3(0, i*6, (float)Math.Sin(i*5) * 0.4f);
                curentLevel.entities.Add(boxDynamic);

                boxDynamic.Start();
            }

        }
    }
}