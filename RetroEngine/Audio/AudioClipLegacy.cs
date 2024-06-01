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

        bool paused;

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

            if (pendingPlay && !pendingPlayDelay.Wait())
            {
                soundEffectInstance.Play();
                pendingPlay = false;
            }

            soundEffectInstance.ApplyPosition(Position, MaxDistance, MinDistance, Volume);

            if (Vector3.Distance(Camera.position, Position) > MaxDistance * 2f)
            {
                if (soundEffectInstance.State != SoundState.Stopped)
                {
                    soundEffectInstance.Stop();
                }
                return;
            }
            soundEffectInstance.IsLooped = IsLooped;

            if (paused)
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
            paused = false;
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

        public override void Pause()
        {
            paused = true;
            soundEffectInstance?.Pause();
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

    }
}
