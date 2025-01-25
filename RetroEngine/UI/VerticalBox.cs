using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.UI
{

    public class VerticalBox : ContentBox
    {

        public float ContentDistance = 5;

        public override void Update()
        {

            int i = -1;
            foreach (UiElement child in childs.ToArray())
            {
                i++;

                child.position = new Vector2(0, child.size.X + ContentDistance) * i;

            }

            base.Update();
        }

    }
}
