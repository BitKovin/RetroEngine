using Engine;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class Utils
    {

        public static Texture2D LoadTextureFromFile(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    return Texture2D.FromStream(GameMain.inst.GraphicsDevice, stream);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during texture loading
                Console.WriteLine("Failed to load texture: " + ex.Message);
                return null;
            }

        }

    }
}
