using Microsoft.Xna.Framework;
using RetroEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{
    internal class FreeCamera : Entity
    {

        public static bool active = false;

        static float speed = 5;

        static FreeCamera instance;

        public FreeCamera() 
        {
        }

        public override void Destroy()
        {
            base.Destroy();

            active = false;

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            Camera.rotation += new Vector3(Input.MouseDelta.Y, -Input.MouseDelta.X, 10) * 1f;

            Camera.rotation = new Vector3(Math.Clamp(Camera.rotation.X, -89, 89), Camera.rotation.Y, 0);

            Vector2 input = new Vector2();

            if (Input.GetAction("moveForward").Holding())
                input += new Vector2(0, 1);

            if (Input.GetAction("moveBackward").Holding())
                input -= new Vector2(0, 1);

            if (Input.GetAction("moveRight").Holding())
                input += new Vector2(1, 0);

            if (Input.GetAction("moveLeft").Holding())
                input -= new Vector2(1, 0);


            Camera.position += (Camera.rotation.GetForwardVector() * input.Y + Camera.rotation.GetRightVector() * input.X)*Time.DeltaTime * speed;


        }

        [ConsoleCommand("freecam")]
        public static void StartFreecam()
        {
            if(active)
            {
                instance.Destroy();
                active = false;
                Logger.Log("free camera destroyed");
            }else
            {
                active = true;
                instance = Level.GetCurrent().AddEntity(new FreeCamera()) as FreeCamera;
                Logger.Log("free camera created");
            }
        }

        [ConsoleCommand("freecam.speed")]
        public static void SetCameraSpeed(string value)
        {

            if (float.TryParse(value.Replace(" ", ""), CultureInfo.InvariantCulture, out float val) == false)
            {
                Logger.Log("wrong formating: " + value);
                return;
            }


            speed = val;
        }

    }
}
