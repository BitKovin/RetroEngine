using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Skeletal
{
    public class Animation : SkeletalMesh
    {

        public float Speed = 1;

        public bool UpdateFinalPose { get { return RiggedModel.UpdateVisual; } set { if(RiggedModel!=null) RiggedModel.UpdateVisual = value; } }


        protected static Dictionary<string, RiggedModel> LoadedRigModelsAnimations = new Dictionary<string, RiggedModel>();

        public static AnimationPose LerpPose(AnimationPose poseA, AnimationPose poseB, float factor)
        {

            var a = poseA.Pose;
            var b = poseB.Pose;

            if(factor<0.005)
                return poseA;
            if(factor>0.995)
                return poseB;

            AnimationPose result = new AnimationPose();
            result.Pose= new Dictionary<string, Matrix>(a);

            foreach (var key in a.Keys)
            {
                if (b.ContainsKey(key) == false) continue;


                MathHelper.Transform transformA = MathHelper.DecomposeMatrix(a[key]);
                MathHelper.Transform transformB = MathHelper.DecomposeMatrix(b[key]);


                result.Pose[key] = MathHelper.Transform.Lerp(transformA,transformB, factor).ToMatrix();

                //result.Pose[key] = Matrix.Lerp(a[key], b[key], factor);

            }

            foreach (var key in result.BoneOverrides.Keys)
            {
                var bone = result.BoneOverrides[key];

                bone.progress = bone.progress - factor;

                result.BoneOverrides[key] = bone;

            }

            return result;
        }

        public Animation() 
        {
            modelReader.OnlyAnimation = true;
        }
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime * Speed);
        }


        public override void LoadFromFile(string filePath)
        {

            string path = AssetRegistry.FindPathForFile(filePath);

            if (LoadedRigModelsAnimations.ContainsKey(path))
            {
                RiggedModel = LoadedRigModelsAnimations[path].MakeCopy();
            }
            else
            {
                RiggedModel = modelReader.LoadAsset(path, 30);


                LoadedRigModelsAnimations.Add(path, RiggedModel);
            }

            RiggedModel = LoadedRigModelsAnimations[path].MakeCopy();


            RiggedModel.Update(0);


            additionalLocalOffsets = RiggedModel.additionalLocalOffsets;

            additionalMeshOffsets = RiggedModel.additionalMeshOffsets;

            RiggedModel.overrideAnimationFrameTime = -1;

            LoadMeshMetaFromFile(path);

        }

        public override AnimationPose GetPoseLocal()
        {
            var pose = base.GetPoseLocal();
            pose.BoneOverrides = new Dictionary<string, BonePoseBlend>();
            return pose;
        }


        public override void DrawUnified()
        {
        }

        public override void UpdateCulling()
        {
        }

        public override void DrawDepth(bool pointLight = false, bool renderTransperent = false)
        {
        }

    }
}
