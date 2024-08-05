using RetroEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FmodForFoxes;
using System.Runtime.InteropServices;
using FmodForFoxes.Studio;

namespace RetroEngine.Audio
{
    public static class SoundManager
    {

        static AudioEmitter emitter;

        internal static AudioListener listener;

        static Listener3D listener3D;

        public static INativeFmodLibrary nativeFmodLibrary;

        public static bool UseFmod = true;

        public static float Volume = 0.2f;


        public static void Init()
        {
            listener = new AudioListener();

            if (UseFmod == false) return;

            

#if DEBUG
            FmodManager.Init(nativeFmodLibrary, FmodInitMode.CoreAndStudio, AssetRegistry.ROOT_PATH, studioInitFlags: FMOD.Studio.INITFLAGS.NORMAL | FMOD.Studio.INITFLAGS.LIVEUPDATE);
#else
            FmodManager.Init(nativeFmodLibrary, FmodInitMode.CoreAndStudio, AssetRegistry.ROOT_PATH, studioInitFlags: FMOD.Studio.INITFLAGS.NORMAL);
#endif

            listener3D = new Listener3D();
        }

        public static void Shutdown() 
        {
            FmodManager.Unload();
        }

        public static void Update()
        {
            listener.Position = Camera.position;
            listener.Up = Camera.rotation.GetUpVector();
            listener.Forward = Camera.rotation.GetForwardVector();
            listener.Velocity = Camera.velocity;


            if (UseFmod == false) return;


            StudioSystem.SetParameterValue("parameter:/GameSpeed", Time.TimeScale);

            FmodManager.Update();
            listener3D.SetAttributes(SoundManager.listener.Position, Camera.velocity, SoundManager.listener.Forward, -SoundManager.listener.Up);

        }

        [ConsoleCommand("volume")]
        public static void SetVolume(float value)
        {
            Volume = value;
        }

    }
}
