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
            LateUpdateWhilePaused = true;
        }

        public override void Update()
        {
            base.Update();

            Input.LockCursor = !GameMain.Instance.paused;

        }

        public override void LateUpdate()
        {
            base.LateUpdate();


            Console.WriteLine(Graphics.EnableAntiAliasing);

            if (Input.GetAction("pause").Pressed())
            {
                GameMain.Instance.paused = !GameMain.Instance.paused;

            }

        }

    }
}
