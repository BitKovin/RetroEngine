﻿using Microsoft.Xna.Framework;
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
using RetroEngine.Entities.Light;
using RetroEngine.Game.Entities.Enemies;
using RetroEngine.Localization;
using RetroEngine.NavigationSystem;

namespace RetroEngine.Game
{
    public class Game : RetroEngine.GameMain
    {

        static FmodEventInstance GameSpeedEvent;

        public Game() 
        {

            SetStylingGraphics();

        }

        protected override void LoadContent()
        {
            base.LoadContent();

            //Input.CenterCursor();
            CreateInputActions();
            devMenu = new GameDevMenu();
            devMenu.Init();

            LocalizationManager.LoadLocalizationProfile("ua");

            Window.Title = "Game";

        }

        public override void GameInitialized()
        {
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.strings.bank");


            GameSpeedEvent = FmodEventInstance.Create("snapshot:/GameSpeed");
            GameSpeedEvent.Play();




                
            Level.LoadFromFile("test");

            base.GameInitialized();

        }

        protected override void Update(GameTime gameTime)
        {



            if (Graphics.DefaultUnlit)
            {
                //DefaultShader = "Unlit";
            }
            else
            {
                //DefaultShader = null;
            }

            base.Update(gameTime);


            if (Input.GetAction("fullscreen").Pressed())
                ToggleBorderless();
        }


        public override void OnLevelChanged()
        {

            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.strings.bank");

            ParticleSystemEnt.Preload("hitBlood");

            //PostProcessStep.StepsAfter.Add(new TestPP());

            //Render.LUT = AssetRegistry.LoadTextureFromFile("engine/textures/Sin Shitty.png_out.png", generateMipMaps: false);   

            for (int i = 1; i <= 0; i++)
            {
                Entity npc = new npc_dog();
                npc.Position = new Vector3(i, i*2f + 4, 0);
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

            Input.AddAction("run").AddKeyboardKey(Keys.LeftShift).AddButton(Buttons.B);

            Input.AddAction("slot0").AddKeyboardKey(Keys.LeftAlt);
            Input.AddAction("slot1").AddKeyboardKey(Keys.D1);
            Input.AddAction("slot2").AddKeyboardKey(Keys.D2);
            Input.AddAction("slot3").AddKeyboardKey(Keys.D3);
            Input.AddAction("slot4").AddKeyboardKey(Keys.D4);
            Input.AddAction("slot5").AddKeyboardKey(Keys.D5);
            Input.AddAction("slot6").AddKeyboardKey(Keys.D6);

            Input.AddAction("slotMelee").AddKeyboardKey(Keys.F).AddButton(Buttons.RightShoulder);

            Input.AddAction("lastSlot").AddKeyboardKey(Keys.Q);

            Input.AddAction("view").AddKeyboardKey(Keys.B);

            Input.AddAction("test").AddKeyboardKey(Keys.R);
            Input.AddAction("test2").AddKeyboardKey(Keys.E);
            Input.AddAction("test3").AddKeyboardKey(Keys.F);

            Input.AddAction("qSave").AddKeyboardKey(Keys.F5);
            Input.AddAction("qLoad").AddKeyboardKey(Keys.F8);

            Input.AddAction("fullscreen").AddKeyboardKey(Keys.F10);

            Input.AddAction("dev").AddKeyboardKey(Keys.OemTilde);

        }

        void SetStylingGraphics()
        {
            Graphics.EnableSSR = false;
            Graphics.EnableSSAO = false;
            Graphics.PointLightShadowQuality = 0;
            Graphics.DirectionalLightShadowQuality = 0;
            Graphics.ShadowResolutionScale = 0.3f;

            Render.SimpleRender = true;

            //Render.LimitedColorSpace = true;

            PointLight.DisableShadows = true;


            //Graphics.GeometricalShadowsEnabled = true;

            //Graphics.Exposure = 1;
            //Graphics.Gamma = 1.1f;

            Graphics.Saturation = 1.0f;


            Graphics.EnableBloom = true;

            Render.DisableMultiPass = false;

            DefaultShader = "VertexLighting";

        }

    }
}