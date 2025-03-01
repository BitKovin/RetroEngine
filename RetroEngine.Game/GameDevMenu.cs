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

            WeaponMenu();

        }

        void WeaponMenu()
        {
            ImGui.Begin("weapon");

            if (ImGui.Button("weapon_pistol"))
            {
                ConsoleCommands.ProcessCommand("weapon.give weapon_pistol");
            }
            if (ImGui.Button("weapon_pistol_double"))
            {
                ConsoleCommands.ProcessCommand("weapon.give weapon_pistol_double");
            }
            if (ImGui.Button("weapon_shotgunNew"))
            {
                ConsoleCommands.ProcessCommand("weapon.give weapon_shotgunNew");
            }
            if (ImGui.Button("weapon_shotgun_double"))
            {
                ConsoleCommands.ProcessCommand("weapon.give weapon_shotgun_double");
            }


            ImGui.End();
        }

    }
}
