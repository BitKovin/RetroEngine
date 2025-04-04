﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RetroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkeletalMeshEditor
{
    internal class FreeCamera : Entity
    {

        static int OldScrollValue = 0;

        static float Speed = 5;

        public override void LateUpdate()
        {
            base.LateUpdate();

            if(Input.GetAction("rmb").Holding())
            Camera.rotation += new Vector3(Input.MouseDelta.Y, -Input.MouseDelta.X, 10) * 2f;

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

            Camera.position += (Camera.rotation.GetForwardVector() * input.Y + Camera.rotation.GetRightVector() * input.X)*Time.DeltaTime * Speed;

            UpdateScroll();

        }

        Delay scrollDelay = new Delay();

        void UpdateScroll()
        {
            float scroll = Mouse.GetState().ScrollWheelValue - OldScrollValue;

            OldScrollValue = Mouse.GetState().ScrollWheelValue;

            if (scrollDelay.Wait())
                return;

            if (scroll > 0 )
            {

                Speed *= 1.5f;
                scrollDelay.AddDelay(0.2f);

            }
            else if( scroll < 0 )
            {
                Speed /= 1.5f;
                scrollDelay.AddDelay(0.2f);
            }

        }


    }
}
