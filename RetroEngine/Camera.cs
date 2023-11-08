using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
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

        public static float FOV = 60;

        public static float GetHorizontalFOV()
        {
            return FOV*HtW;
        }

        public static void Update()
        {
            HtW = GameMain.inst.Window.ClientBounds.Width / GameMain.inst.Window.ClientBounds.Height;

            float ScaleY = (float)GameMain.inst.Window.ClientBounds.Height / Constants.ResoultionY;
            var scale = Matrix.CreateScale(ScaleY * HtW, ScaleY, 1);

            UiMatrix = scale;

            //rotation = new Vector3(Input.MousePos.Y, Input.MousePos.X, 0);
            //position = new Vector3(0, 0, 0);
            world = Matrix.CreateTranslation(Vector3.Zero);
            view = Matrix.CreateLookAt(position, position + rotation.GetForwardVector(), Vector3.UnitY);
            projection = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(FOV), (float)GameMain.inst.Window.ClientBounds.Width / (float)GameMain.inst.Window.ClientBounds.Height, 0.01f, 1000000f); 

        }

        public static void Follow(Entity target)
        {
            position = target.Position;
        }
    }
}
