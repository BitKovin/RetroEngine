using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Skeletal
{
    public class ActionAnimation : Animation
    {

        public float BlendIn = 0.1f;
        public float BlendOut = 0.2f;

        public ActionAnimation() 
        {
        }


        public float GetBlendFactor()
        {
            if (RiggedModel == null) return 0;

            float duration = RiggedModel.AnimationDuration;
            float currentTime = RiggedModel.AnimationTime;

            if (RiggedModel.animationRunning == false)
                return 0;

            if(currentTime>=duration)
                return 0;

            if (currentTime < BlendIn)
            {
                // Blend-in phase
                return Math.Clamp(currentTime / BlendIn,0,1);
            }
            else if (currentTime > duration - BlendOut)
            {
                // Blend-out phase
                return Math.Clamp((duration - currentTime) / BlendOut,0,1);
            }
            else
            {
                // Full animation phase
                return 1;
            }
        }

        public override void Update(float deltaTime)
        {

            base.Update(deltaTime);
        }

        public void Play()
        {
            PlayAnimation(RiggedModel.currentAnimation,false,0);
            RiggedModel.Update(0.0001f);
            
        }

        public void Stop()
        {
            RiggedModel.animationRunning = false;
        }

    }
}
