using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine.Entities;
using RetroEngine.Game.Entities;
using System;
using RetroEngine.MapParser;
using RetroEngine.Map;
using Engine;

namespace RetroEngine.Game
{
    public class Game : Engine.GameMain
    {
        protected override void LoadContent()
        {

            devMenu = new GameDevMenu();

            base.LoadContent();


            MapData mapData = MapParser.MapParser.ParseMap("test.map");

            GameMain.inst.curentLevel = mapData.GetLevel();

            Player player = new Player();

            curentLevel.entities.Add(player);

            

            Vector3 startPos = mapData.GetEntityDataFromClass("info_player_start").GetPropertyVector("origin");

            Engine.Camera.position = player.Position = (startPos + new Vector3(0,0,0));

            player.Start();
            foreach (var ent in mapData.Entities)
            {
                foreach (var brush in ent.Brushes)
                {
                    foreach(Vector3 vertex in brush.Vertices)
                    {
                        Box boxB = new Box();
                        boxB.size = new Vector3(1, 1, 1);
                        boxB.Position = vertex;
                        GameMain.inst.curentLevel.entities.Add(boxB);
                    }
                }
            }

            Box box = new Box();
            box.size = new Vector3(1, 1, 1);
            box.Position = new Vector3(0, -4, 0);
            curentLevel.entities.Add(box);

            box.Start();

            Box box2 = new Box();
            box2.Position = new Vector3(0, -6, 0);
            box2.size = new Vector3(100, 1, 100);
            //curentLevel.entities.Add(box2);

            //box2.Start();

            Box box3 = new Box();
            box3.size = new Vector3(7, 5, 5);
            box3.Position = new Vector3(0, -1, 0);
            //curentLevel.entities.Add(box3);

            //box3.Start();

            for (float i = 1; i < 0; i++)
            {
                BoxDynamic boxDynamic = new BoxDynamic();
                boxDynamic.Position = new Vector3(0, i * 6, (float)Math.Sin(i * 5) * 0.4f);
                curentLevel.entities.Add(boxDynamic);

                boxDynamic.Start();
            }

        }
    }
}