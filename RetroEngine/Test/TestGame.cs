using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Test
{
    internal class TestGame : GameMain
    {
        public override void GameInitialized()
        {
            base.GameInitialized();

            Level.LoadFromFile("test2.map");
            Input.LockCursor = false;

            InactiveSleepTime = TimeSpan.Zero;

        }
    }
}
