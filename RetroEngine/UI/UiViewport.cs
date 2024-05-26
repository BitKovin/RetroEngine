using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.UI
{
    public class UiViewport : UiElement
    {

        public static float UiScale = 1f;

        public override void Update()
        {

            size = new Vector2 (GetViewportHeight() * Camera.HtW, GetViewportHeight());

            ParrentTopLeft = new Vector2();

            ParrentBottomRight = size;

            base.Update();
        }

        public static float GetViewportHeight()
        {
            return Constants.UI_RESOLUTION / UiScale;
        }

    }
}
