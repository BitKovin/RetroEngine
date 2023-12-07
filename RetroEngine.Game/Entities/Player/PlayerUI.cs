using Microsoft.Xna.Framework;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Player
{
    internal class PlayerUI
    {

        PlayerCharacter player;

        public PlayerUI(PlayerCharacter plr) { player = plr; }

        Image crosshair = new Image();
        Text health = new Text();


        bool loaded = false;

        public void Load()
        {
            crosshair.baseColor = new Color(0.9f, 0.8f, 0.6f) * 0.6f;
            crosshair.SetTexture("ui/crosshair.png");

            crosshair.originH = Origin.CenterH;
            crosshair.originV = Origin.CenterV;
            crosshair.position = new Vector2(-4);
            crosshair.size = new Vector2(8, 8);
            UiElement.main.childs.Add(crosshair);

            health.originH = Origin.Left;
            health.originV = Origin.Bottom;
            health.position = new Vector2(10,-50);
            health.size = new Vector2(2);
            UiElement.main.childs.Add(health);

            loaded = true;
        }

        public void Update()
        {
            if (loaded == false) return;

            health.text = ((Vector3)player.body.LinearVelocity).XZ().Length().ToString();

        }

        public void Destroy()
        {

        }

    }
}
