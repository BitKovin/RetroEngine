using Assimp;
using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Csg;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

        Solid[] pendingApplySolids;

        bool pendingAppy = false;

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        void ApplySolids()
        {

            if (dirty == false) return;
            dirty = false;

            if(workingTask != null && workingTask.Status == TaskStatus.Running)
            {
                tokenSource.Cancel();
            }

            if(tokenSource.TryReset() == false)
            {
                tokenSource.Dispose();
                tokenSource = new CancellationTokenSource();
            }

            CancellationToken ct = tokenSource.Token;

            workingTask = Task.Run(StartGeneratingNewSolids, ct);

        }

        void StartGeneratingNewSolids()
        {
            int i = -1;

            pendingAppy = false;

            pendingApplySolids = new Solid[solids.Length];

            foreach (var mesh in meshes)
            {
                i++;

                BrushFaceMesh brushFaceMesh = mesh as BrushFaceMesh;
                if (brushFaceMesh == null) continue;

                Solid solid = solids[i];

                if (solid == null) continue;



                pendingApplySolids[i] = CsgHelper.GetConnectedPartWithUV(solid.Substract(subtractSolids.ToArray()), originalVertices.ToArray());

            }
            pendingAppy = true;
            subtractSolids.Clear(); //since new one is default one there is no need to store changes

        }

        Task workingTask = null;

        void ApplyPendingSolids()
        {

            if (pendingAppy == false) return;

            int i = -1;

            foreach (var mesh in meshes)
            {
                i++;

                BrushFaceMesh brushFaceMesh = mesh as BrushFaceMesh;
                if (brushFaceMesh == null) continue;

                Solid solid = pendingApplySolids[i];

                if(solid == null) continue;

                brushFaceMesh.ApplySolid(solid);
                solids[i] = solid; //making new one as default one


            }

            RegenerateBodies();

            pendingAppy = false;

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
            ApplyPendingSolids();

        }

        public override void OnPointDamage(float damage, Vector3 point, Vector3 direction, Entity causer = null, Entity weapon = null)
        {
            base.OnPointDamage(damage, point, direction, causer, weapon);

            subtractSolids.Add(Solids.Sphere(0.2f, point.ToCsg()));
            dirty = true;

        }

        List<Vector3> originalVertices = new List<Vector3>();

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

                originalVertices.AddRange(brushFaceMesh.GetMeshVertices());

            }

            avgLocation /= (float)(i+1);

            //subtractSolids.Add(Solids.Sphere(2, avgLocation.ToCsg()));

            //dirty = true;


        }

    }
}
