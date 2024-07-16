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

            size = GetSize();

            ParrentTopLeft = new Vector2();

            ParrentBottomRight = size;

            base.Update();
        }

        public override Vector2 GetSize()
        {
            return new Vector2(GetViewportHeight() * Camera.HtW, GetViewportHeight());
        }

        public static float GetViewportHeight()
        {
            return Constants.UI_RESOLUTION / UiScale;
        }

    }
}
