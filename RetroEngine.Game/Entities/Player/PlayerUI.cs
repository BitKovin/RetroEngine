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
        Text fps = new Text();

        bool loaded = false;

        public void Load()
        {
            crosshair.baseColor = new Color(1f, 1f, 1f) * 2f;
            crosshair.SetTexture("ui/crosshair.png");

            crosshair.originH = Origin.CenterH;
            crosshair.originV = Origin.CenterV;
            crosshair.size = new Vector2(4, 4);
            crosshair.position = -crosshair.size/2;
            UiElement.Viewport.childs.Add(crosshair);

            health.originH = Origin.Left;
            health.originV = Origin.Bottom;
            health.position = new Vector2(10,-50);
            health.FontSize = 24;
            health.AlignProgress = new Vector2(0.0f, 0.5f);
            UiElement.Viewport.childs.Add(health);

            fps.originH = Origin.Left;
            fps.originV = Origin.Top;
            fps.position = new Vector2(10, 50);
            fps.FontSize = 24;
            fps.AlignProgress = new Vector2(0.0f, 0.5f);
            UiElement.Viewport.childs.Add(fps);

            loaded = true;
        }

        public void Update()
        {
            if (loaded == false) return;

            health.text = ((Vector3)player.body.LinearVelocity).XZ().Length().ToString();

            fps.text = ((int)(1f / Time.deltaTime)).ToString();

        }

        public void Destroy()
        {

        }

    }
}
