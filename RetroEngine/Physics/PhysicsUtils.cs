using BulletSharp;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public static class PhysicsUtils
    {

        public static void SetPosition(this RigidBody body, Vector3 newPosition)
        {
            System.Numerics.Matrix4x4 rotationMatrix = body.WorldTransform.GetBasis();
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);

            // Create a new motion state with the updated position and current rotation
            Matrix newTransform = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(newPosition);


            lock (body) lock (Physics.dynamicsWorld)
                {
                    // Update the body's world transform directly to apply the transformation
                    body.WorldTransform = newTransform.ToNumerics();
                }
        }

        public static void SetRotation(this RigidBody body, Quaternion newRotation)
        {

            // Create a new motion state with the updated position and current rotation
            Matrix newTransform = Matrix.CreateFromQuaternion(newRotation) * Matrix.CreateTranslation(body.WorldTransform.Translation);

            lock (body) lock (Physics.dynamicsWorld)
                {
                    // Update the body's world transform directly to apply the transformation
                    body.WorldTransform = newTransform.ToNumerics();
                    body.MotionState.WorldTransform = newTransform.ToNumerics();
                }
        }

        public static void SetRotation(this RigidBody body, Vector3 Rotation)
        {
            SetRotation(body, Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationX(Rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(Rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(Rotation.Z / 180 * (float)Math.PI)));
        }

        public static System.Numerics.Vector3 ToPhysics(this Microsoft.Xna.Framework.Vector3 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static System.Numerics.Matrix4x4 ToPhysics(this Microsoft.Xna.Framework.Matrix matrix)
        {
            System.Numerics.Matrix4x4 newM = new System.Numerics.Matrix4x4();

            newM.M11 = matrix.M11;
            newM.M12 = matrix.M12;
            newM.M13 = matrix.M13;
            newM.M14 = matrix.M14;
            newM.M21 = matrix.M21;
            newM.M22 = matrix.M22;
            newM.M23 = matrix.M23;
            newM.M24 = matrix.M24;
            newM.M31 = matrix.M31;
            newM.M32 = matrix.M32;
            newM.M33 = matrix.M33;
            newM.M34 = matrix.M34;
            newM.M41 = matrix.M41;
            newM.M42 = matrix.M42;
            newM.M43 = matrix.M43;
            newM.M44 = matrix.M44;

            return newM;
        }

        public static Microsoft.Xna.Framework.Vector3 FromPhysics(this Vector3 vector)
        {
            return new Microsoft.Xna.Framework.Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
        }

    }
}
