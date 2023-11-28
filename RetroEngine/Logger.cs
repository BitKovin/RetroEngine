using RetroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class Logger
    {

        public static void Log(string s)
        {
            Console.WriteLine(s);
            if(GameMain.inst.devMenu is not null)
                GameMain.inst.devMenu.log.Add(s);
        }

    }
}
