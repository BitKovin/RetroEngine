using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using RetroEngine.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    public class SoundPlayer : Entity, IDisposable
    {
        SoundEffectInstance soundEffectInstance;

        public float MaxDistance = 10;
        public float MinDistance = 1;

        public float Volume = 1;

        bool _looped;

        bool playing = false;

        public bool IsLooped { get { return _looped; } set { if (soundEffectInstance != null) { soundEffectInstance.IsLooped = value; } _looped = value; } }

        bool paused;

        public SoundPlayer() { }

        bool pendingPlay = false;
        Delay pendingPlayDelay = new Delay();

        SoundEffect _sound;

        public override void LateUpdate()
        {
            base.LateUpdate();

            if(soundEffectInstance == null) return;

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
            else if(playing)
            {
                if (IsLooped)
                {
                    try
                    {
                        soundEffectInstance.Play();
                    }
                    catch (Exception) { }
                }
                

            }


        }

        public void SetSound(SoundEffect sound)
        {
            if (sound != null)
            {
                soundEffectInstance = sound.CreateInstance();
                soundEffectInstance.IsLooped = IsLooped;
                _sound = sound;
            }
        }

        public void Play(bool fromStart = false)
        {
            paused = false;
            playing = true;

            LateUpdate();

            

            if(fromStart)
            {
                soundEffectInstance?.Stop(true);
            }
            try
            {
                SetSound(_sound);
                LateUpdate();
                pendingPlay = true;
            }
            catch (Exception) { }
        }

        public void Stop()
        {
            soundEffectInstance?.Stop(true);
            playing = false;
        }

        public void Pause()
        {
            paused = true;
            soundEffectInstance?.Pause();
        }

        public override void Destroy()
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
