﻿using CppNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Graphic
{
    public class PostProcessStep
    {

        public static List<PostProcessStep> StepsBefore = new List<PostProcessStep>();
        public static List<PostProcessStep> StepsAfter = new List<PostProcessStep>();

        public Effect Shader;

        public Dictionary<string, float> FloatValues = new Dictionary<string, float>();
        public Dictionary<string, Matrix> MatrixValues = new Dictionary<string, Matrix>();
        public Dictionary<string, Texture> TextureValues = new Dictionary<string, Texture>();

        internal RenderTarget2D RenderTarget;

        internal Texture2D BackBuffer;

        internal void Perform()
        {
            if(Shader == null)
            {
                Logger.Log("Warining: " + this.ToString()+" Shader == null!");
                return;
            }

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

            Shader.Parameters["DepthTexture"]?.SetValue(GameMain.Instance.render.DepthOutput);

            Shader.Parameters["NormalTexture"]?.SetValue(GameMain.Instance.render.normalPath);

            foreach (string key in FloatValues.Keys)
            {
                Shader.Parameters[key].SetValue(FloatValues[key]);
            }

            foreach (string key in MatrixValues.Keys)
            {
                Shader.Parameters[key].SetValue(MatrixValues[key]);
            }

            foreach (string key in TextureValues.Keys)
            {
                Shader.Parameters[key].SetValue(TextureValues[key]);
            }
        }


    }
}