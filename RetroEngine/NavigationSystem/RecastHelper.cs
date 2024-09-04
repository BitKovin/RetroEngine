using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.NavigationSystem
{
    public static class RecastHelper
    {

        public static RcVec3f ToRc(this Vector3 vector)
        {
            return new RcVec3f(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 FromRc(this RcVec3f vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);   
        }

        public static List<Vector3> ConvertPath(this List<RcVec3f> l)
        {

            List<Vector3> res = new List<Vector3>();
            res.Capacity = l.Count;

            foreach (RcVec3f r in l)
            {
                res.Add(r.FromRc());
            }

            return res;

        }

        public static Vector3 GetAvgVertexPosition(DtMeshTile curTile, DtPoly curPoly)
        {
            // Calculate the centroid of the current polygon
            RcVec3f polyPos = new RcVec3f();
            int vertCount = curPoly.vertCount;

            // Iterate through each vertex of the polygon
            for (int i = 0; i < vertCount; ++i)
            {
                int vertIndex = curPoly.verts[i] * 3;

                polyPos.X += curTile.data.verts[vertIndex];
                polyPos.Y += curTile.data.verts[vertIndex + 1];
                polyPos.Z += curTile.data.verts[vertIndex + 2];
            }

            // Calculate the average to get the centroid position
            polyPos.X /= vertCount;
            polyPos.Y /= vertCount;
            polyPos.Z /= vertCount;

            return polyPos.FromRc();

        }

        public static List<Vector3> GetVertexPositions(DtMeshTile curTile, DtPoly curPoly)
        {
            int vertCount = curPoly.vertCount;

            List<Vector3> vectors = new List<Vector3>();

            // Iterate through each vertex of the polygon
            for (int i = 0; i < vertCount; ++i)
            {
                int vertIndex = curPoly.verts[i] * 3;

                Vector3 v = new Vector3();

                v.X += curTile.data.verts[vertIndex];
                v.Y += curTile.data.verts[vertIndex + 1];
                v.Z += curTile.data.verts[vertIndex + 2];

                vectors.Add(v);
            }

            return vectors;

        }

    }
}
