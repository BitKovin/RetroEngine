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

        public static Dictionary<string, Matrix> LerpPose(Dictionary<string, Matrix> a, Dictionary<string, Matrix> b, float factor)
        {

            Dictionary<string, Matrix> result = new Dictionary<string, Matrix>();

            foreach (var key in a.Keys)
            {
                if (b.ContainsKey(key) == false) continue;

                MathHelper.Transform transformA = MathHelper.DecomposeMatrix(a[key]);

                MathHelper.Transform transformB = MathHelper.DecomposeMatrix(b[key]);


                result.Add(key, MathHelper.Transform.Lerp(transformA,transformB, factor).ToMatrix());

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

            if (LoadedRigModels.ContainsKey(path))
            {
                RiggedModel = LoadedRigModels[path].MakeCopy();
            }
            else
            {
                RiggedModel = modelReader.LoadAsset(path, 30);


                LoadedRigModels.Add(path, RiggedModel);
            }

            RiggedModel = LoadedRigModels[path].MakeCopy();


            RiggedModel.Update(0);

            CalculateBoundingSphere();

            GetBoneMatrix("");

            additionalLocalOffsets = RiggedModel.additionalLocalOffsets;

            additionalMeshOffsets = RiggedModel.additionalMeshOffsets;

            RiggedModel.overrideAnimationFrameTime = -1;
        }


        public override void DrawUnified()
        {
        }

        public override void UpdateCulling()
        {
        }

        public override void DrawDepth()
        {
        }

    }
}
