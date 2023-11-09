using RetroEngine;
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

        public static void ApplyPosition(this SoundEffectInstance soundEffectInstance, Vector3 position, float MaxDistance = 10, float MinDistance = 2)
        {
            float distance = Vector3.Distance(listener.Position, position);



            float n = 2.5f;

            distance -= MinDistance;

            distance = Math.Max(distance, 0);

            MaxDistance -= MinDistance;

            float x = (distance / MaxDistance);

            // Calculate the attenuation factor based on the inverse square law
            float attenuation = (1f-x) / ((x*8 + (1/n))*n);

            // Set the volume and pitch based on attenuation
            float maxVolume = 1.0f; // Adjust this value for maximum volume
            float minVolume = 0.0f; // Adjust this value for minimum volume
            float volume = minVolume + (maxVolume - minVolume) * attenuation;


            Vector3 toEmitter =  listener.Position - position;

            toEmitter.Normalize();

            volume /= ((Vector3.Dot(listener.Forward, toEmitter) + 1) / 4f) + 1;

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
