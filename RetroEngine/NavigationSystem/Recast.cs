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
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Geom;
using RetroEngine.Map;
using Assimp;
using DotRecast.Detour.TileCache;
using DotRecast.Detour.TileCache.Io.Compress;
using DotRecast.Core.Collections;
using DotRecast.Recast.Toolset.Tools;
using Sdcb.FFmpeg.Filters;
using System.Collections;

namespace RetroEngine.NavigationSystem
{
    public static class Recast
    {

        public static DtNavMesh dtNavMesh;



        public static void LoadSampleNavMesh()
        {

            var partitionType = RcPartition.WATERSHED;

            string filename = AssetRegistry.ROOT_PATH + $"GameData/maps/{Level.GetCurrent().Name.Replace(".map",".obj")}";

            //filename = AssetRegistry.ROOT_PATH + "annotation_test.obj";

            filename = Path.GetFullPath(filename);

            DemoInputGeomProvider geomProvider = DemoInputGeomProvider.LoadFile(filename, 1f/MapData.UnitSize);

            TileNavMeshBuilder tileNavMeshBuilder = new TileNavMeshBuilder();

            RcNavMeshBuildSettings rcNavMeshBuildSettings = new RcNavMeshBuildSettings();

            rcNavMeshBuildSettings.cellSize = 0.1f;
            rcNavMeshBuildSettings.agentRadius = 0.3f;

            var buildResult = tileNavMeshBuilder.Build(geomProvider, rcNavMeshBuildSettings);


            if (buildResult.Success)
            {
                dtNavMesh = buildResult.NavMesh;
            }else
            {
                Logger.Log("Error generating nav mesh");
            }

        }

        public static List<Vector3> FindPathSimple(Vector3 start, Vector3 end, IDtQueryFilter filter = null)
        {
            List<RcVec3f> path = new List<RcVec3f>();


            DtNavMeshQuery navMeshQuery = new DtNavMeshQuery(NavigationSystem.Recast.TileCache.GetNavMesh());


            if (filter == null)
                filter = new NavigationQueryFilter();

            List<long> longs = new List<long>();

            RcTestNavMeshTool rcTestNavMeshTool = new RcTestNavMeshTool();

            long startRef = 0;
            long endRef = 0;



            RcVec3f m_polyPickExt = new RcVec3f(2, 4, 2);

            lock (NavigationSystem.Recast.TileCache.GetNavMesh())
                navMeshQuery.FindNearestPoly(start.ToRc(), m_polyPickExt, filter, out startRef, out var _, out var _);

            lock (NavigationSystem.Recast.TileCache.GetNavMesh())
                navMeshQuery.FindNearestPoly(end.ToRc(), m_polyPickExt, filter, out endRef, out var _, out var _);


            DtStatus result;

            lock (NavigationSystem.Recast.dtNavMesh) lock(TileCache)
                result = rcTestNavMeshTool.FindFollowPath(NavigationSystem.Recast.dtNavMesh, navMeshQuery, startRef, endRef, start.ToRc(), end.ToRc(), filter, true, ref longs, 0, ref path);

            if (result.Succeeded())
                return path.ConvertPath();




            return new List<Vector3> { start, end };

        }

        public static bool IsPointOnNavmesh(Vector3 point)
        {

            DtNavMeshQuery navMeshQuery = new DtNavMeshQuery(NavigationSystem.Recast.TileCache.GetNavMesh());

            var filter = new NavigationQueryFilter();

            List<long> longs = new List<long>();

            RcTestNavMeshTool rcTestNavMeshTool = new RcTestNavMeshTool();

            long startRef = 0;
            long endRef = 0;

            bool isOver;

            RcVec3f m_polyPickExt = new RcVec3f(0, 0.1f, 0);

            lock (NavigationSystem.Recast.TileCache.GetNavMesh()) 
                navMeshQuery.FindNearestPoly(point.ToRc(), m_polyPickExt, filter, out startRef, out var _, out isOver);

            return isOver;

        }

