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

        public static float ToDegrees(float radians)
        {
            return radians * (180.0f / (float)Math.PI);
        }

        public static float ToRadians(float degrees)
        {
            return (float)((double)degrees * (Math.PI / 180.0));
        }

        public static Vector3 FindLookAtRotation(Vector3 source, Vector3 target)
        {
            Vector3 direction = target - source;

            float yaw = ToDegrees((float)Math.Atan2(direction.X, direction.Z));
            float pitch = ToDegrees((float)-Math.Asin(direction.Y / direction.Length()));

            return new Vector3(pitch, yaw, 0);
        }

        public static Vector3 ToEulerAnglesDegrees(this Quaternion quaternion)
        {
            float pitch, yaw, roll;

            // Calculate pitch (x-axis rotation)
            float sinPitchCosYaw = 2 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            float cosPitchCosYaw = 1 - 2 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
            pitch = MathHelper.ToDegrees((float)Math.Atan2(sinPitchCosYaw, cosPitchCosYaw));

            // Calculate yaw (y-axis rotation)
            float sinYaw = 2 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            if (Math.Abs(sinYaw) >= 1)
            {
                yaw = MathHelper.ToDegrees((float)Math.CopySign(Math.PI / 2, sinYaw)); // use 90 degrees if out of range
            }
            else
            {
                yaw = MathHelper.ToDegrees((float)Math.Asin(sinYaw));
            }

            // Calculate roll (z-axis rotation)
            float sinRollCosYaw = 2 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            float cosRollCosYaw = 1 - 2 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
            roll = MathHelper.ToDegrees((float)Math.Atan2(sinRollCosYaw, cosRollCosYaw));

            return new Vector3(pitch, yaw, roll);
        }


        public struct Transform
        {
            public Vector3 Position;
            public Vector3 Rotation; // Now in degrees
            public Vector3 Scale;

            public Transform()
            {
                Position = new Vector3();
                Rotation = new Vector3();
                Scale = new Vector3(1);
            }

        }

        public static Transform DecomposeMatrix(this Matrix matrix)
        {
            Vector3 position, scale, rotationDegrees;
            Quaternion rotation;

            matrix.Decompose(out scale, out rotation, out position);

            rotationDegrees = MathHelper.ToEulerAnglesDegrees(rotation);

            return new Transform
            {
                Position = position,
                Rotation = rotationDegrees, // Store as degrees
                Scale = scale
            };
        }

        public static Matrix ToMatrix(this Transform transform)
        {
            return Matrix.CreateScale(transform.Scale) *
                                Matrix.CreateRotationX(transform.Rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(transform.Rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(transform.Rotation.Z / 180 * (float)Math.PI) *
                                Matrix.CreateTranslation(transform.Position);
        }


    }
}
