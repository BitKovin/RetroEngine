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

        public void Update()
        {

            if(loaded == false) return;

            foreach (var animation in AnimationsToUpdate)
            {
                animation.Update(Time.deltaTime * Speed);
            }
        }

        bool loaded = false;

        public AnimationPose GetResultPose()
        {
            if(loaded)
                return ProcessResultPose();

            return new AnimationPose();
        }

        protected virtual AnimationPose ProcessResultPose()
        {
            return new AnimationPose();
        }


        protected Animation AddAnimation(string path, bool loop = true, int index = 0, bool interpolation = true, bool updatePose = false)
        {

            Animation animation = new Animation();

            
            animation.LoadFromFile(path);
            animation.PlayAnimation(index, loop);
            AnimationsToUpdate.Add(animation);

            animation.UpdateFinalPose = updatePose;
            animation.SetInterpolationEnabled(interpolation);

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
