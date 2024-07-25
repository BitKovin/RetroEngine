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
        public static Vector3 finalizedForward;

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

        public static float GetHorizontalFOV()
        {
            return FOV*HtW;
        }

        public static void Update()
        {

            //rotation = new Vector3(Input.MousePos.Y, Input.MousePos.X, 0);
            //position = new Vector3(0, 0, 0);
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

            view = CalculateView();

            projection = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(FOV),HtW, 0.05f, FarPlane);

            projectionOcclusion = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(FOV*1.3f), HtW, 0.05f, FarPlane);

            projectionViewmodel = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(ViewmodelFOV), HtW, 0.01f, 1f);

            frustum.Matrix = view * projection;

        }

        public static Matrix CalculateView()
        {
            return Matrix.CreateLookAt(position, position + rotation.GetForwardVector(), lastWorkingRotation.GetUpVector().RotateVector(rotation.GetForwardVector(), roll));
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
}
