using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.UI
{

    public class HorizontalBox : ContentBox
    {

        public float ContentDistance = 5;

        public override void Update()
        {

            int i = -1;
            foreach (UiElement child in childs.ToArray())
            {
                i++;

                child.position = new Vector2(child.size.X + ContentDistance, 0) * i;

            }

            base.Update();
        }

    }
}
