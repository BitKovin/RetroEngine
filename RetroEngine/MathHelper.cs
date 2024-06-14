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
            float epsilon = 0.00001f;
            rot.X = Math.Abs(rot.X) < epsilon ? epsilon : rot.X;

            double X = Math.Sin(rot.Y * Math.PI / 180.0);
            double Y = -Math.Tan((rot.X + 90.0) * Math.PI / 180.0);
            double Z = Math.Cos(rot.Y * Math.PI / 180.0);

            Vector3 rotation = new Vector3((float)X, (float)Y, (float)Z);
            rotation = Vector3.Normalize(rotation);

            // Ensure correct direction of the up vector
            if (rot.X >= 0)
                rotation = -rotation;

            return -rotation;
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

        public static float Saturate(float a)
        {
            return Math.Clamp(a, 0f, 1f);
        }

        public static Vector3 RotateVector(this Vector3 vector, Vector3 axis, float angleInDegrees)
        {
            float angleInRadians = MathHelper.ToRadians(angleInDegrees);

            // Create a rotation matrix around the given axis
            Matrix rotationMatrix = Matrix.CreateFromAxisAngle(axis, angleInRadians);

            // Apply the rotation matrix to the vector
            return Vector3.Transform(vector, rotationMatrix);
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

        public static Vector3 ToEulerAnglesDegrees(this Quaternion q)
        {
            Vector3 angles = new();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            angles.X = ToDegrees(angles.X);
            angles.Y = ToDegrees(angles.Y);
            angles.Z = ToDegrees(angles.Z);

            return angles;
        }


        public struct Transform
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public Quaternion RotationQuaternion;
            public Vector3 Scale;

            public Transform()
            {
                Position = new Vector3();
                Rotation = new Vector3();
                RotationQuaternion = new Quaternion();
                Scale = new Vector3(1);
            }

            public static Transform Lerp(Transform a, Transform b, float factor)
            {
                Transform result = new Transform();

                result.Position = Vector3.Lerp(a.Position, b.Position, factor);
                result.RotationQuaternion = Quaternion.Lerp(a.RotationQuaternion, b.RotationQuaternion, factor);
                result.Rotation = result.RotationQuaternion.ToEulerAnglesDegrees();
                result.Scale = Vector3.Lerp(a.Scale, b.Scale, factor);

                return result;
            }

            public override string ToString()
            {
                return $"position: {Position} \nrotation: {Rotation}\n rotationQ: {RotationQuaternion}\n Scale:{Scale}";
            }

        }

        public static float NormalizeAngle(float angle)
        {

            float a = angle;

            while (a < -180)
                a += 360;

            while (a > 180)
                a -= 360;

            return a;
        }

        public static float NonZeroAngle(float angle)
        {
            if (angle % 90 == 0)
            {
                angle += 0.0001f;
            }

            return angle;
        }

        public static Vector3 NormalizeAngles(this Vector3 a)
        {
            a.X = NormalizeAngle(a.X);
            a.Y = NormalizeAngle(a.Y);
            a.Z = NormalizeAngle(a.Z);
            return a;
        }

        public static Vector3 NonZeroAngles(this Vector3 a)
        {
            a.X = NonZeroAngle(a.X);
            a.Y = NonZeroAngle(a.Y);
            a.Z = NonZeroAngle(a.Z);

            return a;
        }

        public static Vector4 GetRow(this Matrix matrix, int row)
        {
            return new Vector4(matrix[row, 0], matrix[row, 1], matrix[row, 2], matrix[row, 3]);
        }

        public static Transform DecomposeMatrix(this Matrix matrix)
        {
            Vector3 position, scale, rotationDegrees;
            Quaternion rotation;

            if(matrix.M41 is float.NaN)
                return new Transform();

            matrix.Decompose(out scale, out rotation, out position);

            rotationDegrees = ToEulerAnglesDegrees(rotation);

            rotationDegrees.X = NormalizeAngle(rotationDegrees.X);
            rotationDegrees.Y = NormalizeAngle(rotationDegrees.Y);
            rotationDegrees.Z = NormalizeAngle(rotationDegrees.Z);

            return new Transform
            {
                Position = position,
                Rotation = rotationDegrees, 
                RotationQuaternion = rotation,
                Scale = scale
            };
        }

        public static Matrix ToMatrix(this Transform transform)
        {

            transform.Rotation = transform.Rotation.NonZeroAngles();

            return Matrix.CreateScale(transform.Scale) *
                                Matrix.CreateRotationX(transform.Rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(transform.Rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(transform.Rotation.Z / 180 * (float)Math.PI) *
                                Matrix.CreateTranslation(transform.Position);
        }


    }
}
