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

        AudioClip AudioClip;

        public float Volume = 1;

        public float Pitch = 1;

        public float MaxDistance = 10;

        public float MinDistance = 1;

        public override void LateUpdate()
        {
            base.LateUpdate();

            AudioClip.Volume = Volume;
            AudioClip.Pitch = Pitch;
            AudioClip.Position = Position;

            AudioClip.MaxDistance = MaxDistance;
            AudioClip.MinDistance = MinDistance;

            AudioClip.Update();


        }

        public void SetSound(AudioClip clip)
        {
            AudioClip?.Dispose();
            AudioClip = clip;
        }

        public void Play(bool fromStart = false)
        {
            LateUpdate();
            AudioClip.Play(fromStart);
        }

        public void Stop()
        {
            AudioClip.Stop();
        }

        public void Pause()
        {
            AudioClip.Pause();
        }

        public override void Destroy()
        {
            base.Destroy();

            AudioClip?.Dispose();   

        }

    }
}
