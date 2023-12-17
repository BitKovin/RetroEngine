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

            Window.Title = "Game";

        }

        public override void GameInitialized()
        {
            //MakeFullscreen();
        }

        public override void OnLevelChanged()
        {
            base.OnLevelChanged();

            GameMain.Instance.curentLevel.entities.Add(new PlayerGlobal());


            for (int i = 1; i <= 0; i++)
            {
                NPCBase npc = new NPCBase();
                npc.Position = new Vector3(0, 10, 0);
                Level.GetCurrent().AddEntity(npc);
                npc.Start();
            }

        }

        void CreateInputActions()
        {
            Input.AddAction("pause").AddKeyboardKey(Keys.Escape);

            Input.AddAction("jump").AddKeyboardKey(Keys.Space).AddButton(Buttons.A);
            Input.AddAction("attack").LMB = true;

            Input.AddAction("moveForward").AddKeyboardKey(Keys.W).AddButton(Buttons.LeftThumbstickUp);
            Input.AddAction("moveBackward").AddKeyboardKey(Keys.S).AddButton(Buttons.LeftThumbstickDown);
            Input.AddAction("moveLeft").AddKeyboardKey(Keys.A).AddButton(Buttons.LeftThumbstickLeft);
            Input.AddAction("moveRight").AddKeyboardKey(Keys.D).AddButton(Buttons.LeftThumbstickRight);

            Input.AddAction("slot1").AddKeyboardKey(Keys.D1);
            Input.AddAction("slot2").AddKeyboardKey(Keys.D2);
            Input.AddAction("slot3").AddKeyboardKey(Keys.D3);

            Input.AddAction("lastSlot").AddKeyboardKey(Keys.Q);

            Input.AddAction("test").AddKeyboardKey(Keys.R);
        }

    }
}