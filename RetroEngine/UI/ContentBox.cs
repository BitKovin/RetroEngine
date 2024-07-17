using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.UI
{
    public class ContentBox : UiElement
    {

        public override Vector2 GetSize()
        {
            if (childs == null || childs.Count == 0)
            {
                return Vector2.Zero;
            }

            Vector2 topLeft = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 bottomRight = new Vector2(float.MinValue, float.MinValue);

            foreach (UiElement elem in childs)
            {
                if (elem.TopLeft.X < topLeft.X)
                    topLeft.X = elem.TopLeft.X;

                if (elem.TopLeft.Y < topLeft.Y)
                    topLeft.Y = elem.TopLeft.Y;

                if (elem.BottomRight.X > bottomRight.X)
                    bottomRight.X = elem.BottomRight.X;

                if (elem.BottomRight.Y > bottomRight.Y)
                    bottomRight.Y = elem.BottomRight.Y;
            }

            return bottomRight - topLeft;
        }

    }
}
