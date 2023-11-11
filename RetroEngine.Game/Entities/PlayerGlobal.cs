using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{
    public class PlayerGlobal : Entity
    {

        public PlayerGlobal() 
        { 
            UpdateWhilePaused = true;
        }

        public override void Update()
        {
            base.Update();

            Input.LockCursor = !GameMain.inst.paused;

            if (Input.pressedKeys.Contains(Keys.R))
            {
                Level.LoadFromFile("maps/test.map");
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            if (Input.pressedKeys.Contains(Keys.Escape))
            {
                GameMain.inst.paused = !GameMain.inst.paused;
            }

        }

    }
}
