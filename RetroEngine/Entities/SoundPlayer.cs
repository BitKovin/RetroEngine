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

        protected float fade = 0;
        protected float fadeSpeed = 1;

        public SoundPlayer()
        {
            LateUpdateWhilePaused = true;
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (AudioClip == null) return;

            fade += fadeSpeed*Time.DeltaTime;
            fade = Math.Clamp(fade, 0, 1);

            AudioClip.Volume = GetVolume();
            AudioClip.Pitch = Pitch;
            AudioClip.Position = Position;

            AudioClip.Is3D = Is3DSound;
            AudioClip.IsUISound = IsUiSound;

            AudioClip.Paused = Paused;

            AudioClip.MaxDistance = Math.Max(MaxDistance, 0.2f);
            AudioClip.MinDistance = Math.Max(MinDistance,0.2f);

            AudioClip.Loop = Loop;


            AudioClip.Update();


        }

        public void SetSound(AudioClip clip)
        {
            //AudioClip?.Dispose();
            AudioClip?.Stop();
            AudioClip = clip;
        }

        public virtual void Play(bool fromStart = false)
        {
            if (AudioClip == null) return;
            LateUpdate();
            AudioClip.Play(fromStart);
            fade = 1;
        }

        public virtual void PlayWithFade(bool fromStart = false, float fadeTime = 1)
        {
            fadeSpeed = 1/fadeTime;
            LateUpdate();
            AudioClip.Play(fromStart);
        }

        public virtual void StopWithFade(float fadeTime = 1)
        {
            fadeSpeed = -1 / fadeTime;
        }

        public virtual void Stop()
        {
            if (AudioClip == null) return;
            AudioClip.Stop();
            fade = 0;
        }

        public static void PlayAtLocation(AudioClip clip, Vector3 location, float Volume = 1, float Distance = 20, bool isUI = false, float duration = 2)
        {
            SoundPlayer soundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;

            soundPlayer.Position = location;

            soundPlayer.Volume = Volume;

            soundPlayer.Is3DSound = true;
            soundPlayer.IsUiSound = isUI;
            soundPlayer.MaxDistance = Distance;

            soundPlayer.SetSound(clip);
            soundPlayer.Play();
            soundPlayer.Destroy(duration);

        }

        public static void Play2D(AudioClip clip, bool isUI = false)
        {
            SoundPlayer soundPlayer = Level.GetCurrent().AddEntity(new SoundPlayer()) as SoundPlayer;

            soundPlayer.IsUiSound = isUI;

            soundPlayer.SetSound(clip);

            soundPlayer.Play();

            soundPlayer.Destroy(clip.GetDuration() + 0.1f);

        }

        public override void Destroy()
        {
            base.Destroy();

            Stop();

            AudioClip?.Stop();
            AudioClip?.Dispose();   


        }

        protected virtual float GetVolume()
        {
            return Volume * fade * GetGlobalVolume();
        }

        public static float GetGlobalVolume()
        {
            return SoundManager.Volume;
        }

        public virtual void SetEventProperty(string name, float value)
        {
            FmodEventInstance fmodEventInstance = AudioClip as FmodEventInstance;

            if(fmodEventInstance == null) return;

            fmodEventInstance.SetParameter(name, value);

        }

        public static void PlaySound(AudioClip audioClip, float volume = 1)
        {
            audioClip.Volume = volume * GetGlobalVolume();
            audioClip.Is3D = false;
            audioClip.Loop = false;
            audioClip.Position = Camera.position;
            audioClip.Play();
            audioClip.Update();

        }

    }
}
