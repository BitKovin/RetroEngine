using BulletSharp.SoftBody;
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

        Animation pistolIdle = new Animation();

        ActionAnimation actionAnimation = new ActionAnimation();

        public float MovementSpeed = 0;

        public Vector2 MovementDirection = Vector2.Zero;

        SkeletalMesh proxy = new SkeletalMesh();

        public void FireAction()
        {
            actionAnimation.Play();
        }

        protected override void Load()
        {
            base.Load();

            idleAnimation = AddAnimation("Animations/human/idle.fbx");

            runFAnimation = AddAnimation("Animations/human/run_f.fbx", interpolation: true);
            //runBAnimation = AddAnimation("Animations/human/run_b.fbx", interpolation: false);
            //runRAnimation = AddAnimation("Animations/human/run_r.fbx", interpolation: true);
            //runLAnimation = AddAnimation("Animations/human/run_l.fbx", interpolation: true);



            proxy.LoadFromFile("Animations/human/rest.fbx");

            pistolIdle = AddAnimation("models/weapons/pistol2.fbx", interpolation: false, loop: false);

            actionAnimation = AddActionAnimation("models/weapons/pistol2.fbx", interpolation: true, BlendIn: 0);

        }


        protected override AnimationPose ProcessResultPose()
        {

            float speedFactor = MovementSpeed/5.5f;

            speedFactor = MathF.Min(speedFactor, 1.2f);

            if (MovementDirection.Y<-0.1)
            {
                Speed = -speedFactor;
                MovementDirection *= -1;
            }else
            {
                Speed = speedFactor;
            }


            float blendFactor = MovementSpeed / 3;
            blendFactor = Math.Clamp(blendFactor, 0, 1);

            var idlePose = idleAnimation.GetPoseLocal();

            var locomotionPose = Animation.LerpPose(idlePose, CalculateMovementDirection(), blendFactor);

            return locomotionPose;

            //locomotionPose = Animation.LerpPose(locomotionPose, actionAnimation.GetPoseLocal(), actionAnimation.GetBlendFactor());


            proxy.SetBoneMeshTransformModification("spine_03", Matrix.Identity);


            proxy.SetBoneMeshTransformModification("spine_03", GetSpineTransforms());

            proxy.UpdateAnimationPose();

            //locomotionPose.LayeredBlend(idleAnimation.GetBoneByName("spine_01"), idlePose, 0.2f);
            //locomotionPose.LayeredBlend(idleAnimation.GetBoneByName("spine_02"), idlePose, 1f);

            //var weaponPose = Animation.LerpPose(pistolIdle.GetPoseLocal(), actionAnimation.GetPoseLocal(), actionAnimation.GetBlendFactor());

            //locomotionPose.LayeredBlend(idleAnimation.GetBoneByName("spine_03"), weaponPose, 1);



            return proxy.GetPoseLocal();

        }

        public bool CanPlayStepSound()
        {

            int frame = runFAnimation.GetCurrentAnimationFrame();

            bool can = (frame == 6) || (frame == 17);

            if (can)
                Console.WriteLine(frame);

            return can;
        }

        public static Matrix GetSpineTransforms()
        {

            return Matrix.Identity;

            MathHelper.Transform transform = new MathHelper.Transform();

            transform.Rotation.X = Camera.rotation.X;

            if (Camera.rotation.X > 0)
            {
                transform.Position.Y = Camera.rotation.X / 3;
                transform.Position.Z = Camera.rotation.X / -15;
            }
            else
            {
                transform.Position.Y = MathF.Abs(Camera.rotation.X) / 10;
                transform.Position.Z = MathF.Abs(Camera.rotation.X) / 6;
            }
            return transform.ToMatrix();

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
