﻿using Microsoft.Xna.Framework;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Navigaion
{
    [LevelObject("info.navpoint")]
    public class NavPoint : Entity
    {

        public List<NavPoint> staticConnected = new List<NavPoint>();
        public List<NavPoint> connected = new List<NavPoint>();


        int MaxDepth = 30;

        int spawnedId = 0;

        public override void Start()
        {
            base.Start();

            SnapToGround();
            spawnedId = Navigation.navPoints.Count;
            Navigation.AddPoint(this);
        }

        public override void Update()
        {
            base.AsyncUpdate();

            int total = Navigation.navPoints.Count;

            if ((Time.FrameCount % total) != spawnedId) return;

            UpdateInternalConnected();

            if(Navigation.DrawNavigation == false) return;

            DrawDebug.Text(Position, ToString());

            foreach(NavPoint p in connected)
            {
                DrawDebug.Line(Position, p.Position, Vector3.UnitZ, 0.01f);
            }

        }

        public void SnapToGround()
        {
            Position = Navigation.ProjectToGround(Position);
        }

        public void BuildConnectionsData()
        {
            connected.Clear();

            List<NavPoint> list = Navigation.GetNavPoints();

            foreach (NavPoint p in list)
            {
                if (p == this) continue;

                var hit = Physics.LineTraceForStatic(Position.ToPhysics(), p.Position.ToPhysics());

                if (hit.HasHit) continue; 
                connected.Add(p);

            }

            staticConnected = new List<NavPoint>(connected);

        }

        void UpdateInternalConnected()
        {
            List<NavPoint> newConnected = new List<NavPoint>();


            foreach (NavPoint p in staticConnected)
            {
                if (p == this) continue;

                var hit = Physics.LineTrace(Position.ToPhysics(), p.Position.ToPhysics(), bodyType: PhysicsSystem.BodyType.World);

                if (hit.HasHit)
                {
                    //DrawDebug.Line(Position, p.Position);
                }

                if (hit.HasHit) continue;
                newConnected.Add(p);

            }

            connected = newConnected;

        }

        public List<Vector3> GetPathNext(List<NavPoint> history,List<NavPoint> blackList, Vector3 target, ref int totalItterations, PathfindingQuery query = null)
        {
            List<Vector3> output = new List<Vector3>();

            List<NavPoint> myHistory = new List<NavPoint>(history);

            totalItterations++;

            if (query != null)
            {
                if (query.deathDelay.Wait() == false)
                    return new List<Vector3> { target};
            }

            if (totalItterations > 300)
            {
                output = PointsToPositions(history);

                Vector3 closest = output.OrderByDescending(pos => Vector3.Distance(pos, target)).ToArray()[0];

                List<Vector3> result = new List<Vector3>();

                foreach (Vector3 p in output)
                {
                    result.Add(p);
                    if (Vector3.Distance(p, target) < 0.1f)
                        return result;
                }
            }

            if (history.Count>MaxDepth)
            {

                if(history.Count>2)
                {
                    blackList.Add(history[history.Count-1]);
                    return history[0].GetPathNext(new List<NavPoint>(), blackList, target, ref totalItterations, query);
                }
                else
                {
                    return new List<Vector3>();
                }


                
                
            }

            connected = connected.Where(p=> blackList.Contains(p) == false).OrderByDescending(point => Vector3.Dot((point.Position - Position).Normalized(), (target - point.Position).Normalized())).ToList();

            myHistory.Add(this);

            bool hited = false;

            var staticHit = Physics.LineTraceForStatic(Position.ToNumerics(), target.ToNumerics());

            var hit = staticHit;

            if (hit.HasHit == false)
                hit = Physics.LineTrace(Position.ToNumerics(), target.ToNumerics(), bodyType: PhysicsSystem.BodyType.World);

            hited = hit.HasHit;


            if (hited)
            {
                foreach(NavPoint point in connected)
                {
                    if (myHistory.Contains(point)) continue;

                    List<Vector3> result = point.GetPathNext(myHistory,blackList, target, ref totalItterations);

                    if(result.Count>0)
                    {
                        return result;
                    }
                }
            }
            else
            {
                output = PointsToPositions(myHistory);
                output.Add(target);
                return output;
            }

            return output;
        }

        List<Vector3> PointsToPositions(List<NavPoint> points)
        {
            List<Vector3> vectors = new List<Vector3>();

            foreach (NavPoint p in points)
            {
                vectors.Add(p.Position);
            }

            return vectors;

        }

    }
}
