using ImGuiNET;
using Monogame.ImGuiNet.Utils;
using RetroEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkeletalMeshEditor
{
    internal class SandboxMenu : DevMenu
    {



        static string path = "models/skeletal_test.fbx";
        static int animId = 0;

        static string animationPath = "Animations/human/rest.fbx";

        static int selectedHitbox = 0;

        public override void Update()
        {
            //base.Update();

            // Inside your rendering loop or where you handle ImGui rendering
            ImGui.PushStyleColor(ImGuiCol.DockingEmptyBg, new Vector4(0));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0));
            ImGui.DockSpaceOverViewport();
            ImGui.PopStyleColor(2);

            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("File"))
            {

                if (ImGui.Button("New"))
                {
                    Level.LoadLevelFromFile("Empty");
                    Level.GetCurrent().Name = "";
                }

                if (ImGui.Button("Open"))
                {
                    OpenFileDialog fileDialog = new OpenFileDialog();
                    fileDialog.Multiselect = false;
                    fileDialog.InitialDirectory = Path.GetFullPath(AssetRegistry.ROOT_PATH + "GameData\\maps\\");
                    fileDialog.Filter = "level files (*.level)|*.level|All files (*.*)|*.*";
                    fileDialog.FilterIndex = 0;
                    fileDialog.RestoreDirectory = true;
                    fileDialog.CheckFileExists = true;
                    fileDialog.CheckPathExists = true;
                    fileDialog.DefaultExt = ".level";

                    if(fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string path = fileDialog.FileName;

                        Level.LoadLevelFromFile(path);

                    }

                }
                ImGui.EndMenu();
            }


        }

    }
}
