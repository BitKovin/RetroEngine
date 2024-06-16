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

namespace RetroEngine.Audio
{
    public static class SoundManager
    {

        static AudioEmitter emitter;

        internal static AudioListener listener;

        static Listener3D listener3D;

        public static INativeFmodLibrary nativeFmodLibrary;

        public static bool UseFmod = true;

        public static void Init()
        {
            listener = new AudioListener();

            if (UseFmod == false) return;

            FmodManager.Init(nativeFmodLibrary, FmodInitMode.CoreAndStudio, AssetRegistry.ROOT_PATH);
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
            
            FmodManager.Update();
            listener3D.SetAttributes(SoundManager.listener.Position, Camera.velocity, SoundManager.listener.Forward, -SoundManager.listener.Up);

        }

    }
}
