﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.UI
{
    public class UiCanvas : UiElement
    {

        public override Vector2 GetSize()
        {
            return parrent.GetSize();

        }

        public override void Update()
        {

            //position = parrent.position + parrent.offset;

            size = GetSize();

            base.Update();
        }

    }
}