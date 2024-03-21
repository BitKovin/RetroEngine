using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Graphic
{
    public class Shader : Effect
    {


        public Dictionary<string, float> FloatValues = new Dictionary<string, float>();
        public Dictionary<string, Matrix> MatrixValues = new Dictionary<string, Matrix>();
        public Dictionary<string, Texture> TextureValues = new Dictionary<string, Texture>();

        public Shader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        public Shader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }

        public Shader(Effect cloneSource) : base(cloneSource)
        {
        }

        internal void ApplyValues()
        {
            foreach (string key in FloatValues.Keys)
            {
                Parameters[key].SetValue(FloatValues[key]);
            }

            foreach (string key in MatrixValues.Keys)
            {
                Parameters[key].SetValue(MatrixValues[key]);
            }

            foreach (string key in TextureValues.Keys)
            {
                Parameters[key].SetValue(TextureValues[key]);
            }
        }

    }
}
