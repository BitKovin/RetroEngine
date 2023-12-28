using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class Helper
    {
        public static Texture2D CopyTexture(this Texture2D src, GraphicsDevice graphics, Point size)
        {
            Texture2D tex = new Texture2D(graphics, size.X, size.Y);
            int count = size.X * size.Y;
            Color[] data = new Color[count];
            src.GetData(data, 0, count);
            tex.SetData(data);
            return tex;
        }
    }
}
