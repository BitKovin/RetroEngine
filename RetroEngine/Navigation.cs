using Microsoft.Xna.Framework;
using RetroEngine.Entities.Navigaion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class Navigation
    {

        static List<NavPoint> navPoints = new List<NavPoint>();

        public static void ClearNavData()
        {
            navPoints.Clear();
        }

        public static void AddPoint(NavPoint point)
        {
            navPoints.Add(point);

        }

        public static List<Vector3> FindPath(Vector3 start, Vector3 target)
        {

            target = Physics.LineTraceForStatic(target.ToNumerics(), (target - new Vector3(0, 100, 0)).ToNumerics()).HitPointWorld;

            target += new Vector3(0, 0.25f, 0);

            int it = 0;

            NavPoint startPoint = GetStartNavPoint(start);

            if (startPoint is null)
                return new List<Vector3>();

            List<Vector3> points = startPoint.GetPathNext(new List<NavPoint>(), target, ref it);
            List<Vector3> result = new List<Vector3>(points);

            for(int i = points.Count -1; i >=0; i--)
            {
                var hit = Physics.LineTraceForStatic(start.ToNumerics(), points[i].ToNumerics());

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

            var hit = Physics.LineTraceForStatic(point.ToNumerics(), (point - new Vector3(0, 10, 0)).ToNumerics());

            Position = hit.HitPointWorld;
            Position += new Vector3(0, 0.25f, 0);

            return Position;
        }

        static NavPoint GetStartNavPoint(Vector3 start)
        {
            navPoints = navPoints.OrderBy(point => Vector3.Distance(start, point.Position)).ToList();

            foreach (var point in navPoints)
            {
               var hit = Physics.LineTraceForStatic(start.ToNumerics(), point.Position.ToNumerics());
                
                if(hit.HasHit == false)
                    return point;

            }

            Console.WriteLine("failed to find point");

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
}
