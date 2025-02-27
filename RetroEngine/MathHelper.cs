using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetroEngine
{
    public static class MathHelper
    {

        public static Vector3 GetForwardVector(this Vector3 rot)
        {
            return Vector3.Transform(Vector3.UnitZ, rot.GetRotationQuaternion());
        }

        public static Vector3 GetRightVector(this Vector3 rot)
        {

            return Vector3.Transform(-Vector3.UnitX, rot.GetRotationQuaternion());
        }

        public static Vector3 GetUpVector(this Vector3 rot)
        {
            return Vector3.Transform(Vector3.UnitY, rot.GetRotationQuaternion());
        }

        public static Vector3 XZ(this Vector3 vector)
        {
            return new Vector3(vector.X, 0, vector.Z);
        }

        public static float InvSqrt(float x)
        {
            float xhalf = 0.5f * x;
            int i = BitConverter.SingleToInt32Bits(x);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.Int32BitsToSingle(i);
            x = x * (1.5f - xhalf * x * x);
            return x;
        }
        public static Vector3 FastNormalize(this Vector3 value)
        {
            float num = InvSqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
            return value * num;
        }
        public static Vector3 Normalized (this Vector3 vector)
        {
            return vector.FastNormalize();
        }

        /// <summary>
        /// Snaps a Vector3 to the nearest grid position.
        /// </summary>
        /// <param name="position">The position to snap.</param>
        /// <param name="gridSize">The size of the grid cells.</param>
        /// <returns>The snapped Vector3 position.</returns>
        public static Vector3 SnapToGrid(this Vector3 position, float gridSize)
        {
            if (gridSize <= 0)
                throw new ArgumentException("Grid size must be greater than zero.", nameof(gridSize));

            float snappedX = (float)Math.Ceiling(position.X / gridSize) * gridSize;
            float snappedY = (float)Math.Ceiling(position.Y / gridSize) * gridSize;
            float snappedZ = (float)Math.Ceiling(position.Z / gridSize) * gridSize;

            return new Vector3(snappedX, snappedY, snappedZ);
        }

        public static Vector3 SnapToGrid(this Vector3 position, Vector3 gridSize)
        {


            float snappedX = (float)Math.Floor(position.X / gridSize.X) * gridSize.X;
            float snappedY = (float)Math.Floor(position.Y / gridSize.Y) * gridSize.Y;
            float snappedZ = (float)Math.Floor(position.Z / gridSize.Z) * gridSize.Z;

            return new Vector3(snappedX, snappedY, snappedZ);
        }

        public static Vector3 Clamp(this Vector3 vector, float min, float max)
        {
            return new Vector3(Math.Clamp(vector.X, min, max), Math.Clamp(vector.Y, min, max), Math.Clamp(vector.Z, min, max));
        }

        public static Vector4 Clamp(this Vector4 vector, float min, float max)
        {
            return new Vector4(Math.Clamp(vector.X, min, max), Math.Clamp(vector.Y, min, max), Math.Clamp(vector.Z, min, max), Math.Clamp(vector.W, min, max));
        }

        public static float Lerp(float a, float b, float progress)
        {
            return Vector2.Lerp(new Vector2(a), new Vector2(b), progress).X;
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 progress)
        {
            return new Vector3(float.Lerp(a.X, b.X, progress.X),
                float.Lerp(a.Y, b.Y, progress.Y),
                float.Lerp(a.Z, b.Z, progress.Z));
        }

        public static float Saturate(float a)
        {
            return Math.Clamp(a, 0f, 1f);
        }

        public static bool HasNan(this Matrix matrix)
        {
            if (float.IsNaN(matrix.M11))
                return true;
            if (float.IsNaN(matrix.M12))
                return true;
            if (float.IsNaN(matrix.M13))
                return true;
            if (float.IsNaN(matrix.M14))
                return true;

            if (float.IsNaN(matrix.M21))
                return true;
            if (float.IsNaN(matrix.M22))
                return true;
            if (float.IsNaN(matrix.M23))
                return true;
            if (float.IsNaN(matrix.M24))
                return true;

            if (float.IsNaN(matrix.M31))
                return true;
            if (float.IsNaN(matrix.M32))
                return true;
            if (float.IsNaN(matrix.M33))
                return true;
            if (float.IsNaN(matrix.M34))
                return true;

            if (float.IsNaN(matrix.M41))
                return true;
            if (float.IsNaN(matrix.M42))
                return true;
            if (float.IsNaN(matrix.M43))
                return true;
            if (float.IsNaN(matrix.M44))
                return true;

            return false;

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

        public static Vector3 ToYawPitchRoll(this Quaternion q)
        {
            // Yaw (rotation about Y-axis)
            double siny = 2.0 * (q.W * q.Y + q.Z * q.X);
            double cosy = 1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            double yaw = Math.Atan2(siny, cosy);

            // Pitch (rotation about X-axis)
            double sinp = 2.0 * (q.W * q.X - q.Y * q.Z);
            double pitch = Math.Abs(sinp) >= 1
                ? Math.CopySign(Math.PI / 2, sinp)  // use 90° if out of range
                : Math.Asin(sinp);

            // Roll (rotation about Z-axis)
            double sinr = 2.0 * (q.W * q.Z + q.X * q.Y);
            double cosr = 1.0 - 2.0 * (q.X * q.X + q.Z * q.Z);
            double roll = Math.Atan2(sinr, cosr);

            // Return Vector3 where:
            // X = yaw, Y = pitch, Z = roll
            return new Vector3(MathHelper.ToDegrees((float)pitch), MathHelper.ToDegrees((float)yaw), MathHelper.ToDegrees((float)roll));
        }

        public static Vector3 FindLookAtRotation(Vector3 source, Vector3 target)
        {
            Vector3 direction = target - source;

            float yaw = ToDegrees((float)Math.Atan2(direction.X, direction.Z));
            float pitch = ToDegrees((float)-Math.Asin(direction.Y / direction.Length()));

            return new Vector3(pitch, yaw, 0);
        }

        /// <summary>
        /// Calculates the angle of rotation required to look at a target point.
        /// </summary>
        /// <param name="origin">The starting point.</param>
        /// <param name="target">The point to look at.</param>
        /// <returns>Angle in radians required to look at the target point.</returns>
        public static float FindLookAtRotation(Vector2 origin, Vector2 target)
        {
            // Get the direction vector from the origin to the target
            Vector2 direction = target - origin;

            // Calculate the angle of the direction vector
            float angle = (float)Math.Atan2(direction.Y, direction.X);

            return angle;
        }

        public static Matrix GetRotationMatrix(this Vector3 rotation)
        {

            float yaw = MathHelper.ToRadians(rotation.Y);
            float pitch = MathHelper.ToRadians(rotation.X);
            float roll = MathHelper.ToRadians(rotation.Z);

            return Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        public static Quaternion GetRotationQuaternion(this Vector3 rotation)
        {

            float yaw = MathHelper.ToRadians(rotation.Y);
            float pitch = MathHelper.ToRadians(rotation.X);
            float roll = MathHelper.ToRadians(rotation.Z);

            return Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        }
        /*
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
        */
        /// <summary>
        /// Converts a world transformation matrix into a local transformation matrix relative to another world transformation matrix.
        /// </summary>
        /// <param name="worldTransform">The world transformation matrix to be converted.</param>
        /// <param name="relativeToWorldTransform">The world transformation matrix to which the local transformation will be relative.</param>
        /// <returns>The local transformation matrix.</returns>
        public static Matrix WorldToLocal(Matrix worldTransform, Matrix relativeToWorldTransform)
        {
            // Invert the relative world transformation matrix
            Matrix inverseRelativeWorld = Matrix.Invert(relativeToWorldTransform);

            // Multiply the world transformation by the inverse of the relative world transformation
            Matrix localTransform = worldTransform * inverseRelativeWorld;

            return localTransform;
        }

        public static float CalculateMinDistance(Vector3 center, IList<Vector3> points)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Points list cannot be null or empty.");

            return points.Min(p => Vector3.Distance(center, p));
        }

        public static float CalculateMaxDistance(Vector3 center, IList<Vector3> points)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Points list cannot be null or empty.");

            return points.Max(p => Vector3.Distance(center, p));
        }

        public struct Transform
        {
            public Vector3 Position = Vector3.Zero;
            public Vector3 Rotation = Vector3.Zero;
            public Quaternion RotationQuaternion = Quaternion.Identity;
            public Vector3 Scale = Vector3.One;

            

            public Transform()
            {

            }

            public static Transform Lerp(Transform a, Transform b, float factor)
            {
                Transform result = new Transform();

                result.Position = Vector3.Lerp(a.Position, b.Position, factor);
                result.RotationQuaternion = Quaternion.Lerp(a.RotationQuaternion, b.RotationQuaternion, factor);
                result.Rotation = result.RotationQuaternion.ToYawPitchRoll();
                result.Scale = Vector3.Lerp(a.Scale, b.Scale, factor);

                return result;
            }

            public static Transform operator +(Transform a, Transform b)
            {
                return new Transform
                {
                    Position = a.Position + b.Position,
                    Rotation = a.Rotation + b.Rotation,
                    RotationQuaternion = a.RotationQuaternion * b.RotationQuaternion,
                    Scale = a.Scale + b.Scale
                };
            }

            public static Transform operator -(Transform a, Transform b)
            {
                return new Transform
                {
                    Position = a.Position - b.Position,
                    Rotation = a.Rotation - b.Rotation,
                    RotationQuaternion = a.RotationQuaternion * Quaternion.Inverse(b.RotationQuaternion),
                    Scale = a.Scale - b.Scale
                };
            }

            public static Transform operator *(Transform a, Transform b)
            {
                return new Transform
                {
                    Position = a.Position * b.Position,
                    Rotation = a.Rotation * b.Rotation,
                    RotationQuaternion = a.RotationQuaternion * Quaternion.Inverse(b.RotationQuaternion),
                    Scale = a.Scale * b.Scale
                };
            }

            public static Transform operator -(Transform a)
            {
                return new Transform
                {
                    Position = -a.Position,
                    Rotation = -a.Rotation,
                    RotationQuaternion = -a.RotationQuaternion,
                    Scale = -a.Scale
                };
            }


            public Transform RotateWithMatrix(Transform transform)
            {
                return new Transform
                {
                    Position = Position,
                    Rotation = Rotation,
                    RotationQuaternion = RotationQuaternion,
                    Scale = Scale
                };
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

            if(float.IsNaN(matrix.M41))
                return new Transform();

            matrix.Decompose(out scale, out rotation, out position);

            rotationDegrees = rotation.ToYawPitchRoll();

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
                                transform.Rotation.GetRotationMatrix() *
                                Matrix.CreateTranslation(transform.Position);
        }


    }
}
