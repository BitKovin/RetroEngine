using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RetroEngine.Entities;
using RetroEngine.Game.Entities;
using System;
using RetroEngine.MapParser;
using RetroEngine.Map;
using RetroEngine;

namespace RetroEngine.Game
{
    public class Game : RetroEngine.GameMain
    {
        protected override void LoadContent()
        {
            base.LoadContent();
            devMenu = new GameDevMenu();
            devMenu.Init();

            Level.LoadFromFile("test.map");

            for (float i = 1; i < 0; i++)
            {
                BoxDynamic boxDynamic = new BoxDynamic();
                boxDynamic.Position = new Vector3(0, i * 6, (float)Math.Sin(i * 5) * 0.4f);
                curentLevel.entities.Add(boxDynamic);

                boxDynamic.Start();
            }

        }

        public override void OnLevelChanged()
        {
            base.OnLevelChanged();

            GameMain.inst.curentLevel.entities.Add(new PlayerGlobal());

        }

    }
}