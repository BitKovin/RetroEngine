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


            Window.Title = "Game";

        }

        public override void GameInitialized()
        {
            //MakeFullscreen();


            Level.LoadFromFile("test2.map");


        }

        protected override void Update(GameTime gameTime)
        {

            if (Graphics.DefaultUnlit)
            {
                DefaultShader = AssetRegistry.GetShaderFromName("Unlit");
            }
            else
            {
                DefaultShader = null;
            }

            base.Update(gameTime);
        }

        public override void OnLevelChanged()
        {
            base.OnLevelChanged();

            Level.GetCurrent().AddEntity(new PlayerGlobal());

            Level.GetCurrent().AddEntity(new Sky());


            for (int i = 1; i <= 200; i++)
            {
                Entity npc = new NPCBase();
                npc.Position = new Vector3(0, i*10.2f + 2, 0);
                Level.GetCurrent().AddEntity(npc);
            }

        }

        void CreateInputActions()
        {
            Input.AddAction("pause").AddKeyboardKey(Keys.Escape);

            Input.AddAction("jump").AddKeyboardKey(Keys.Space).AddButton(Buttons.A);
            Input.AddAction("attack").AddButton(Buttons.RightTrigger).LMB = true;
            Input.AddAction("attack2").AddButton(Buttons.LeftTrigger).RMB = true;

            Input.AddAction("moveForward").AddKeyboardKey(Keys.W).AddButton(Buttons.LeftThumbstickUp);
            Input.AddAction("moveBackward").AddKeyboardKey(Keys.S).AddButton(Buttons.LeftThumbstickDown);
            Input.AddAction("moveLeft").AddKeyboardKey(Keys.A).AddButton(Buttons.LeftThumbstickLeft);
            Input.AddAction("moveRight").AddKeyboardKey(Keys.D).AddButton(Buttons.LeftThumbstickRight);

            Input.AddAction("slot1").AddKeyboardKey(Keys.D1);
            Input.AddAction("slot2").AddKeyboardKey(Keys.D2);
            Input.AddAction("slot3").AddKeyboardKey(Keys.D3);

            Input.AddAction("lastSlot").AddKeyboardKey(Keys.Q);

            Input.AddAction("test").AddKeyboardKey(Keys.R);
            Input.AddAction("test2").AddKeyboardKey(Keys.E);
        }

    }
}