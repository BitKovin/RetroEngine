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

        public AudioClip AudioClip;

        public float Volume = 1;

        public float Pitch = 1;

        public float MaxDistance = 30;

        public float MinDistance = 1;

        public bool Loop = false;

        public bool Paused = false;

        public bool IsUiSound = false;
        public bool Is3DSound = true;

        public SoundPlayer()
        {
            LateUpdateWhilePaused = true;
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (AudioClip == null) return;

            AudioClip.Volume = Volume;
            AudioClip.Pitch = Pitch;
            AudioClip.Position = Position;

            AudioClip.Is3D = Is3DSound;
            AudioClip.IsUISound = IsUiSound;

            AudioClip.Paused = Paused;

            AudioClip.MaxDistance = MaxDistance;
            AudioClip.MinDistance = MinDistance;

            AudioClip.Loop = Loop;


            AudioClip.Update();


        }

        public void SetSound(AudioClip clip)
        {
            AudioClip?.Dispose();
            AudioClip = clip;
        }

        public void Play(bool fromStart = false)
        {
            if (AudioClip == null) return;
            LateUpdate();
            AudioClip.Play(fromStart);
        }

        public void Stop()
        {
            if (AudioClip == null) return;
            AudioClip.Stop();
        }

        public override void Destroy()
        {
            base.Destroy();

            AudioClip?.Stop();
            AudioClip?.Dispose();   

        }

    }
}
