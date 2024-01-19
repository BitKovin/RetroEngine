using Microsoft.Xna.Framework;
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

        public List<NavPoint> connected = new List<NavPoint>();

        int MaxDepth = 12;

        public override void Start()
        {
            base.Start();

            SnapToGround();

            Navigation.AddPoint(this);
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

                var hit = Physics.LineTraceForStatic(Position.ToNumerics(), p.Position.ToNumerics());

                if (hit.HasHit) continue; 
                connected.Add(p);

            }

        }

        public List<Vector3> GetPathNext(List<NavPoint> history, Vector3 target, ref int totalItterations)
        {
            List<Vector3> output = new List<Vector3>();

            List<NavPoint> myHistory = new List<NavPoint>(history);

            totalItterations++;

            if (totalItterations > 40)
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
                return new List<Vector3>();
                output = PointsToPositions(history);

                

                Vector3 closest = output.OrderByDescending(pos => Vector3.Distance(pos, target)).ToArray()[0];

                List <Vector3> result = new List<Vector3>();

                foreach (Vector3 p in output)
                {
                    result.Add(p);
                    if (Vector3.Distance(p, target) < 0.1f) 
                        return result;
                }
            }

            connected = connected.OrderByDescending(point => Vector3.Dot((point.Position - Position).Normalized(), (target - point.Position).Normalized())).ToList();

            myHistory.Add(this);

            var hit = Physics.SphereTraceForStatic(Position.ToNumerics(), target.ToNumerics(),0.3f);

            

            if (hit.HasHit)
            {
                foreach(NavPoint point in connected)
                {
                    if (myHistory.Contains(point)) continue;

                    List<Vector3> result = point.GetPathNext(myHistory, target, ref totalItterations);

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
