using Microsoft.Xna.Framework;
using RetroEngine.Skeletal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Player
{
    internal class PlayerBodyAnimator : Animator
    {

        Animation idleAnimation;

        Animation runFAnimation = new Animation();
        Animation runBAnimation = new Animation();
        Animation runLAnimation = new Animation();
        Animation runRAnimation = new Animation();

        public float MovementSpeed = 0;

        public Vector2 MovementDirection = Vector2.Zero;

        protected override void Load()
        {
            base.Load();

            idleAnimation = AddAnimation("Animations/human/idle.fbx");

            runFAnimation = AddAnimation("Animations/human/run_f.fbx", interpolation: true);
            runBAnimation = AddAnimation("Animations/human/run_b.fbx", interpolation: false);
            runRAnimation = AddAnimation("Animations/human/run_r.fbx", interpolation: true);
            runLAnimation = AddAnimation("Animations/human/run_l.fbx", interpolation: true);

        }

        protected override AnimationPose ProcessResultPose()
        {

            if(MovementDirection.Y<-0.3)
            {
                Speed = -1;
                MovementDirection *= -1;
            }else
            {
                Speed = 1;
            }

            float blendFactor = MovementSpeed / 5;
            blendFactor = Math.Clamp(blendFactor, 0, 1);

            var idlePose = idleAnimation.GetPoseLocal();

            var locomotionPose = Animation.LerpPose(idlePose, CalculateMovementDirection(), blendFactor);

            locomotionPose.LayeredBlend(idleAnimation.GetBoneByName("spine_01"), idlePose, 0.2f);
            locomotionPose.LayeredBlend(idleAnimation.GetBoneByName("spine_02"), idlePose, 0.2f);
            locomotionPose.LayeredBlend(idleAnimation.GetBoneByName("spine_03"), idlePose, 1f);

            return locomotionPose;

        }

        AnimationPose CalculateMovementDirection()
        {

            float fwdBlend = MovementDirection.Y / 2f + 0.5f;
            float horBlend = MovementDirection.X / 2f + 0.5f;

            

            var forwardPose = Animation.LerpPose(runBAnimation.GetPoseLocal(), runFAnimation.GetPoseLocal(), fwdBlend);

            var horizonalPose = Animation.LerpPose(runLAnimation.GetPoseLocal(), runRAnimation.GetPoseLocal(), horBlend);

            float fwdBlendFactor = Math.Abs(MovementDirection.Y);

            fwdBlendFactor *= MathHelper.Lerp(1, 0.5f, Math.Abs(MovementDirection.X));

            return Animation.LerpPose(horizonalPose, forwardPose, fwdBlendFactor);
        }

    }
}
