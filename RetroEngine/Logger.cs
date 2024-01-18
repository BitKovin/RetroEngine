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

        public static void Log(object s)
        {
            Console.WriteLine(s);
            if(GameMain.Instance.devMenu is not null)
                GameMain.Instance.devMenu.Log(s==null? "null" : s.ToString());
        }

    }
}
