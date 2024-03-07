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

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime * Speed);
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
