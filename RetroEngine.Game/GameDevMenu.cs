using RetroEngine;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetroEngine.Entities;

namespace RetroEngine.Game
{
    internal class GameDevMenu : DevMenu
    {

        public override void Update()
        {
            base.Update();

            ImGui.Begin("game");

            ImGui.DragFloat3("sm rot", ref StaticMeshEntity.testRot);

            ImGui.End();

        }

    }
}
