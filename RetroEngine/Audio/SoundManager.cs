using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Audio
{
    public static class SoundManager
    {

        static AudioEmitter emitter;

        private static AudioListener listener;

        public static void Init()
        {
            listener = new AudioListener();
        }

        public static void Update()
        {
            listener.Position = Camera.position;
            listener.Up = Camera.rotation.GetUpVector();
            listener.Forward = Camera.rotation.GetForwardVector();
            listener.Velocity = Camera.velocity;
        }

        public static void ApplyPosition(this SoundEffectInstance soundEffectInstance, Vector3 position, float MaxDistance = 10)
        {
            float distance = Vector3.Distance(listener.Position, position);

            float volume = 1f - (distance / MaxDistance);

            

            Vector3 toEmitter =  listener.Position - position;

            toEmitter.Normalize();

            volume /= ((Vector3.Dot(listener.Forward, toEmitter) + 1) / 5f) + 1;

            Vector3 right = Vector3.Cross(listener.Up, listener.Forward);

            right.Normalize();

            float pan = Vector3.Dot(toEmitter, right);

            pan /= 15;

            soundEffectInstance.Volume = Math.Clamp(volume,0,1);
            soundEffectInstance.Pan = Math.Clamp(pan, -1.0f, 1.0f);
            //soundEffectInstance.Apply3D(listener, emitter);
        }

    }
}
