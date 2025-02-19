using Assimp;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static RetroEngine.SkeletalMesh;

namespace RetroEngine.Skeletal
{
    public class Animator
    {

        protected List<Animation> AnimationsToUpdate = new List<Animation>();

        public float Speed = 1;

        public bool InterpolateAnimations = true;

        public bool Simple = false;

        public bool UpdateVisual = true;

        public event AnimationEventPlayed OnAnimationEvent;

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

        protected Animation AddAnimation(string path, bool loop = true, string name = "", bool interpolation = true)
        {

            Animation animation = new Animation();

            animation.Name = path + name;
            
            animation.LoadFromFile(path);

            if(name!= "")
                animation.PlayAnimation(name, interpolation);
            else
                animation.PlayAnimation(0, interpolation);
            

            lock (AnimationsToUpdate)
            {
                AnimationsToUpdate.Add(animation);
            }

            animation.UpdateFinalPose = false;
            animation.SetInterpolationEnabled(interpolation);

            animation.OnAnimationEvent += Animation_OnAnimationEvent;

            return animation;
        }

        protected ActionAnimation AddActionAnimation(string path, string name = "", float BlendIn = 0.2f, float BlendOut = 0.2f, bool interpolation = true)
        {

            ActionAnimation animation = new ActionAnimation();

            animation.Name = path + name;

            animation.LoadFromFile(path);

            if (name != "")
                animation.SetAnimation(name);
            else
                animation.SetAnimation(0);
            

            animation.UpdateFinalPose = false;
            animation.SetInterpolationEnabled(interpolation);

            animation.BlendIn = BlendIn;
            animation.BlendOut = BlendOut;

            lock (AnimationsToUpdate)
            {
                AnimationsToUpdate.Add(animation);
            }

            animation.OnAnimationEvent += Animation_OnAnimationEvent;

            return animation;
        }

        private void Animation_OnAnimationEvent(AnimationEvent animationEvent)
        {
            OnAnimationEvent?.Invoke(animationEvent);
        }

        public void LoadAssets()
        {
            Load();

            loaded = true;
        }
        protected virtual void Load()
        {

        }

        public AnimatorSaveState SaveState()
        {
            return new AnimatorSaveState(this);
        }

        public void LoadState(AnimatorSaveState animatorSaveState)
        {
            var animationStates = animatorSaveState.animationStates;

            int i = -1;
            foreach (Animation animation in AnimationsToUpdate)
            {
                i++;
                if(i < animationStates.Length)
                    animation.SetAnimationState(animationStates[i]);

            }
        }

        public struct AnimatorSaveState
        {
            [JsonInclude]
            public SkeletalMesh.AnimationState[] animationStates;

            public AnimatorSaveState(Animator animator)
            {

                animationStates = new SkeletalMesh.AnimationState[animator.AnimationsToUpdate.Count];

                int i = -1;
                foreach(Animation animation in animator.AnimationsToUpdate)
                {
                    i++;

                    animationStates[i] = animation.GetAnimationState();

                }
            }
        }

    }
}
