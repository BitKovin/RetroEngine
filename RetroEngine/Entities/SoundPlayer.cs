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

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (Vector3.Distance(Camera.position, Position) > MaxDistance * 1.1f)
            {
                soundEffectInstance.Pause();
                return;
            }

            if (paused) 
            {
                soundEffectInstance.Pause();
            }
            else if(playing)
            {
                soundEffectInstance.Play();
            

                soundEffectInstance.ApplyPosition(Position, MaxDistance, MinDistance);

                soundEffectInstance.Volume *= Volume;

            }
        }

        public void SetSound(SoundEffect sound)
        {
            if (sound != null)
            {
                soundEffectInstance = sound.CreateInstance();
                soundEffectInstance.IsLooped = IsLooped;
            }
        }

        public void Play()
        {
            soundEffectInstance.Play();
            paused = false;
            playing = true;
        }

        public void Stop()
        {
            soundEffectInstance.Stop();
            playing = false;
        }

        public void Pause()
        {
            paused = true;
        }

        public override void Destroy()
        {
            base.Destroy();

            soundEffectInstance.Stop(true);

        }

    }
}
