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