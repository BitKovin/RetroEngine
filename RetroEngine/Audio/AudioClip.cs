using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Audio
{
    public class AudioClip : IDisposable
    {

        public Vector3 Position;

        public float MaxDistance = 10;
        public float MinDistance = 1;

        public float Volume = 1;
        public float Pitch = 1;

        public Vector3 Velocity;

        public bool Is3D = true;

        public bool Loop = false;

        public bool ApplyDistanceVolume = true;

        public bool Paused = false;

        public bool IsUISound = false;

        public void Dispose()
        {
            Destroy();
        }

        protected virtual bool isPaused()
        {

            if(IsUISound)
            {
                return Paused;
            }

            return Paused || GameMain.Instance.paused;
        }
        protected virtual void Destroy()
        {
            Stop();
        }

        public virtual void Update() { }

        public virtual void Play(bool fromStart = true) { }

        public virtual void Stop() { }

        public virtual float GetPlaybackPosition()
        {
            return 0;
        }


        public virtual float GetDuration()
        {
            return 0;
        }
        public virtual void SetPlaybackPosition(float position)
        {

        }

        public virtual bool isPlaying()
        {
            return false;
        }

        public virtual void Apply3D()
        {

        }

    }
}
