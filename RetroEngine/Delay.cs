using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class Delay
    {
        double waitUntilTime = -10000;

        bool ignorePause;

        public Delay() { }
        public Delay(bool ignorePause)
        {
            this.ignorePause = ignorePause;
        }

        public bool Wait()
        {

            double time = Time.GameTime;
            if (ignorePause)
            {
                time = Time.GameTimeNoPause;
            }

            return waitUntilTime >= time;
        }

        public void AddDelay(float delay)
        {

            double time = Time.GameTime;
            if (ignorePause)
            {
                time = Time.GameTimeNoPause;
            }

            waitUntilTime = time + delay;
        }

        public float GetRemainTime()
        {

            double time = Time.GameTime;
            if (ignorePause)
            {
                time = Time.GameTimeNoPause;
            }

            return (float)(waitUntilTime - time);
        }

    }
}
