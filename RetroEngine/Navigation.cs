using BulletSharp.SoftBody;
using Microsoft.Xna.Framework;
using RetroEngine.Entities.Navigaion;
using System;
using System.Collections;
using System.Collections.Generic;
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

        static List<NavPoint> navPoints = new List<NavPoint>();

        internal static List<PathfindingQuery> pathfindingQueries = new List<PathfindingQuery>();

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
            while(true)
            {

                ProcessingPathfinding = false;

                if (waitingToFinish)
                {
                    Thread.Sleep(1);
                    waitingToFinish = false;
                    continue;
                }

                if (Level.ChangingLevel)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ProcessingPathfinding = true;

                ParallelOptions options = new ParallelOptions();
                options.MaxDegreeOfParallelism = 8;


                int n = Math.Max(pathfindingQueries.Count, Math.Min(8, pathfindingQueries.Count * 3));

                List<PathfindingQuery> queries = pathfindingQueries.GetRange(0, Math.Min(pathfindingQueries.Count, n));

                List<PathfindingQuery> removeList = new List<PathfindingQuery>();

                Parallel.ForEach(queries, options, item =>
                {
                    item?.Execute();
                    removeList.Add(item);
                });

                foreach (PathfindingQuery query in removeList)
                    pathfindingQueries.Remove(query);

                Thread.Sleep(1);

                ProcessingPathfinding = false;

            }
        }

        static Task UpdateTask;

        internal static void WaitForProcess()
        {

            waitingToFinish = true;

            while(ProcessingPathfinding)
            {

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




        }


        public static List<Vector3> FindPath(Vector3 start, Vector3 target, PathfindingQuery query = null)
        {

            target = Navigation.ProjectToGround(target);

            int it = 0;

            NavPoint startPoint = GetStartNavPoint(start);

            if (startPoint is null)
                return new List<Vector3>();

            List<Vector3> points = startPoint.GetPathNext(new List<NavPoint>(), target, ref it, query);
            List<Vector3> result = new List<Vector3>(points);

            for(int i = points.Count -1; i >=0; i--)
            {
                var hit = Physics.SphereTraceForStatic(start.ToNumerics(), points[i].ToNumerics(), 0.3f);

                if(hit.HasHit == false)
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
            Position += new Vector3(0, 1.25f, 0);

            return Position;
        }

        public static NavPoint GetStartNavPoint(Vector3 start)
        {
            navPoints = navPoints.OrderBy(point => Vector3.Distance(start, point.Position)).ToList();

            foreach (var point in navPoints)
            {
               var hit = Physics.SphereTraceForStatic(start.ToNumerics(), point.Position.ToNumerics(), 0.3f);
                
                if(hit.HasHit == false)
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

        internal Delay deathDelay = new Delay();

        public void Start(Vector3 start, Vector3 target)
        {
            if (Navigation.pathfindingQueries.Contains(this)) return;


            startLocation = start;
            endLocation = target;

            deathDelay.AddDelay(3);

            Navigation.pathfindingQueries.Add(this);
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

        void ProcessPoint(NavPoint point,Vector3 pos)
        {
            var hit = Physics.SphereTraceForStatic(pos.ToNumerics(), point.Position.ToNumerics(), 0.3f);

            if (hit.HasHit) return;

            if(VisibleLocations.ContainsKey(point) == false)
                VisibleLocations.Add(point, new List<Vector3>());

            if (VisibleLocationsFromPoint.ContainsKey(point.Position) == false)
                VisibleLocationsFromPoint.Add(point.Position, new List<Vector3>());

            VisibleLocations[point].Add(pos);
            VisibleLocationsFromPoint[point.Position].Add(pos);

        }

        internal void Execute()
        {
            Process(startLocation, endLocation);
        }

        void Process(Vector3 start, Vector3 target)
        {

            start = Navigation.ProjectToGround(start);
            target = Navigation.ProjectToGround(target);    

            var hit = Physics.SphereTraceForStatic(start.ToPhysics(), target.ToPhysics(), 0.3f);

            if(hit.HasHit == false)
            {
                OnPathFound?.Invoke(new List<Vector3>() { target});
                return;
            }

            List<Vector3> points;
            points = Navigation.FindPath(start, target);
            OnPathFound?.Invoke(points);

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

            List<Vector3> points = startPoint.GetPathNext(new List<NavPoint>(), target, ref it);
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
