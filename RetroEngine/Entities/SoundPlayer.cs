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

        public bool IsLooped { get { return _looped; } set { if (soundEffectInstance != null) { soundEffectInstance.IsLooped = value; } _looped = value; } }

        public SoundPlayer() { }

        public override void LateUpdate()
        {
            base.LateUpdate();

            soundEffectInstance.ApplyPosition(Position, MaxDistance, MinDistance);

            soundEffectInstance.Volume *= Volume;

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
        }

        public void Stop()
        {
            soundEffectInstance.Stop();
        }

        public void Pause()
        {
            soundEffectInstance.Pause();
        }

        public override void Destroy()
        {
            base.Destroy();

            soundEffectInstance.Stop(true);

        }

    }
}
