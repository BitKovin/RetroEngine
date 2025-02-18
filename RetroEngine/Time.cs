using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RetroEngine
{
    public static class Time
    {
        public static float DeltaTime = 1;
        public static float TimeScale = 1;

        public static float deltaTimeDifference = 1;

        public static double GameTime = 0;
        public static double GameTimeNoPause = 0;

        static List<float> frames = new List<float>();
        static int framesCount = 0;

        public static int FrameCount = 0;

        static List<TimeScaleEffect> timeScaleEffects = new List<TimeScaleEffect>();

        public static void AddFrameTime(float time)
        {
            if(frames.Count>framesCount)
                frames.RemoveAt(0);

            frames.Add(time);

            float avg = 0;

            foreach(float frame in frames)
            {
                avg += frame;
            }

            avg /= (float)frames.Count;

            deltaTimeDifference = Math.Abs(avg - DeltaTime);

            DeltaTime = avg*GetFinalTimeScale();
            FrameCount++;

            lock (timeScaleEffects)
            {
                foreach (TimeScaleEffect timeScaleEffect in timeScaleEffects.ToArray())
                {
                    if (timeScaleEffect.DurationDelay.Wait() == false)
                    {
                        timeScaleEffects.Remove(timeScaleEffect);
                    }
                }
            }

        }

        public static float GetFinalTimeScale()
        {

            float time = TimeScale;

            foreach (TimeScaleEffect timeScaleEffect in timeScaleEffects.ToArray())
            {
                if (timeScaleEffect.DurationDelay.Wait())
                {
                    time *= timeScaleEffect.TimeScale;
                }
                else
                {
                    
                }
            }

            return time;
        }

        public static float GetSoundFinalTimeScale()
        {

            float time = TimeScale;

            foreach (TimeScaleEffect timeScaleEffect in timeScaleEffects.ToArray())
            {

                if (timeScaleEffect.AffectSound == false) continue;

                if (timeScaleEffect.DurationDelay.Wait())
                {
                    time *= timeScaleEffect.TimeScale;
                }
                else
                {
                    
                }
            }

            return time;

        }

        [ConsoleCommand("time.scale")]
        public static void SetTimeScale(string value)
        {

            if (float.TryParse(value.Replace(" ",""), CultureInfo.InvariantCulture, out float val) == false)
            {
                Logger.Log("wrong formating: " + value);
                return;
            }


            TimeScale = val;
        }

        public static void AddTimeScaleEffect(TimeScaleEffect effect)
        {
            lock (timeScaleEffects)
                timeScaleEffects.Add(effect);
        }

    }

    public class TimeScaleEffect
    {

        public Delay DurationDelay = new Delay(true);
        public bool AffectSound = false;
        public float TimeScale = 1.0f;

        public TimeScaleEffect(float duration = 0.2f, float timeScale = 0.0f, bool affectSound = false)
        {
            DurationDelay.AddDelay(duration);
            TimeScale = timeScale;
            AffectSound = affectSound;

        }
    }

}
