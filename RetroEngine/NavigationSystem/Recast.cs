using BulletSharp;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using DotRecast.Recast.Geom;
using DotRecast.Core;

using static DotRecast.Recast.RcRecast;
using static DotRecast.Recast.RcAreas;
using System.IO;
using System.Runtime.CompilerServices;
using MonoGame.Extended.ECS;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Geom;
using RetroEngine.Map;

namespace RetroEngine.NavigationSystem
{
    public static class Recast
    {

        private const float m_cellSize = 0.3f;
        private const float m_cellHeight = 0.2f;
        private const float m_agentHeight = 2.0f;
        private const float m_agentRadius = 0.6f;
        private const float m_agentMaxClimb = 0.9f;
        private const float m_agentMaxSlope = 45.0f;
        private const int m_regionMinSize = 8;
        private const int m_regionMergeSize = 20;
        private const float m_edgeMaxLen = 12.0f;
        private const float m_edgeMaxError = 1.3f;
        private const int m_vertsPerPoly = 6;
        private const float m_detailSampleDist = 6.0f;
        private const float m_detailSampleMaxError = 1.0f;
        private static RcPartition m_partitionType = RcPartition.WATERSHED;

        public static DtNavMesh dtNavMesh;



        public static void LoadSampleNavMesh()
        {

            var partitionType = RcPartition.LAYERS;

            string filename = AssetRegistry.ROOT_PATH + $"GameData/maps/{Level.GetCurrent().Name.Replace(".map",".obj")}";

            //filename = AssetRegistry.ROOT_PATH + "annotation_test.obj";

            filename = Path.GetFullPath(filename);

            m_partitionType = partitionType;
            DemoInputGeomProvider geomProvider = DemoInputGeomProvider.LoadFile(filename, 1f/MapData.UnitSize);

            TileNavMeshBuilder tileNavMeshBuilder = new TileNavMeshBuilder();

            RcNavMeshBuildSettings rcNavMeshBuildSettings = new RcNavMeshBuildSettings();

            rcNavMeshBuildSettings.cellSize = 0.2f;

            var buildResult = tileNavMeshBuilder.Build(geomProvider, rcNavMeshBuildSettings);

            if (buildResult.Success)
            {
                dtNavMesh = buildResult.NavMesh;
            }else
            {
                Logger.Log("Error generating nav mesh");
            }

        }

        private static void SaveObj(string filename, RcPolyMesh mesh)
        {
            try
            {
                string path = Path.Combine("test-output", filename);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using StreamWriter fw = new StreamWriter(path);
                for (int v = 0; v < mesh.nverts; v++)
                {
                    fw.Write("v " + (mesh.bmin.X + mesh.verts[v * 3] * mesh.cs) + " "
                             + (mesh.bmin.Y + mesh.verts[v * 3 + 1] * mesh.ch) + " "
                             + (mesh.bmin.Z + mesh.verts[v * 3 + 2] * mesh.cs) + "\n");
                }

                for (int i = 0; i < mesh.npolys; i++)
                {
                    int p = i * mesh.nvp * 2;
                    fw.Write("f ");
                    for (int j = 0; j < mesh.nvp; ++j)
                    {
                        int v = mesh.polys[p + j];
                        if (v == RC_MESH_NULL_IDX)
                        {
                            break;
                        }

                        fw.Write((v + 1) + " ");
                    }

                    fw.Write("\n");
                }

                fw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void SaveObj(string filename, RcPolyMeshDetail dmesh)
        {
            try
            {
                string filePath = Path.Combine("test-output", filename);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                using StreamWriter fw = new StreamWriter(filePath);
                for (int v = 0; v < dmesh.nverts; v++)
                {
                    fw.Write(
                        "v " + dmesh.verts[v * 3] + " " + dmesh.verts[v * 3 + 1] + " " + dmesh.verts[v * 3 + 2] + "\n");
                }

                for (int m = 0; m < dmesh.nmeshes; m++)
                {
                    int vfirst = dmesh.meshes[m * 4];
                    int tfirst = dmesh.meshes[m * 4 + 2];
                    for (int f = 0; f < dmesh.meshes[m * 4 + 3]; f++)
                    {
                        fw.Write("f " + (vfirst + dmesh.tris[(tfirst + f) * 4] + 1) + " "
                                 + (vfirst + dmesh.tris[(tfirst + f) * 4 + 1] + 1) + " "
                                 + (vfirst + dmesh.tris[(tfirst + f) * 4 + 2] + 1) + "\n");
                    }
                }

                fw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }

    

}
