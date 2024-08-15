using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Graphic
{
    public static class TextureHelper
    {

        static Dictionary<Texture2D, IntPtr> registeredPointers = new Dictionary<Texture2D, IntPtr>();

        public static IntPtr GetImGuiPointer(this Texture2D texture)
        {
            if(registeredPointers.ContainsKey(texture))
                return registeredPointers[texture];

            var ptr = GameMain.Instance.ImGuiRenderer.BindTexture(texture);

            registeredPointers.Add(texture, ptr);

            return ptr;

        }

    }
}
