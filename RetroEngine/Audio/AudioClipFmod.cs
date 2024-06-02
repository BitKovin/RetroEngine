using FmodForFoxes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RetroEngine.Audio
{
    public class AudioClipFmod : AudioClip
    {

        Sound sound;

        static ChannelGroup group = new ChannelGroup("general");


        Channel channel = new Channel();


        public AudioClipFmod(Sound _sound) 
        {
            sound = _sound;
            sound.LowPass = 1;
            sound.ChannelGroup = group;
        }

        public override void Update()
        {
            base.Update();

            channel.DopplerLevel3D = 0;

            Apply3D();
        }

        public override void Apply3D()
        {
            base.Apply3D();

            sound.Is3D = true;
            channel.Is3D = true;


            Apply3DData(channel);
            Apply3DData(sound);

            ApplyDistance();
        }

        void ApplyDistance()
        {
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

            sound.Volume = volume;
            channel.Volume = volume;


        }

        void Apply3DData(I3DControl control)
        {
            control.MinDistance3D = MinDistance;
            control.MaxDistance3D = MaxDistance;
            

            control.Velocity3D = Velocity;

            control.Position3D = Position;
        }

        public override void Play(bool fromStart = false)
        {
            base.Play(fromStart);

            Update();

            if (fromStart == false)
            {
                if(channel.Paused)
                {
                    channel.Resume();
                    return;
                }
            }

            channel.Stop();


            

            channel = sound.Play();

        }

        public override void Stop()
        {
            base.Stop();

            channel.Stop();

        }

        protected override void Destroy()
        {
            base.Destroy();

            sound.Dispose();

        }

        public override void Pause()
        {
            base.Pause();

            channel.Pause();

        }



    }
}
