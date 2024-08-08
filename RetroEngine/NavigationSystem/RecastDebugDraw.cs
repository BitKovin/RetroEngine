using DotRecast.Detour;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.NavigationSystem
{
    public class RecastDebugDraw
    {

        public static void DebugDrawNavMeshPolysWithFlags(DtNavMesh mesh, int polyFlags, int col)
        {
            for (int i = 0; i < mesh.GetMaxTiles(); ++i)
            {
                DtMeshTile tile = mesh.GetTile(i);
                if (tile == null || tile.data == null || tile.data.header == null)
                {
                    continue;
                }

                long @base = mesh.GetPolyRefBase(tile);

                for (int j = 0; j < tile.data.header.polyCount; ++j)
                {
                    DtPoly p = tile.data.polys[j];
                    if ((p.flags & polyFlags) == 0)
                    {
                        continue;
                    }

                    DebugDrawNavMeshPoly(mesh, @base | (long)j, col);
                }
            }
        }

        public static void DebugDrawNavMeshPolys(DtNavMesh mesh)
        {

            for (int i = 0; i < mesh.GetMaxTiles(); ++i)
            {

                DtMeshTile tile = mesh.GetTile(i);
                if (tile == null || tile.data == null || tile.data.header == null)
                {
                    continue;
                }

                long @base = mesh.GetPolyRefBase(tile);


                for (long j = 0; j < tile.data.header.polyCount; ++j)
                {

                    Vector3 p = (tile.data.header.bmax + tile.data.header.bmin).FromRc();
                    p /= 2;

                    if(Vector3.Distance(p, Camera.position) < 30)
                        DebugDrawNavMeshPoly(mesh, @base | j, 0);
                }
            }
        }

        public static void DebugDrawNavMeshPoly(DtNavMesh mesh, long refs, int col)
        {
            if (refs == 0)
            {
                return;
            }

            var status = mesh.GetTileAndPolyByRef(refs, out var tile, out var poly);
            if (status.Failed())
            {
                return;
            }

            int c = 0;
            int ip = poly.index;

            DrawPoly(tile, ip, col);

        }

        static void Vertex(float x, float y, float z, int color)
        {

            Vector3 v = new Vector3(x, y, z);

            //DrawDebug.Text(v, v.ToString(), 0.01f);

            Vertices.Add(v);
        }

        static List<Vector3> Vertices = new List<Vector3>();

        static void BeginTriangle()
        {
            Vertices.Clear();
        }

        static void EndTriangle()
        {

            foreach (var v in Vertices)
            {
                foreach (var v2 in Vertices)
                {
                    DrawDebug.Line(v, v2,Vector3.UnitY, 0.01f);
                }
            }

        }

        private static void DrawPoly(DtMeshTile tile, int index, int col)
        {
            DtPoly p = tile.data.polys[index];
            if (tile.data.detailMeshes != null)
            {
                ref DtPolyDetail pd = ref tile.data.detailMeshes[index];
                for (int j = 0; j < pd.triCount; ++j)
                {
                    int t = (pd.triBase + j) * 4;

                    BeginTriangle();

                    for (int k = 0; k < 3; ++k)
                    {
                        int v = tile.data.detailTris[t + k];
                        if (v < p.vertCount)
                        {
                            Vertex(tile.data.verts[p.verts[v] * 3], tile.data.verts[p.verts[v] * 3 + 1],
                                tile.data.verts[p.verts[v] * 3 + 2], col);
                        }
                        else
                        {
                            Vertex(tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3],
                                tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 1],
                                tile.data.detailVerts[(pd.vertBase + v - p.vertCount) * 3 + 2], col);
                        }
                    }

                    EndTriangle();

                }
            }
            else
            {
                for (int j = 1; j < p.vertCount - 1; ++j)
                {
                    Vertex(tile.data.verts[p.verts[0] * 3], tile.data.verts[p.verts[0] * 3 + 1],
                        tile.data.verts[p.verts[0] * 3 + 2], col);
                    for (int k = 0; k < 2; ++k)
                    {
                        Vertex(tile.data.verts[p.verts[j + k] * 3], tile.data.verts[p.verts[j + k] * 3 + 1],
                            tile.data.verts[p.verts[j + k] * 3 + 2], col);
                    }
                }
            }
        }

    }
}
