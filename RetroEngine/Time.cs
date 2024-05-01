using System;
using System.Collections.Generic;
using System.Text;

namespace RetroEngine
{
    public static class Time
    {
        public static float DeltaTime = 1;
        public static float TimeScale = 1;

        public static float deltaTimeDifference = 1;

        public static double gameTime = 0;

        static List<float> frames = new List<float>();
        static int framesCount = 0;

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

            DeltaTime = avg;
        }

    }
}
