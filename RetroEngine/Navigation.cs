using BulletSharp.SoftBody;
using DotRecast.Detour;
using Microsoft.Xna.Framework;
using RetroEngine.Entities.Navigaion;
using RetroEngine.NavigationSystem;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class Navigation
    {

        internal static int ActiveQueries = 0;

        internal static List<NavPoint> navPoints = new List<NavPoint>();

        static List<PathfindingQuery> removeList = new List<PathfindingQuery>();

        internal static List<PathfindingQuery> pathfindingQueries = new List<PathfindingQuery>();

        internal static List<PathfindingQuery> PendingPathfindingQueries = new List<PathfindingQuery>();

        internal static bool DrawNavigation = false;



        [ConsoleCommand("nav.draw")]
        public static void SetDrawNavigation(bool value)
        {
            DrawNavigation = value;
        }

        public static void ClearNavData()
        {
            navPoints.Clear();
        }

        public static void AddPoint(NavPoint point)
        {
            navPoints.Add(point);

        }

        public static List<NavPoint> GetAllPoints()
        {
            return navPoints;
        }

        internal static bool ProcessingPathfinding = false;

        static bool waitingToFinish = false;

        static void UpdateCycle()
        {


            Thread.CurrentThread.IsBackground = true;

            while (true)
            {


                ProcessingPathfinding = false;

                if (waitingToFinish)
                {
                    Thread.Sleep(10);
                    waitingToFinish = false;
                    continue;
                }

                if (Level.ChangingLevel)
                {
                    Thread.Sleep(10);
                    continue;
                }

                ProcessingPathfinding = true;

                lock (PendingPathfindingQueries) lock(pathfindingQueries)
                {
                    pathfindingQueries.AddRange(PendingPathfindingQueries);
                    PendingPathfindingQueries.Clear();
                }
                

                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = 8;
                List<PathfindingQuery> queries;
                lock (pathfindingQueries)
                {
                    int n = Math.Max(pathfindingQueries.Count, Math.Min(8, pathfindingQueries.Count * 3));

                    queries = pathfindingQueries.GetRange(0, Math.Min(pathfindingQueries.Count, n));
                }
                foreach (PathfindingQuery item in queries)
                {
                    item?.Execute();
                    removeList.Add(item);
                }
                lock (pathfindingQueries)
                {
                    foreach (PathfindingQuery query in removeList)
                    {
                        pathfindingQueries.Remove(query);
                    }
                }
                removeList.Clear();
                Thread.Sleep(1);

                ProcessingPathfinding = false;

            }
        }

        static Task UpdateTask;

        internal static void WaitForProcess()
        {

            waitingToFinish = true;

            Stopwatch stopwatch = Stopwatch.StartNew();

            while (ProcessingPathfinding)
            {
                if (stopwatch.Elapsed.TotalSeconds > 5)
                {
                    ProcessingPathfinding = false;
                    Logger.Log("Force Skipping Navigation");
                    UpdateTask = null;
                    return;
                }
                    
            }
        }

        public static void Update()
        {

            if (UpdateTask == null)
            {
                UpdateTask = Task.Factory.StartNew(() => { UpdateCycle(); });
            }
            else
            {
                if (UpdateTask.IsCompleted || UpdateTask.IsFaulted)
                {
                    UpdateTask = Task.Factory.StartNew(() => { UpdateCycle(); });
                    Logger.Log("restarting nav task");
                }
            }

            if (Recast.dtNavMesh != null && GameMain.Instance.paused == false)
            {

                lock (Recast.TileCache)
                {

                    Recast.TileCache?.Update();

                    if(DrawNavigation)
                        RecastDebugDraw.DebugDrawNavMeshPolys(Recast.dtNavMesh);
                }

                CrowdSystem.Update();

            }


        }


        public static List<Vector3> FindPath(Vector3 start, Vector3 target, PathfindingQuery query = null)
        {

            target = ProjectToGround(target);

            int it = 0;

            NavPoint startPoint = GetStartNavPoint(start);

            if (startPoint is null)
                return new List<Vector3>();

            List<Vector3> points = startPoint.GetPathNext(new List<NavPoint>(), new List<NavPoint>(), target, ref it, query);
            List<Vector3> result = new List<Vector3>(points);

            for (int i = points.Count - 1; i >= 0; i--)
            {
                var hit = Physics.LineTrace(start.ToNumerics(), points[i].ToNumerics(), bodyType: PhysicsSystem.BodyType.World);

                if (hit.HasHit == false)
                {
                    result.RemoveRange(0, i); break;
                }

            }

            return result;

        }

        public static Vector3 ProjectToGround(Vector3 point)
        {
            Vector3 Position;

            var hit = Physics.LineTraceForStatic(point.ToNumerics(), (point - new Vector3(0, 100, 0)).ToNumerics());

            Position = hit.HitPointWorld;
            Position += new Vector3(0, 0.35f, 0);

            return Position;
        }

        public static NavPoint GetStartNavPoint(Vector3 start)
        {
            navPoints = navPoints.OrderBy(point => Vector3.Distance(start, point.Position)).ToList();

            foreach (var point in navPoints)
            {
                var hit = Physics.SphereTrace(start.ToNumerics(), point.Position.ToNumerics(), 0.15f, bodyType: BodyType.World);

                if (hit.HasHit == false)
                    return point;

            }

            return null;

        }

        public static void RebuildConnectionsData()
        {
            foreach (NavPoint point in navPoints)
            {
                point.BuildConnectionsData();
            }
            Logger.Log("Built Nav Data");
        }

        public static List<NavPoint> GetNavPoints() { return navPoints; }

    }

    public delegate void PathFound(List<Vector3> points);

    public class PathfindingQuery
    {
        public event PathFound OnPathFound;

        public bool Processing { get; private set; } = false;

        Dictionary<NavPoint, List<Vector3>> VisibleLocations = new Dictionary<NavPoint, List<Vector3>>();
        Dictionary<Vector3, List<Vector3>> VisibleLocationsFromPoint = new Dictionary<Vector3, List<Vector3>>();

        Task task;

        Vector3 startLocation;
        Vector3 endLocation;

        IDtQueryFilter navFilter = null;

        internal Delay deathDelay = new Delay();

        public void Start(Vector3 start, Vector3 target, IDtQueryFilter QueryFilter = null)
        {
            if (Navigation.pathfindingQueries.Contains(this)) return;


            startLocation = start;
            endLocation = target;

            navFilter = QueryFilter;
            
            lock (Navigation.pathfindingQueries)
            {
                Navigation.PendingPathfindingQueries.Add(this);
            }
            return;

            task = Task.Run(() => { Process(start, target); });


            return;
            target = Navigation.ProjectToGround(target);

            if (Physics.SphereTraceForStatic(start.ToNumerics(), target.ToNumerics(), 0.3f).HasHit == false)
            {
                OnPathFound?.Invoke(new List<Vector3> { target });
                return;
            }



        }

        void ProcessPoint(NavPoint point, Vector3 pos)
        {
            var hit = Physics.SphereTrace(pos.ToNumerics(), point.Position.ToNumerics(), 0.3f, bodyType: PhysicsSystem.BodyType.World);

            if (hit.HasHit) return;

            if (VisibleLocations.ContainsKey(point) == false)
                VisibleLocations.Add(point, new List<Vector3>());

            if (VisibleLocationsFromPoint.ContainsKey(point.Position) == false)
                VisibleLocationsFromPoint.Add(point.Position, new List<Vector3>());

            VisibleLocations[point].Add(pos);
            VisibleLocationsFromPoint[point.Position].Add(pos);

        }

        internal void Execute()
        {
            deathDelay.AddDelay(5);
            Process(startLocation, endLocation);
            //Thread.Sleep(1);
        }

        void Process(Vector3 start, Vector3 target)
        {

            var result = Recast.FindPathSimple(start, target, navFilter);

            if(result.Count>0)
                result.RemoveAt(0);

            OnPathFound.Invoke(result);

            return;


            start = Navigation.ProjectToGround(start);
            target = Navigation.ProjectToGround(target);

            var hit = Physics.SphereTrace(start.ToPhysics() + Vector3.UnitY.ToPhysics() * 0.3f, target.ToPhysics() + Vector3.UnitY.ToPhysics() * 0.3f, 0.3f, bodyType: PhysicsSystem.BodyType.World);

            if (hit.HasHit == false)
            {
                OnPathFound?.Invoke(new List<Vector3>() { target });
                return;
            }

            List<Vector3> points1;
            List<Vector3> points2;
            points1 = Navigation.FindPath(start, target);
            points2 = Navigation.FindPath(target, start);
            points2.Reverse();
            if (points2.Count > 0)
            {
                points2.RemoveAt(0);
                points2.Add(target);
            }
            

            float dist1 = CalculatePathDistance(points1, start);
            float dist2 = CalculatePathDistance(points1, target);


            List<Vector3> points = (dist2 > dist1) || (points2.Count < 2) ? points1 : points2;

            OnPathFound?.Invoke(points);

        }

        float CalculatePathDistance(List<Vector3> points, Vector3 startPoint)
        {

            if(points.Count==0)
                return 0;

            float distance = 0;

            Vector3 prevPoint = startPoint;

            foreach (Vector3 point in points)
            {
                distance += Vector3.Distance(prevPoint, point);
                prevPoint = point;
            }

            return distance;
        }

        internal bool IsVisibleFromPoint(NavPoint point, Vector3 location)
        {
            if (VisibleLocations.ContainsKey(point) == false) return false;

            return VisibleLocations[point].Contains(location);

        }

        internal bool IsVisibleFromPoint(Vector3 point, Vector3 location)
        {
            if (VisibleLocationsFromPoint.ContainsKey(point) == false) return false;

            return VisibleLocationsFromPoint[point].Contains(location);

        }

        List<Vector3> FindPath(Vector3 start, Vector3 target, NavPoint startPoint)
        {

            int it = 0;

            if (startPoint is null)
                return new List<Vector3>();

            List<Vector3> points = startPoint.GetPathNext(new List<NavPoint>(), new List<NavPoint>(), target, ref it);
            List<Vector3> result = new List<Vector3>(points);

            for (int i = points.Count - 1; i >= 0; i--)
            {
                if (IsVisibleFromPoint(points[i], start) == true)
                {

                    Console.WriteLine($"{points[i]}   {start}");
                    result.RemoveRange(0, i); break;
                }

            }

            bool removing = false;


            return result;

        }


    }
}
