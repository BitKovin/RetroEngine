using RetroEngine;
using RetroEngine.Skeletal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkeletalMeshEditor
{
    internal class SkeletalMeshPreview : Entity
    {

        public SkeletalMesh skeletalMesh = new SkeletalMesh();

        public static Animation Animation;

        public static SkeletalMeshPreview instance;

        public SkeletalMeshPreview()
        {
            meshes.Add(skeletalMesh);
            instance =  this;
        }

        public override void Update()
        {
            base.Update();

            skeletalMesh.Update(Time.DeltaTime);

            skeletalMesh.UpdatePose = true;

            if(Animation!=null)
            {
                Animation.Update(Time.DeltaTime);
                skeletalMesh.PastePoseLocal(Animation.GetPoseLocal());
            }

            skeletalMesh.UpdateHitboxes();

            skeletalMesh.CopyRagdollToBodies();

        }

        public void CreateRagdoll()
        {
            skeletalMesh.CreateRagdollBodies(this);
        }

        public void StopRagdoll()
        {
            skeletalMesh.ClearRagdollBodies();
        }


    }
}
