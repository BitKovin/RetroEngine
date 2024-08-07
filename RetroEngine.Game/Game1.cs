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
using RetroEngine.Graphic;
using RetroEngine.Game.Effects;
using RetroEngine.Audio;

namespace RetroEngine.Game
{
    public class Game : RetroEngine.GameMain
    {

        static FmodEventInstance GameSpeedEvent;

        protected override void LoadContent()
        {
            base.LoadContent();

            //Input.CenterCursor();
            CreateInputActions();
            devMenu = new GameDevMenu();
            devMenu.Init();


            Window.Title = "Game";

        }

        public override void GameInitialized()
        {
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.strings.bank");


            GameSpeedEvent = FmodEventInstance.Create("snapshot:/GameSpeed");
            GameSpeedEvent.Play();

            Level.LoadFromFile("recastTest");

            base.GameInitialized();

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

            DefaultShader = AssetRegistry.GetShaderFromName("SimpleEffect");

            base.Update(gameTime);

            if (NavigationSystem.Recast.dtNavMesh != null && GameMain.Instance.paused == false)
            {
                NavigationSystem.RecastDebugDraw.DebugDrawNavMeshPolys(NavigationSystem.Recast.dtNavMesh);
            }

            if (Input.GetAction("fullscreen").Pressed())
                ToggleFullscreen();
        }


        public override void OnLevelChanged()
        {

            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.strings.bank");


            NavigationSystem.Recast.LoadSampleNavMesh();


            //PostProcessStep.StepsAfter.Add(new TestPP());

            //Render.LUT = AssetRegistry.LoadTextureFromFile("engine/textures/Sin Shitty.png_out.png", generateMipMaps: false);   

            for (int i = 1; i <= 0; i++)
            {
                Entity npc = new NPCBase();
                npc.Position = new Vector3(0, i*2.2f + 2, 0);
                npc.Start();
                Level.GetCurrent().AddEntity(npc);
            }
            base.OnLevelChanged();
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

            Input.AddAction("slot0").AddKeyboardKey(Keys.LeftAlt);
            Input.AddAction("slot1").AddKeyboardKey(Keys.D1);
            Input.AddAction("slot2").AddKeyboardKey(Keys.D2);
            Input.AddAction("slot3").AddKeyboardKey(Keys.D3);

            Input.AddAction("lastSlot").AddKeyboardKey(Keys.Q);

            Input.AddAction("view").AddKeyboardKey(Keys.B);

            Input.AddAction("test").AddKeyboardKey(Keys.R);
            Input.AddAction("test2").AddKeyboardKey(Keys.E);

            Input.AddAction("qSave").AddKeyboardKey(Keys.F5);
            Input.AddAction("qLoad").AddKeyboardKey(Keys.F8);

            Input.AddAction("fullscreen").AddKeyboardKey(Keys.F10);
        }

    }
}