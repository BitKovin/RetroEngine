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

        public void Dispose()
        {
            Destroy();
        }

        protected virtual void Destroy()
        {

        }

        public virtual void Update() { }

        public virtual void Play(bool fromStart = false) { }

        public virtual void Stop() { }

        public virtual void Pause() { }

        public virtual void Apply3D()
        {

        }

    }
}
