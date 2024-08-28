using Microsoft.Xna.Framework;
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

            name = "PlayerGlobal";

        }

        public override void Update()
        {
            base.Update();

            Input.LockCursor = !GameMain.Instance.paused;

            Graphics.LowLatency = Input.GetAction("test3").Holding();

            Vector3 offset = Camera.position + Camera.Forward + Camera.Right * 0.3f + Camera.Up*-0.3f;

            if (Input.GetAction("dev").Pressed())
                GameMain.Instance.DevMenuEnabled = !GameMain.Instance.DevMenuEnabled;

            if (GameMain.Instance.paused) return;

            DrawDebug.Line(offset, offset + Vector3.UnitX/2, Vector3.UnitX, 0.01f);

            DrawDebug.Line(offset, offset + Vector3.UnitY/2, Vector3.UnitY, 0.01f);

            DrawDebug.Line(offset, offset + Vector3.UnitZ/2, Vector3.UnitZ, 0.01f);

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
