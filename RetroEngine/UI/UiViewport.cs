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

        public override void Update()
        {

            size = new Vector2 (Constants.ResolutionY * Camera.HtW, Constants.ResolutionY);

            ParrentTopLeft = new Vector2();

            ParrentBottomRight = size;

            base.Update();
        }

    }
}
