using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.NavigationSystem
{
    public class DynamicObstacleHelper
    {

        public List<StaticMesh> Meshes;

        public long[] obstacles = new long[8];
        bool updated = false;

        bool destroyed = false;

        public void Update()
        {
            if(destroyed) return;

            if (Meshes == null)
                return;

            foreach (StaticMesh mesh in Meshes)
            {

                var boxes = mesh.GetSubdividedBoundingBoxes();

                int i = 0;
                foreach (var box in boxes)
                {
                    if(updated)
                        Recast.RemoveObstacle(obstacles[i]);

                    

                    obstacles[i] = Recast.AddObstacleBox(box.Min - Vector3.UnitY*2f, box.Max);

                    //DrawDebug.Box(box.Min, box.Max, Vector3.Zero, 0.01f);

                    i++;
                }
            }

            updated = true;

        }

        public void DebugDraw()
        {
            if (destroyed) return;

            if (Meshes == null)
                return;

            foreach (StaticMesh mesh in Meshes)
            {

                var boxes = mesh.GetSubdividedBoundingBoxes();

                foreach (var box in boxes)
                {
                    DrawDebug.Box(box.Min - Vector3.UnitY * 2f, box.Max, Vector3.Zero, 0.01f);
                }
            }

            updated = true;

        }

        public void Destroy()
        {
            if(updated)
                foreach(int o in obstacles)
                    Recast.RemoveObstacle(o);

            destroyed = true;
            Meshes = null;
            obstacles = null;

        }

    }
}