        static List<long> allObstacles = new List<long>();

        public static long AddObstacleBox(Vector3 min, Vector3 max)
        {

            lock (TileCache)
            {

                if (TileCache == null)
                    return 0;

                long obst = TileCache.AddBoxObstacle(min.ToRc(), max.ToRc());
                lock (allObstacles)
                {
                    allObstacles.Add(obst);
                }

                return obst;

            }
            
        }

        public static long AddObstacleCapsule(Vector3 pos, float radius, float height)
        {
            lock (TileCache)
                {
                    long obst = TileCache.AddObstacle(pos.ToRc(), radius, height);
                    lock (allObstacles)
                    {
                        allObstacles.Add(obst);
                    }
                    return obst;
                }
        }

        public static void RemoveObstacle(long id)
        {
            lock (TileCache)
                    TileCache.RemoveObstacle(id);
            lock (allObstacles)
                allObstacles.Remove(id);
        }

        public static void RemoveAllObstacles()
        {

            lock(allObstacles)
            {
                foreach(long obst in allObstacles)
                    TileCache.RemoveObstacle(obst);

                allObstacles.Clear();

            }

        }


        public static void BuildNavigationData()
        {


            float[] verts = new float[0];
            int[] faces = new int[0];

            List<StaticMesh.MeshData> meshDatas = new List<StaticMesh.MeshData>();  

            foreach(Entity entity in Level.GetCurrent().entities)
            {

                if (entity.Static == false) continue;

                foreach(StaticMesh mesh in entity.meshes)
                {
                    if (mesh.Static == false) continue;

                    meshDatas.AddRange(mesh.GetMeshData());

                }

            }

            MergeMeshes(meshDatas, out verts, out faces);

            if (verts.Length == 0)
                return;

            DemoInputGeomProvider geomProvider = new DemoInputGeomProvider(verts, faces);

            TileNavMeshBuilder tileNavMeshBuilder = new TileNavMeshBuilder();

            RcNavMeshBuildSettings rcNavMeshBuildSettings = new RcNavMeshBuildSettings();

            rcNavMeshBuildSettings.cellSize = 0.3f;
            rcNavMeshBuildSettings.agentRadius = 0.4f;
            rcNavMeshBuildSettings.tileSize = 32;

            var buildResult = Build(geomProvider, rcNavMeshBuildSettings, RcByteOrder.LITTLE_ENDIAN, true);//tileNavMeshBuilder.Build(geomProvider, rcNavMeshBuildSettings);

            if (buildResult.Success)
            {
                dtNavMesh = buildResult.NavMesh;
            }
            else
            {
                Logger.Log("Error generating nav mesh");
            }

            CrowdSystem.Init();

        }

        private static IDtTileCacheCompressorFactory _comp = DtTileCacheCompressorFactory.Shared;
        private static DemoDtTileCacheMeshProcess _proc = new DemoDtTileCacheMeshProcess();
        internal static DtTileCache TileCache;

