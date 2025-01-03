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

            //mesh.LoadFromFile("Animations/human/run_f_root.fbx");
            
            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            mesh.LoadMeshMetaFromFile("models/enemies/dog.fbx");
            mesh.ReloadHitboxes(this);

            mesh.PlayAnimation("attack");

            meshes.Add(mesh);

        }

        public override void Update()
        {
            base.Update();


            

            mesh.Update(Time.DeltaTime);

            var rootMotion = mesh.PullRootMotion();

            mesh.AlwaysUpdateVisual = true;

            Position += rootMotion.Position;
            //Rotation += rootMotion.Rotation;
            mesh.Position = Position;
            mesh.Rotation = Rotation;

            DrawDebug.Sphere(0.3f, Position, Vector3.Zero, 0.01f);
            DrawDebug.Line(Position + Vector3.UnitY, Position + Vector3.UnitY + Rotation.GetForwardVector(), Vector3.UnitY, 0.01f);

            mesh.UpdateHitboxes();

            //Console.WriteLine(rootMotion);



        }

    }
}
