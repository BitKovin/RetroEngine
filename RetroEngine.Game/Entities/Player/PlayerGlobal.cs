using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RetroEngine.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Player
{
    public class PlayerGlobal : Entity
    {

        FmodEventInstance pauseEvent;

        [JsonInclude]
        public float InAction = 0;

        public static PlayerGlobal Instance;

        public PlayerGlobal()
        {
            UpdateWhilePaused = true;
            LateUpdateWhilePaused = true;

            pauseEvent = FmodEventInstance.Create("snapshot:/Pause");

            name = "PlayerGlobal";

            SaveGame = true;
            SaveAsUnique = true;

        }

        public override void Start()
        {
            base.Start();

            Instance = this;

        }

        public override void Update()
        {
            base.Update();

            Instance = this;

            Input.LockCursor = !GameMain.Instance.paused;

            Graphics.LowLatency = Input.GetAction("test3").Holding();

            Vector3 offset = Camera.position + Camera.Forward + Camera.Right * 0.5f + Camera.Up*-0.5f;

            if (Input.GetAction("dev").Pressed())
                GameMain.Instance.DevMenuEnabled = !GameMain.Instance.DevMenuEnabled;

            if (GameMain.Instance.paused) return;

            DrawDebug.Line(offset, offset + Vector3.UnitX/6, Vector3.UnitX, Time.DeltaTime*1.5f);

            DrawDebug.Line(offset, offset + Vector3.UnitY/6, Vector3.UnitY, Time.DeltaTime * 1.5f);

            DrawDebug.Line(offset, offset + Vector3.UnitZ/6, Vector3.UnitZ, Time.DeltaTime * 1.5f);

            StudioSystem.SetParameterValue("parameter:/inAction", InAction);

            //testSpawnTime();

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

        Entity spawnedEntity;

        void testSpawnTime()
        {

            spawnedEntity?.Destroy();

            var ent = LevelObjectFactory.CreateByTechnicalName("npc_humanAxe");

            if (ent != null)
            {
                ent.Position = Position;
                ent.Rotation = Rotation;
                ent.SetOwner(this);

                ent.Start();

                Level.GetCurrent().AddEntity(ent);

                spawnedEntity = ent;

            }
        }

    }
}