        static NavMeshBuildResult Build(IInputGeomProvider geom, RcNavMeshBuildSettings setting, RcByteOrder order, bool cCompatibility)
        {
            if (null == geom || null == geom.GetMesh())
            {
                //m_ctx->log(RC_LOG_ERROR, "buildTiledNavigation: No vertices and triangles.");
                return new NavMeshBuildResult();
            }


            _proc.Init(geom);

            // Init cache
            var bmin = geom.GetMeshBoundsMin();
            var bmax = geom.GetMeshBoundsMax();
            RcRecast.CalcGridSize(bmin, bmax, setting.cellSize, out var gw, out var gh);
            int ts = setting.tileSize;
            int tw = (gw + ts - 1) / ts;
            int th = (gh + ts - 1) / ts;

            // Generation params.
            var walkableRadius = (int)MathF.Ceiling(setting.agentRadius / setting.cellSize); // Reserve enough padding.
            RcConfig cfg = new RcConfig(
                true, setting.tileSize, setting.tileSize,
                walkableRadius + 3,
                RcPartitionType.OfValue(setting.partitioning),
                setting.cellSize, setting.cellHeight,
                setting.agentMaxSlope, setting.agentHeight, setting.agentRadius, setting.agentMaxClimb,
                (int)RcMath.Sqr(setting.minRegionSize), (int)RcMath.Sqr(setting.mergedRegionSize), // Note: area = size*size
                (int)(setting.edgeMaxLen / setting.cellSize), setting.edgeMaxError,
                setting.vertsPerPoly,
                setting.detailSampleDist, setting.detailSampleMaxError,
                true, true, true,
                SampleAreaModifications.SAMPLE_AREAMOD_WALKABLE, true);

            var builder = new DtTileCacheLayerBuilder(DtTileCacheCompressorFactory.Shared);
            var storageParams = new DtTileCacheStorageParams(order, cCompatibility);
            var results = builder.Build(geom, cfg, storageParams, Environment.ProcessorCount, tw, th);

            var layers = results
                .SelectMany(x => x.layers)
                .ToList();

            TileCache = CreateTileCache(geom, setting, tw, th, order, cCompatibility);

            for (int i = 0; i < layers.Count; ++i)
            {
                var layer = layers[i];
                var refs = TileCache.AddTile(layer, 0);
                TileCache.BuildNavMeshTile(refs);
            }


            return new NavMeshBuildResult(RcImmutableArray<RcBuilderResult>.Empty, TileCache.GetNavMesh());
        }

        static DtTileCache CreateTileCache(IInputGeomProvider geom, RcNavMeshBuildSettings setting, int tw, int th, RcByteOrder order, bool cCompatibility)
        {
            DtTileCacheParams option = new DtTileCacheParams();
            option.ch = setting.cellHeight;
            option.cs = setting.cellSize;
            option.orig = geom.GetMeshBoundsMin();
            option.height = setting.tileSize;
            option.width = setting.tileSize;
            option.walkableHeight = setting.agentHeight;
            option.walkableRadius = setting.agentRadius;
            option.walkableClimb = setting.agentMaxClimb;
            option.maxSimplificationError = setting.edgeMaxError;
            option.maxTiles = tw * th * 15; // for test EXPECTED_LAYERS_PER_TILE;
            option.maxObstacles = 2048;

            DtNavMeshParams navMeshParams = new DtNavMeshParams();
            navMeshParams.orig = geom.GetMeshBoundsMin();
            navMeshParams.tileWidth = setting.tileSize * setting.cellSize;
            navMeshParams.tileHeight = setting.tileSize * setting.cellSize;
            navMeshParams.maxTiles = tw * th * 4; // ..
            navMeshParams.maxPolys = 16384*2 *2;

            var navMesh = new DtNavMesh();
            navMesh.Init(navMeshParams, 6);
            var comp = _comp.Create(cCompatibility ? 0 : 1);
            var storageParams = new DtTileCacheStorageParams(order, cCompatibility);
            DtTileCache tc = new DtTileCache(option, storageParams, navMesh, comp, _proc);


            return tc;
        }

        static void MergeMeshes(List<StaticMesh.MeshData> meshDataList, out float[] verts, out int[] faces)
        {
            List<float> mergedVertices = new List<float>();
            List<int> mergedIndices = new List<int>();

            int vertexOffset = 0;

            foreach (var meshData in meshDataList)
            {
                // Add vertices to the merged list
                foreach (var vertex in meshData.vertices)
                {
                    mergedVertices.Add(vertex.X);
                    mergedVertices.Add(vertex.Y);
                    mergedVertices.Add(vertex.Z);
                }

                // Add indices to the merged list
                foreach (var index in meshData.indices)
                {
                    mergedIndices.Add(index + vertexOffset);
                }

                // Update the vertex offset
                vertexOffset += meshData.vertices.Count;
            }

            // Convert lists to arrays
            verts = mergedVertices.ToArray();
            faces = mergedIndices.ToArray();
        }


    }

    

}
