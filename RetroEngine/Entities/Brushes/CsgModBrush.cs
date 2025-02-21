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
            mergeBrushes = false; //holes will happen otherwise
            ConvexBrush = false;
        }

        List<Solid> solids;

        List<Solid> subtractSolids = new List<Solid>();

        bool dirty = false;

        Solid[] pendingApplySolids;
        List<(Solid solid, BrushFaceMesh original)> pendingCreateSolid = new List<(Solid solid, BrushFaceMesh original)> ();

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

            pendingApplySolids = new Solid[solids.Count()];
            lock (pendingApplySolids)
            {

                pendingAppy = false;

                List<Solid> solidsToRemove = subtractSolids.ToList();



                for (int j = 0; j < pendingApplySolids.Count(); j++)
                {
                    Solid solid = solids[j];

                    pendingApplySolids[j] = solid.Substract(subtractSolids.ToArray());

                }
                int i = -1;
                foreach (var mesh in meshes)
                {
                    i++;

                    BrushFaceMesh brushFaceMesh = mesh as BrushFaceMesh;
                    if (brushFaceMesh == null) continue;

                    Solid solid = pendingApplySolids[i];

                    if (solid == null) continue;

                    if (solid.isVisual == false)
                    {
                        var result = CsgHelper.GetConnectedAndDisconnectedPartsWithUV(solid, GetLocationsOfSolidsExcept(solid, originalSolids).ToArray());

                        pendingApplySolids[i] = result.MainSolid;



                        foreach (var s in result.DisconnectedSolids)
                        {
                            pendingCreateSolid.Add((s, brushFaceMesh));
                        }
                    }
                    else
                    {
                        //pendingApplySolids[i] = solid.Substract(subtractSolids.ToArray());
                    }


                }

                lock (subtractSolids)
                {
                    foreach (var s in solidsToRemove)
                    {
                        subtractSolids.Remove(s);
                    }

                }

                pendingAppy = true;

                if (subtractSolids.Count != 0)
                    dirty = true;

            }

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

                if (solid == null) continue;

                brushFaceMesh.ApplySolid(solid);
                solids[i] = solid; //making new one as default one
            }

            lock (pendingCreateSolid)
            {
                foreach (var data in pendingCreateSolid.ToArray())
                {

                    Solid solid = data.solid;
                    BrushFaceMesh originalMesh = data.original;

                    BrushFaceMesh newMesh = new BrushFaceMesh(originalMesh.model, originalMesh.texture, originalMesh.textureName);

                    newMesh.Transperent = originalMesh.Transperent;
                    newMesh.Masked = originalMesh.Masked;


                    solids.Add(solid);
                    meshes.Add(newMesh);
                    newMesh.ApplySolid(solid);

                    newMesh.Transperent = true;
                    newMesh.Transparency = 0.5f;

                }

                pendingCreateSolid.Clear();
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

                var shape = Physics.CreateCollisionShapeFromModel(face.model, shapeData: new Physics.CollisionSurfaceData { surfaceType = face.textureName }, complex: this.ConvexBrush == false);
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

        public override void OnPointDamage(float damage, Vector3 point, Vector3 direction, string hitBone = "", Entity causer = null, Entity weapon = null)
        {
            base.OnPointDamage(damage, point, direction, hitBone, causer, weapon);

            subtractSolids.Add(Solids.Sphere(1, point.ToCsg()));
            dirty = true;

        }

        List<Vector3> originalVertices = new List<Vector3>();

        List<Vector3> GetLocationsOfSolidsExcept(Solid except, List<Solid> Solids)
        {

            List<Vector3> locations = new List<Vector3>();

            foreach(Solid solid in Solids)
            {
                if(solid == except) continue;

                foreach(var p in solid.Polygons)
                    foreach(var v in p.Vertices)
                    {
                        locations.Add(new Vector3((float)v.Pos.X, (float)v.Pos.Y, (float)v.Pos.Z));
                    }

            }

            return locations;

        }

        List<Solid> originalSolids;

        public override void Start()
        {
            base.Start();
            int i = -1;

            solids = new List<Solid>(meshes.Count);

            Vector3 avgLocation = Vector3.Zero;

            foreach (var mesh in meshes)
            {
                i++;

                BrushFaceMesh brushFaceMesh = mesh as BrushFaceMesh;
                if (brushFaceMesh == null) continue;

                solids.Add(brushFaceMesh.ToSolid());

                avgLocation += brushFaceMesh.avgVertexPosition;

            }

            originalSolids = new List<Solid>(solids);

            avgLocation /= (float)(i+1);

            //subtractSolids.Add(Solids.Sphere(2, avgLocation.ToCsg()));

            //dirty = true;


        }

    }
}
