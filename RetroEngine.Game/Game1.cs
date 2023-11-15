using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RetroEngine.Entities;
using RetroEngine.Game.Entities;
using System;
using RetroEngine.MapParser;
using RetroEngine.Map;
using RetroEngine;
using RetroEngine.Entities.Navigaion;

namespace RetroEngine.Game
{
    public class Game : RetroEngine.GameMain
    {
        protected override void LoadContent()
        {
            

            base.LoadContent();
            Input.CenterCursor();
            CreateInputActions();
            devMenu = new GameDevMenu();
            devMenu.Init();

            Level.LoadFromFile("test2.map");

            for (float i = 1; i < 0; i++)
            {
                BoxDynamic boxDynamic = new BoxDynamic();
                boxDynamic.Position = new Vector3(0, i * 6, (float)Math.Sin(i * 5) * 0.4f);
                curentLevel.entities.Add(boxDynamic);

                boxDynamic.Start();
            }

            Window.Title = "Game";

        }

        public override void GameInitialized()
        {
            //MakeFullscreen();
        }

        public override void OnLevelChanged()
        {
            base.OnLevelChanged();

            GameMain.inst.curentLevel.entities.Add(new PlayerGlobal());

            GameMain.inst.curentLevel.entities.Add(new NavDebuger());

            foreach (NavPoint point in Navigation.GetNavPoints())
            {
                int n = point.connected.Count;

                for (int i = 0;  i < n; i++)
                {
                    //Level.GetCurrent().AddEntity(new Box { Position = point.Position + new Vector3(0, i, 0) });
                }

            }

        }

        void CreateInputActions()
        {
            Input.AddAction("pause").AddKeyboardKey(Keys.Escape);

            Input.AddAction("jump").AddKeyboardKey(Keys.Space);
            Input.AddAction("attack").LMB = true;

            Input.AddAction("moveForward").AddKeyboardKey(Keys.W).AddButton(Buttons.DPadUp);
            Input.AddAction("moveBackward").AddKeyboardKey(Keys.S).AddButton(Buttons.DPadDown);
            Input.AddAction("moveLeft").AddKeyboardKey(Keys.A).AddButton(Buttons.DPadLeft);
            Input.AddAction("moveRight").AddKeyboardKey(Keys.D).AddButton(Buttons.DPadRight);

            Input.AddAction("test").AddKeyboardKey(Keys.R);
            Input.AddAction("test2").AddKeyboardKey(Keys.T);
        }

    }
}