using Microsoft.Xna.Framework.Input;
using RetroEngine.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Player
{
    public class PlayerGlobal : Entity
    {

        FmodEventInstance pauseEvent;

        public PlayerGlobal()
        {
            UpdateWhilePaused = true;
            LateUpdateWhilePaused = true;

            pauseEvent = FmodEventInstance.Create("snapshot:/Pause");

        }

        public override void Update()
        {
            base.Update();

            Input.LockCursor = !GameMain.Instance.paused;

        }

        public override void Destroy()
        {
            base.Destroy();

            pauseEvent?.Stop();
            pauseEvent?.Dispose();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();


            if (Input.GetAction("pause").Pressed())
            {
                GameMain.Instance.paused = !GameMain.Instance.paused;

                if(GameMain.Instance.paused)
                {
                    pauseEvent.StartEvent();
                }else
                {
                    pauseEvent.Stop();
                }

            }
            if (GameMain.Instance.paused == false)
            {
                if (Input.GetAction("qSave").Pressed())
                {
                    SaveSystem.SaveManager.SaveGame();

                }

                if (Input.GetAction("qLoad").Pressed())
                {
                    SaveSystem.SaveManager.LoadGameFromFile("save.sav");

                }
            }

        }

    }
}
