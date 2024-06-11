using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Audio
{
    internal class AudioClipLegacy : AudioClip
    {
        

        SoundEffectInstance soundEffectInstance;
        SoundEffect _sound;


        bool _looped;

        bool playing = false;

        public bool IsLooped { get { return _looped; } set { if (soundEffectInstance != null) { soundEffectInstance.IsLooped = value; } _looped = value; } }


        bool pendingPlay = false;
        Delay pendingPlayDelay = new Delay();

        internal AudioClipLegacy(SoundEffect soundEffect)
        {
            SetSound(soundEffect);
        }


        public override void Update()
        {
            if (soundEffectInstance == null) return;

            soundEffectInstance.Pitch = 1 - Pitch;

            soundEffectInstance.IsLooped = Loop;

            if (pendingPlay && !pendingPlayDelay.Wait())
            {
                soundEffectInstance.Play();
                pendingPlay = false;
            }

            Apply3D();

            if (Vector3.Distance(Camera.position, Position) > (MaxDistance+5 * 2f))
            {
                if (soundEffectInstance.State != SoundState.Stopped)
                {
                    soundEffectInstance.Stop();
                }
                return;
            }
            soundEffectInstance.IsLooped = IsLooped;

            if (isPaused())
            {
                soundEffectInstance.Pause();
            }
            else if (playing)
            {
                if (IsLooped)
                {
                    try
                    {
                        soundEffectInstance.Play();
                    }
                    catch (Exception ex) { Console.WriteLine(ex); }
                }


            }
        }

        internal void SetSound(SoundEffect sound)
        {
            if (sound != null)
            {

                soundEffectInstance = sound.CreateInstance();
                soundEffectInstance.IsLooped = IsLooped;
                _sound = sound;
            }
        }

        public override void Play(bool fromStart = false)
        {
            Paused = false;
            playing = true;

            Update();



            if (fromStart)
            {
                soundEffectInstance?.Stop(true);
            }
            try
            {
                SetSound(_sound);
                Update();
                pendingPlay = true;
            }
            catch (Exception ex) { Console.WriteLine(ex); }
        }

        public override void Stop()
        {
            soundEffectInstance?.Stop(true);
            playing = false;
        }

        protected override void Destroy()
        {
            base.Destroy();

            soundEffectInstance?.Stop(true);
            soundEffectInstance?.Dispose();
            soundEffectInstance = null;
            _sound = null;
            GC.SuppressFinalize(this);

        }

        public override void Apply3D()
        {
            if (soundEffectInstance is null) return;

            float distance = Vector3.Distance(SoundManager.listener.Position, Position);

            float n = 2.5f;

            distance -= MinDistance;

            distance = Math.Max(distance, 0f);

            MaxDistance -= MinDistance;

            float x = (distance / MaxDistance);

            // Calculate the attenuation factor based on the inverse square law
            float attenuation = (1f - x) / ((x * 8 + (1 / n)) * n);

            // Set the volume and pitch based on attenuation
            float maxVolume = 1.0f; // Adjust this value for maximum volume
            float minVolume = 0.0f; // Adjust this value for minimum volume
            float volume = minVolume + (maxVolume - minVolume) * attenuation * Volume;


            Vector3 toEmitter = SoundManager.listener.Position - Position;

            toEmitter.Normalize();

            if (distance > 0.2f)
                volume /= ((Vector3.Dot(SoundManager.listener.Forward, toEmitter) + 1) / 4f) + 1;

            Vector3 right = Vector3.Cross(SoundManager.listener.Up, SoundManager.listener.Forward);

            right.Normalize();

            float pan = Vector3.Dot(toEmitter, right);

            pan /= 1.333f;

            pan = Math.Clamp(pan, -1.0f, 1.0f);

            if (float.IsNaN(pan))
                pan = 0f;

            if(float.IsNaN(volume))
                volume = 0f;

            soundEffectInstance.Volume = Math.Clamp(volume, 0, 1);
            soundEffectInstance.Pan = pan;
        }

    }
}
