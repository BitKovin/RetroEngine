using RetroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkeletalMeshEditor
{
    internal class SkeletalMeshPreview : Entity
    {

        public static SkeletalMesh skeletalMesh = new SkeletalMesh();

        public static SkeletalMeshPreview instance;

        public SkeletalMeshPreview()
        {
            meshes.Add(skeletalMesh);
            instance= this;
        }

        public override void Start()
        {
            base.Start();

            meshes.Add(skeletalMesh);

        }

        public override void Update()
        {
            base.Update();

            skeletalMesh.Update(Time.DeltaTime);
            skeletalMesh.UpdateHitboxes();
            skeletalMesh.UpdatePose = true;
        }


    }
}
