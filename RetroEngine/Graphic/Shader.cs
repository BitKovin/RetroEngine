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
        public Dictionary<string, Vector3> Vector3Values = new Dictionary<string, Vector3>();
        public Dictionary<string, Vector4> Vector4Values = new Dictionary<string, Vector4>();
        public Dictionary<string, Vector2> Vector2Values = new Dictionary<string, Vector2>();
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

            foreach (string key in Vector3Values.Keys)
            {
                Parameters[key].SetValue(Vector3Values[key]);
            }

            foreach (string key in Vector4Values.Keys)
            {
                Parameters[key].SetValue(Vector4Values[key]);
            }

            foreach (string key in Vector2Values.Keys)
            {
                Parameters[key].SetValue(Vector2Values[key]);
            }

        }

    }
}
