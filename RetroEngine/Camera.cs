using Microsoft.Xna.Framework;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetroEngine
{

    public class Camera
    {

        public static float HtW;
        public static Vector3 position = new Vector3(0,0,0);
        public static Vector3 rotation = new Vector3(45,0,0);
        public static Matrix Transform;
        public static Matrix UiMatrix;

        public static Matrix world;
        public static Matrix view;
        public static Matrix projection;
        public static Matrix projectionOcclusion;
        public static Matrix projectionViewmodel;

        public static Matrix finalizedView;
        public static Matrix finalizedProjection;
        public static Matrix finalizedProjectionViewmodel;

        public static Vector3 finalizedPosition;
        public static Vector3 finalizedRotation;

        public static BoundingFrustum frustum = new BoundingFrustum(new Matrix());
        public static BoundingFrustum frustumOcclusion = new BoundingFrustum(new Matrix());

        public static float roll = 0;
        public static float FOV = 80;
        public static float ViewmodelFOV = 60;

        public static float FarPlane = 3000;

        public static Vector3 Up => rotation.GetUpVector();
        public static Vector3 Right => rotation.GetRightVector();
        public static Vector3 Forward => rotation.GetForwardVector();

        static Vector3 lastWorkingRotation = new Vector3();

        public static Vector3 velocity = new Vector3(0,0,0);

        public static List<CameraShake> CameraShakes = new List<CameraShake>();

        public static float GetHorizontalFOV()
        {
            return FOV*HtW;
        }

        public static void Update()
        {

            Camera.finalizedPosition = Camera.position;
            Camera.finalizedRotation = Camera.rotation;

            world = Matrix.CreateTranslation(Vector3.Zero);

            if(rotation.GetUpVector().RotateVector(rotation.GetForwardVector(), roll).Y > 0)
            {
                lastWorkingRotation = rotation;
                
            }
            else
            {
                rotation += new Vector3(0.0001f, 0.0001f, 0);
                //Logger.Log("Wrong camera up vector detected! Trying to fix");
            }

            StupidCameraFix();

            view = CalculateView();

            projection = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(FOV),HtW, 0.05f, FarPlane);

            projectionOcclusion = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(FOV*1.3f), HtW, 0.05f, FarPlane);

            projectionViewmodel = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(ViewmodelFOV), HtW, 0.01f, 1f);

            frustum.Matrix = view * projection;

            Camera.finalizedView = Camera.view;
            Camera.finalizedProjection = Camera.projection;
            Camera.finalizedProjectionViewmodel = Camera.projectionViewmodel;



        }

        public static void ApplyCameraShake()
        {

            rotation.Z = 0;

            foreach(var shake in CameraShakes.ToArray())
            {
                shake.Update(Time.DeltaTime);

                var result = shake.GetResult(position, rotation);

                position = result.position;
                rotation = result.rotation;

                if(shake.IsFinished())
                    CameraShakes.Remove(shake);

            }

            //roll += rotation.Z;

            rotation.Z = 0;

        }

        public static void AddCameraShake(CameraShake cameraShake)
        {
            lock (CameraShakes)
            {
                CameraShakes.Add(cameraShake);
            }
        }

        public static Matrix GetRotationMatrix()
        {

            StupidCameraFix();

            return Matrix.CreateRotationX(rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(rotation.Z / 180 * (float)Math.PI);
        }

        public static Matrix GetMatrix()
        {

            StupidCameraFix();

            return Matrix.CreateScale(1) *
                                Matrix.CreateRotationX(rotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(rotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(rotation.Z / 180 * (float)Math.PI) *
                                Matrix.CreateTranslation(position);
        }

        static void StupidCameraFix() // I FUCKING HATE IT, but it's only way I could fix wrong results after using matrix as result of GetMatrix(). It errored when rotation.Y was close to 90 or -90. 
        {
            if(rotation.X == 0)
                rotation.X = float.Epsilon;

            if (MathF.Abs(rotation.Y) % 90 < 0.0001f)
                rotation.Y += 0.0001f * ((rotation.Y>0) ? 1 : -1);

            if (rotation.Z == 0)
                rotation.Z = float.Epsilon;
        }

        public static Matrix CalculateView()
        {
            return Matrix.CreateLookAt(finalizedPosition, finalizedPosition + finalizedRotation.GetForwardVector(), finalizedRotation.GetUpVector().RotateVector(finalizedRotation.GetForwardVector(), roll));
        }

        public static void ViewportUpdate()
        {
            HtW = (float)GameMain.Instance.Window.ClientBounds.Width / (float)GameMain.Instance.Window.ClientBounds.Height;

            float ScaleY = (float)GameMain.Instance.Window.ClientBounds.Height / UiViewport.GetViewportHeight();

            var scale = Matrix.CreateScale(ScaleY, ScaleY, 1);

            UiMatrix = scale;
        }
        public static void Follow(Entity target)
        {
            position = target.Position;
        }
    }

    public class CameraShake
    {
        // Parameters
        private float duration; // Total duration of the shake
        private Vector3 positionAmplitude; // Max displacement for position on each axis (X, Y, Z)
        private Vector3 positionFrequency; // Oscillation frequency for position on each axis (X, Y, Z)
        private Vector3 rotationAmplitude; // Max displacement for rotation on each axis (Pitch, Yaw, Roll)
        private Vector3 rotationFrequency; // Oscillation frequency for rotation on each axis (Pitch, Yaw, Roll)
        private float falloff; // Intensity reduction over time
        private float elapsedTime; // Tracks the elapsed time since the shake started

        private float interpIn;

        private Vector3 currentOffsetPosition; // Stores the current position offset
        private Vector3 currentOffsetRotation; // Stores the current rotation offset

        // Shake type
        public enum ShakeType { SingleWave, PerlinNoise }
        private ShakeType shakeType;

        // Constructor with default values
        public CameraShake(float interpIn = 0.2f,
            float duration = 1f,
            Vector3? positionAmplitude = null,
            Vector3? positionFrequency = null,
            Vector3? rotationAmplitude = null,
            Vector3? rotationFrequency = null,
            float falloff = 1f,
            ShakeType shakeType = ShakeType.SingleWave)
        {
            this.interpIn = interpIn;
            this.duration = duration;
            this.positionAmplitude = positionAmplitude ?? new Vector3(1f, 1f, 1f);
            this.positionFrequency = positionFrequency ?? new Vector3(10f, 10f, 10f);
            this.rotationAmplitude = rotationAmplitude ?? new Vector3(1f, 1f, 1f);
            this.rotationFrequency = rotationFrequency ?? new Vector3(10f, 10f, 10f);
            this.falloff = falloff;
            this.elapsedTime = 0f;
            this.currentOffsetPosition = Vector3.Zero;
            this.currentOffsetRotation = Vector3.Zero;
            this.shakeType = shakeType;
        }


        float intensity;
        // Update function
        public void Update(float deltaTime)
        {
            // If the shake is over, do nothing
            if (elapsedTime >= duration)
            {
                currentOffsetPosition = Vector3.Zero;
                currentOffsetRotation = Vector3.Zero;
                return;
            }

            // Calculate the normalized time (0 to 1)
            float normalizedTime = elapsedTime / duration;


            float interpTime = MathHelper.Saturate(elapsedTime / interpIn);

            // Calculate the falloff multiplier
            intensity = MathHelper.Lerp(1f, 0f, normalizedTime) * (1f - normalizedTime * falloff) * interpTime;



            if (shakeType == ShakeType.SingleWave)
            {
                // Single wave offsets
                currentOffsetPosition = new Vector3(
                    intensity * positionAmplitude.X * (float)Math.Sin(elapsedTime * positionFrequency.X),
                    intensity * positionAmplitude.Y * (float)Math.Sin(elapsedTime * positionFrequency.Y),
                    intensity * positionAmplitude.Z * (float)Math.Sin(elapsedTime * positionFrequency.Z)
                );

                currentOffsetRotation = new Vector3(
                    intensity * rotationAmplitude.X * (float)Math.Cos(elapsedTime * rotationFrequency.X),
                    intensity * rotationAmplitude.Y * (float)Math.Cos(elapsedTime * rotationFrequency.Y),
                    intensity * rotationAmplitude.Z * (float)Math.Cos(elapsedTime * rotationFrequency.Z)
                );
            }
            else if (shakeType == ShakeType.PerlinNoise)
            {
                // Perlin noise offsets
                currentOffsetPosition = new Vector3(
                    intensity * positionAmplitude.X * PerlinNoise(elapsedTime * positionFrequency.X),
                    intensity * positionAmplitude.Y * PerlinNoise(elapsedTime * positionFrequency.Y),
                    intensity * positionAmplitude.Z * PerlinNoise(elapsedTime * positionFrequency.Z)
                );

                currentOffsetRotation = new Vector3(
                    intensity * rotationAmplitude.X * PerlinNoise(elapsedTime * rotationFrequency.X),
                    intensity * rotationAmplitude.Y * PerlinNoise(elapsedTime * rotationFrequency.Y),
                    intensity * rotationAmplitude.Z * PerlinNoise(elapsedTime * rotationFrequency.Z)
                );
            }

            // Increment elapsed time
            elapsedTime += deltaTime;
        }

        // Get the result camera position and rotation
        public (Vector3 position, Vector3 rotation) GetResult(Vector3 currentPosition, Vector3 currentRotation)
        {
            // Apply offsets to the position and rotation
            Vector3 newPosition = currentPosition +
                                  (currentOffsetPosition.X * currentRotation.GetRightVector() +
                                   currentOffsetPosition.Y * currentRotation.GetUpVector() +
                                   currentOffsetPosition.Z * currentRotation.GetForwardVector());

            Vector3 newRotation = currentRotation + currentOffsetRotation;

            return (newPosition, newRotation);
        }

        // Resets the shake timer
        public void Reset()
        {
            elapsedTime = 0f;
            currentOffsetPosition = Vector3.Zero;
            currentOffsetRotation = Vector3.Zero;
        }

        // Checks if the shake is finished
        public bool IsFinished()
        {
            return elapsedTime >= duration;
        }

        // Simple Perlin noise implementation (placeholder)
        private float PerlinNoise(float input)
        {
            return (float)(Math.Sin(input) * 0.5 + 0.5); // Replace with actual Perlin noise logic if needed
        }
    }


}
