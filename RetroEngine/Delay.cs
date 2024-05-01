using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class Delay
    {
        double waitUntilTime = 0;

        public bool Wait()
        {
            return waitUntilTime >= Time.gameTime;
        }

        public void AddDelay(float delay)
        {
            waitUntilTime = Time.gameTime + delay;
        }

    }
}
