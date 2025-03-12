using CppNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RetroEngine.Graphic.PostProcessStep;

namespace RetroEngine.Graphic
{
    public class PostProcessStep
    {

        public static List<PostProcessStep> StepsBefore = new List<PostProcessStep>();
        public static List<PostProcessStep> StepsAfter = new List<PostProcessStep>();

        public Shader Shader;

        public Dictionary<string, float> FloatValues = new Dictionary<string, float>();
        public Dictionary<string, Matrix> MatrixValues = new Dictionary<string, Matrix>();
        public Dictionary<string, Texture> TextureValues = new Dictionary<string, Texture>();
        public Dictionary<string, Vector3> Vector3Values = new Dictionary<string, Vector3>();
        public Dictionary<string, Vector4> Vector4Values = new Dictionary<string, Vector4>();
        public Dictionary<string, Vector2> Vector2Values = new Dictionary<string, Vector2>();

        internal RenderTarget2D RenderTarget;

        internal Texture2D BackBuffer;

        public delegate void BeforePerform();
        public event BeforePerform OnBeforePerform;

        internal void Perform()
        {
            if(Shader == null)
            {
                Logger.Log("Warining: " + this.ToString()+" Shader == null!");
                return;
            }

            GameMain.Instance.render.UpdateDataForShader(Shader);

            OnBeforePerform?.Invoke();

            RenderStep();

        }

        void RenderStep()
        {
            GraphicsDevice graphicsDevice = GameMain.Instance.GraphicsDevice;

            graphicsDevice.SetRenderTarget(RenderTarget);

            graphicsDevice.Clear(Color.Pink);

            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            SetValues();

            spriteBatch.Begin(blendState: BlendState.Opaque, effect: Shader);
            Render.DrawFullScreenQuad(spriteBatch, BackBuffer);
            spriteBatch.End();

        }

        void SetValues()
        {

            Shader.Parameters["Color"]?.SetValue(BackBuffer);

            Shader.Parameters["DepthTexture"]?.SetValue(GameMain.Instance.render.DepthPrepathOutput);

            Shader.Parameters["PositionTexture"]?.SetValue(GameMain.Instance.render.positionPath);

            Shader.Parameters["NormalTexture"]?.SetValue(GameMain.Instance.render.normalPath);

            Shader.FloatValues = FloatValues;
            Shader.MatrixValues = MatrixValues;
            Shader.TextureValues = TextureValues;
            Shader.Vector2Values = Vector2Values;
            Shader.Vector3Values = Vector3Values;
            Shader.Vector4Values = Vector4Values;

            Shader.ApplyValues();

        }


    }
}
