﻿using Microsoft.Xna.Framework;
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

            List<Vector3> points = GetStartNavPoint(start).GetPathNext(new List<NavPoint>(), target);
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

            return navPoints[0];

        }

        public static void RebuildConnectionsData()
        {
            foreach (NavPoint point in navPoints)
            {
                point.BuildConnectionsData();
            }
        }

        public static List<NavPoint> GetNavPoints() { return navPoints; }

    }
}