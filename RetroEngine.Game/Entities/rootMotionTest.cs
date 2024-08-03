using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{

    [LevelObject("rootTest")]
    public class rootMotionTest : Entity
    {

        SkeletalMesh mesh = new SkeletalMesh();

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile("Animations/human/run_f_root.fbx");
            mesh.PlayAnimation(0);
            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            mesh.LoadMeshMetaFromFile("models/skeletal_test.fbx");
            mesh.ReloadHitboxes(this);

            meshes.Add(mesh);

        }

        public override void Update()
        {
            base.Update();


            

            mesh.Update(Time.DeltaTime);

            Vector3 rootMotion = mesh.PullRootMotion();

            mesh.AlwaysUpdateVisual = true;

            Position += rootMotion;
            mesh.Position = Position;

            mesh.UpdateHitboxes();

            //Console.WriteLine(rootMotion);



        }

    }
}
