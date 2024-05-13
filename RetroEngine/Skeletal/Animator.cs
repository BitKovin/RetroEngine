using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Skeletal
{
    public class Animator
    {

        protected List<Animation> AnimationsToUpdate = new List<Animation>();

        public float Speed = 1;

        public bool InterpolateAnimations = true;

        public bool Simple = false;

        public bool UpdateVisual = true;

        public void Update()
        {

            if(loaded == false) return;

            lock (AnimationsToUpdate)
            {
                foreach (var animation in AnimationsToUpdate)
                {
                    animation.SetInterpolationEnabled(InterpolateAnimations);
                    animation.UpdateFinalPose = UpdateVisual;
                    animation.Update(Time.DeltaTime * Speed);
                }
            }
        }

        bool loaded = false;

        public AnimationPose GetResultPose()
        {
            if (loaded == false) return new AnimationPose();

            if (Simple)
                return ProcessSimpleResultPose();

            return ProcessResultPose();
        }

        protected virtual AnimationPose ProcessResultPose()
        {
            return new AnimationPose();
        }

        protected virtual AnimationPose ProcessSimpleResultPose()
        {
            return new AnimationPose();
        }

        protected Animation AddAnimation(string path, bool loop = true, int index = 0, bool interpolation = true)
        {

            Animation animation = new Animation();

            
            animation.LoadFromFile(path);
            animation.PlayAnimation(index, loop);
            lock (AnimationsToUpdate)
            {
                AnimationsToUpdate.Add(animation);
            }

            animation.UpdateFinalPose = false;
            animation.SetInterpolationEnabled(interpolation);

            return animation;
        }

        protected ActionAnimation AddActionAnimation(string path, int index = 0, float BlendIn = 0.2f, float BlendOut = 0.2f, bool interpolation = true)
        {

            ActionAnimation animation = new ActionAnimation();


            animation.LoadFromFile(path);
            animation.SetAnimation(index);
            

            animation.UpdateFinalPose = false;
            animation.SetInterpolationEnabled(interpolation);

            animation.BlendIn = BlendIn;
            animation.BlendOut = BlendOut;

            lock (AnimationsToUpdate)
            {
                AnimationsToUpdate.Add(animation);
            }

            return animation;
        }

        public void LoadAssets()
        {
            Load();

            loaded = true;
        }
        protected virtual void Load()
        {

        }

    }
}
