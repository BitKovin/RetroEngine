using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Physics
{
    public static class PhysicsUtils
    {

        public static void SetPosition(this RigidBody body, Vector3 newPosition)
        {
            // Get the current orientation (rotation) of the body
            Quaternion currentRotation = Quaternion.Identity;

            // Create a new motion state with the updated position and current rotation
            Matrix newTransform = Matrix.Translation(newPosition) * Matrix.RotationQuaternion(currentRotation);
            DefaultMotionState newMotionState = new DefaultMotionState(newTransform);

            // Update the body's motion state to set the new position
            body.MotionState = newMotionState;

            // Update the body's world transform directly to apply the transformation
            body.WorldTransform = newTransform;
        }

        public static Vector3 ToPhysics(this Microsoft.Xna.Framework.Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Matrix ToPhysics(this Microsoft.Xna.Framework.Matrix matrix)
        {
            Matrix newM = new Matrix();

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
