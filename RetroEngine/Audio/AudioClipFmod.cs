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

        int ticks = 0;

        Channel channel = new Channel();

        public AudioClipFmod(Sound _sound) 
        {
            sound = _sound;
            sound.LowPass = 1;
            sound.Volume = 0;
        }

        public override void Update()
        {
            base.Update();

            channel.Pitch = Pitch;

            channel.Paused = isPaused();

            Apply3D();
            ticks++;
        }

        public override void Apply3D()
        {
            base.Apply3D();

            
            channel.Is3D = Is3D;

            
            channel.Pitch = Pitch;

            
            channel.Loops = Loop ? -1 : 0;

            Apply3DData(channel);
            
            if(ticks>2)
            ApplyDistance();
        }
        void ApplyStartSoundData()
        {
            sound.Volume = 0;
            sound.Pitch = Pitch;
            sound.Loops = Loop ? -1 : 0;
            sound.Volume = 0;
        }

        public override float GetDuration()
        {
            if(sound == null) return 0;

            sound.LengthTimeunit = FMOD.TIMEUNIT.MS;

            return ((float)sound.Length) / 1000.0f;

        }

        void ApplyDistance()
        {

            if (ApplyDistanceVolume == false)
                return;

            float distance = Vector3.Distance(SoundManager.listener.Position, Position);

            float n = 2.5f;

            distance -= MinDistance;

            distance = Math.Max(distance, 0f);

            float maxDistance = MaxDistance;
            maxDistance -= MinDistance;

            float x = (distance / maxDistance);


            // Calculate the attenuation factor based on the inverse square law
            float attenuation = (1f - x) / ((x * 6 + (1 / n)) * n);

            // Set the volume and pitch based on attenuation
            float maxVolume = 1.0f; // Adjust this value for maximum volume
            float minVolume = 0.0f; // Adjust this value for minimum volume
            float volume = minVolume + (maxVolume - minVolume) * attenuation * Volume;

            if (float.IsNaN(volume) || float.IsInfinity(volume))
                volume = 0;

            channel.Volume = volume;


        }

        void Apply3DData(I3DControl control)
        {
            control.MinDistance3D = MinDistance+1;
            control.MaxDistance3D = MaxDistance+1;
            

            control.Velocity3D = Velocity;

            control.Position3D = Position;
        }

        public override void Play(bool fromStart = true)
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

            Stop();

            ApplyStartSoundData();
            channel = sound.Play();
            channel.VolumeRamp = false;
            channel.Volume = 0;
            Update();
        }



        public override void Stop()
        {
            base.Stop();

            channel.Stop();
            channel.Volume = 0;
            channel.Mute = true;
            channel.Native.getNumDSPs(out int num);
            for (int i = 0; i < num; i++)
            {
                channel.Native.getDSP(i, out var dsp);
                dsp.reset();
            }

        }

        protected override void Destroy()
        {
            base.Destroy();

        }



    }
}
