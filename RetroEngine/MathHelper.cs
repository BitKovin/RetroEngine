using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public static class MathHelper
    {
        public static Vector3 GetForwardVector(this Vector3 rot)
        {
            double X = Math.Sin(rot.Y / 180d * Math.PI);
            double Y = -Math.Tan(rot.X / 180d * Math.PI);
            double Z = Math.Cos(rot.Y / 180d * Math.PI);

            Vector3 vector = new Vector3((float)X, (float)Y, (float)Z);

            vector.Normalize();

            return vector;
        }

        public static Vector3 GetRightVector(this Vector3 rot)
        {
            double X = Math.Sin((rot.Y + 90) / 180d * Math.PI);
            double Y = 0;//-Math.Tan(rot.X / 180d * Math.PI);
            double Z = Math.Cos((rot.Y + 90) / 180d * Math.PI);
            
            Vector3 rotation = new Vector3((float)X, (float)Y, (float)Z);
            rotation.Normalize();
            return rotation * -1f;
        }

        public static Vector3 GetUpVector(this Vector3 rot)
        {
            if (rot.X > 0)
            {
                return (rot + new Vector3(89.9f, 0, 0)).GetForwardVector();
            }else
            {
                return (rot + new Vector3(89.9f, 0, 0)).GetForwardVector() * -1f;
            }
        }

        public static Vector3 XZ(this Vector3 vector)
        {
            return new Vector3(vector.X, 0, vector.Z);
        }

        

    }
}
