using Engine;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game
{
    internal class GameDevMenu : DevMenu
    {

        public override void Update()
        {
            base.Update();

            ImGui.Text((1 / Time.deltaTime).ToString());

        }

    }
}
