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
            Camera.position = new Microsoft.Xna.Framework.Vector3(5, 5, 5);
            InactiveSleepTime = TimeSpan.Zero;


        }
    }
}
