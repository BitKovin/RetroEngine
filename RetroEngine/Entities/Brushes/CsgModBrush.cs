using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Csg;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assimp.Metadata;

namespace RetroEngine.Entities.Brushes
{
    [LevelObject("csgModBrush")]
    public class CsgModBrush : Entity
    {

        public CsgModBrush() 
        {
            Static = false;
            mergeBrushes = true;
            ConvexBrush = false;
        }

        Solid[] solids;

        List<Solid> subtractSolids = new List<Solid>();

        bool dirty = false;

        void ApplySolids()
        {

            if (dirty == false) return;

            if (solids == null) return;

            int i = -1;

            foreach (var mesh in meshes)
            {
                i++;

                BrushFaceMesh brushFaceMesh = mesh as BrushFaceMesh;
                if (brushFaceMesh == null) continue;

                Solid solid = solids[i];

                if(solid == null) continue;

                brushFaceMesh.ApplySolid(solid.Substract(subtractSolids.ToArray()));

                //solids[i] = brushFaceMesh.ToSolid();

            }

            RegenerateBodies();

        }

        void RegenerateBodies()
        {

            foreach (RigidBody body in bodies)
                Physics.Remove(body);

            foreach (var mesh in meshes)
            {
                BrushFaceMesh face = mesh as BrushFaceMesh;

                if(face == null) continue;

                var shape = Physics.CreateCollisionShapeFromModel(face.model, shapeData: new Physics.CollisionShapeData { surfaceType = face.textureName }, complex: this.ConvexBrush == false);
                RigidBody rigidBody = Physics.CreateFromShape(this, Vector3.One.ToPhysics(), shape, collisionFlags: BulletSharp.CollisionFlags.StaticObject, bodyType: PhysicsSystem.BodyType.World);
                rigidBody.SetCollisionMask(BodyType.GroupAll);
                bodies.Add(rigidBody);
            }
        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

            ApplySolids();

        }

        public override void Start()
        {
            base.Start();
            int i = -1;

            solids = new Solid[meshes.Count];

            Vector3 avgLocation = Vector3.Zero;

            foreach (var mesh in meshes)
            {
                i++;

                BrushFaceMesh brushFaceMesh = mesh as BrushFaceMesh;
                if (brushFaceMesh == null) continue;

                solids[i] = brushFaceMesh.ToSolid();

                avgLocation += brushFaceMesh.avgVertexPosition;

            }

            avgLocation /= (float)(i+1);

            subtractSolids.Add(Solids.Sphere(2, avgLocation.ToCsg()));

            dirty = true;


        }

    }
}
