﻿using RetroEngine.Audio;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("music_player")]
    public class MusicPlayer : SoundPlayer
    {


        string bankName = "Music.bank";
        string eventName = "";
        string soundFileName = "";

        float fadeTime;

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            bankName = data.GetPropertyString("bankName",bankName);
            eventName = data.GetPropertyString("eventName", eventName);
            soundFileName = data.GetPropertyString("soundFileName", soundFileName);

            Volume = data.GetPropertyFloat("volume", 1f);

            fadeTime = data.GetPropertyFloat("fadeTime", 1);

            LoadAssetsIfNeeded();

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/" + bankName);

            if (eventName.Length > 2)
            {
                var fmodEvent = FmodEventInstance.Create(eventName);
                SetSound(fmodEvent);
            }

            if (soundFileName.Length > 2)
            {
                var soundFile = AssetRegistry.LoadSoundFromFile(soundFileName);
                SetSound(soundFile);
            }

        }

        public MusicPlayer() 
        {
            Is3DSound = false;
            IsUiSound = true;   
        }

        public override void LateUpdate()
        {

            MinDistance = 100;
            MaxDistance = 10000;

            Position = Camera.position;

            base.LateUpdate();
        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            if(action == "play")
            {
                PlayWithFade(false,fadeTime);
            }

            if(action == "stop")
            {
                StopWithFade(fadeTime);
            }

            if (action.ToLower().StartsWith("SetEventProperty".ToLower()))
            {
                var parts = action.Split(' ');

                try
                {
                    float value = float.Parse(parts[2]);

                    SetEventProperty(parts[1], value);


                }
                catch(Exception e) { Logger.Log($"ERROR: wrong format for setEvent in {GetType().Name}"); }

            }


        }

    }
}