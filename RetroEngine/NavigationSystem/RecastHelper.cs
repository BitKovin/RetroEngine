using DotRecast.Core.Numerics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.NavigationSystem
{
    public static class RecastHelper
    {

        public static RcVec3f ToRc(this Vector3 vector)
        {
            return new RcVec3f(vector.X, vector.Y, vector.Z);
        }

        public static Vector3 FromRc(this RcVec3f vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);   
        }

        public static List<Vector3> ConvertPath(this List<RcVec3f> l)
        {

            List<Vector3> res = new List<Vector3>();
            res.Capacity = l.Count;

            foreach (RcVec3f r in l)
            {
                res.Add(r.FromRc());
            }

            return res;

        }

    }
}
