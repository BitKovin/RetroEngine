using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Graphic
{
    public class SurfaceShaderInstance
    {


        public Dictionary<string, float> FloatValues = new Dictionary<string, float>();
        public Dictionary<string, Vector3> Vector3Values = new Dictionary<string, Vector3>();
        public Dictionary<string, Vector4> Vector4Values = new Dictionary<string, Vector4>();
        public Dictionary<string, Vector2> Vector2Values = new Dictionary<string, Vector2>();
        public Dictionary<string, Matrix> MatrixValues = new Dictionary<string, Matrix>();
        public Dictionary<string, Texture> TextureValues = new Dictionary<string, Texture>();

        Effect EffectDefault = AssetRegistry.GetShaderFromName("unlit");
        Effect EffectTransperent = AssetRegistry.GetShaderFromName("unlit");
        Effect EffectInstanced = AssetRegistry.GetShaderFromName("UnifiedOutput.instanced");

        public SurfaceShaderInstance(string name)
        {
            Effect Default = AssetRegistry.GetShaderFromName(name);
            Effect Transperent = AssetRegistry.GetShaderFromName(name+".transperent");
            Effect Instanced = AssetRegistry.GetShaderFromName(name + ".instanced");



            if(Default != null)
                EffectDefault = Default;

            if(Transperent != null)
                EffectTransperent = Transperent;
            else 
                EffectTransperent = EffectDefault;

            if(Instanced != null) 
                EffectInstanced = Instanced;

        }

        public Effect GetAndApply(ShaderSurfaceType surfaceType = ShaderSurfaceType.Default)
        {

            ApplyValues();

            switch (surfaceType)
            {
                case ShaderSurfaceType.Default:
                    return EffectDefault;
                case ShaderSurfaceType.Transperent:
                    return EffectTransperent;

                case ShaderSurfaceType.Instanced:
                    return EffectInstanced;
            }

            return EffectDefault;
        }

        internal void ApplyValues()
        {
            foreach (string key in FloatValues.Keys)
            {
                EffectDefault?.Parameters[key].SetValue(FloatValues[key]);
                EffectTransperent?.Parameters[key].SetValue(FloatValues[key]);
                EffectInstanced?.Parameters[key].SetValue(FloatValues[key]);
            }

            foreach (string key in MatrixValues.Keys)
            {
                EffectDefault?.Parameters[key].SetValue(MatrixValues[key]);
                EffectTransperent?.Parameters[key].SetValue(MatrixValues[key]);
                EffectInstanced?.Parameters[key].SetValue(MatrixValues[key]);
            }

            foreach (string key in TextureValues.Keys)
            {
                EffectDefault?.Parameters[key].SetValue(TextureValues[key]);
                EffectTransperent?.Parameters[key].SetValue(TextureValues[key]);
                EffectInstanced?.Parameters[key].SetValue(TextureValues[key]);
            }

            foreach (string key in Vector3Values.Keys)
            {
                EffectDefault?.Parameters[key].SetValue(Vector3Values[key]);
                EffectTransperent?.Parameters[key].SetValue(Vector3Values[key]);
                EffectInstanced?.Parameters[key].SetValue(Vector3Values[key]);
            }

            foreach (string key in Vector4Values.Keys)
            {
                EffectDefault?.Parameters[key].SetValue(Vector4Values[key]);
                EffectTransperent?.Parameters[key].SetValue(Vector4Values[key]);
                EffectInstanced?.Parameters[key].SetValue(Vector4Values[key]);
            }

            foreach (string key in Vector2Values.Keys)
            { 
                EffectDefault?.Parameters[key].SetValue(Vector2Values[key]);
                EffectTransperent?.Parameters[key].SetValue(Vector2Values[key]);
                EffectInstanced?.Parameters[key].SetValue(Vector2Values[key]);
            }

        }

        public enum ShaderSurfaceType
        {
            Default = 0,
            Transperent = 1,
            Instanced = 2
        }


    }
}
