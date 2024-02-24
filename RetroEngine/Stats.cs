using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class Stats
    {

        static Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();

        static Dictionary<string, float> results = new Dictionary<string, float>();

        public static bool Enabled = true;


        public static int RenderedMehses = 0;

        public static void StartRecord(string name)
        {
            if (!Enabled) return;

            if (stopwatches.ContainsKey(name))
            {
                stopwatches[name] = Stopwatch.StartNew();
            }
            else
            {
                stopwatches.Add(name, Stopwatch.StartNew());
            }
        }

        public static void StopRecord(string name)
        {
            if (!Enabled) return;
            if (stopwatches.ContainsKey(name) == false) return;

            if (results.ContainsKey(name) == false)
                results.Add(name, (float)stopwatches[name].Elapsed.TotalMilliseconds * 1000);
            else
                results[name] = (float)stopwatches[name].Elapsed.TotalMilliseconds * 1000;

        }

        public static Dictionary<string, float> GetResults()
        {
            if (!Enabled) return new Dictionary<string, float>();
            return new Dictionary<string, float>(results);
        }

    }
}
