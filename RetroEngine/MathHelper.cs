using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetroEngine
{
    public static class MathHelper
    {
        public static Vector3 GetForwardVector(this Vector3 rot)
        {
            double X = Math.Sin(rot.Y / 180d * Math.PI);
            double Y = -Math.Tan(rot.X / 180d * Math.PI);
            double Z = Math.Cos(rot.Y / 180d * Math.PI);

            Vector3 vector = new Vector3((float)X, (float)Y, (float)Z);

            vector = Vector3.Normalize(vector);

            return vector;
        }

        public static Vector3 GetRightVector(this Vector3 rot)
        {
            double X = Math.Sin((rot.Y + 90) / 180d * Math.PI);
            double Y = 0;//-Math.Tan(rot.X / 180d * Math.PI);
            double Z = Math.Cos((rot.Y + 90) / 180d * Math.PI);
            
            Vector3 rotation = new Vector3((float)X, (float)Y, (float)Z);
            
            rotation = Vector3.Normalize(rotation);

            return rotation * -1f;
        }

        public static Vector3 GetUpVector(this Vector3 rot)
        {
            if (rot.X == 0)
                rot.X = 0.00001f;

            double X = Math.Sin((rot.Y) / 180d * Math.PI);
            double Y = -Math.Tan((rot.X+90f) / 180d * Math.PI);
            double Z = Math.Cos((rot.Y) / 180d * Math.PI);

            Vector3 rotation = new Vector3((float)X, (float)Y, (float)Z);
            rotation = Vector3.Normalize(rotation);

            if(rot.X>=0)
                rotation = rotation * -1f;

            return rotation * -1f;
        }

        public static Vector3 XZ(this Vector3 vector)
        {
            return new Vector3(vector.X, 0, vector.Z);
        }

        public static Vector3 Normalized (this Vector3 vector)
        {
            vector.Normalize();
            return vector;
        }

        public static float Lerp(float a, float b, float progress)
        {
            return Vector2.Lerp(new Vector2(a), new Vector2(b), progress).X;
        }
        public static Vector3 RotateVector(this Vector3 vector, Vector3 axis, float angleInDegrees)
        {
            // Convert angle to radians
            float angleInRadians = angleInDegrees/180f * 3.141f;

            // Create a rotation matrix
            Matrix rotationMatrix = Matrix.CreateFromAxisAngle(axis, angleInRadians);

            // Apply the rotation matrix to the vector
            Vector3 rotatedVector = Vector3.Transform(vector, rotationMatrix);

            return rotatedVector;
        }

    }
}
