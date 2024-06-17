using ImGuiNET;
using RetroEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SkeletalMeshEditor
{
    internal class SkeletalDevMenu : DevMenu
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
                    SkeletalMeshPreview.skeletalMesh.ClearHitboxBodies();
                    SkeletalMeshPreview.skeletalMesh.hitboxes = new List<HitboxInfo>();
                }

                if (ImGui.Button("Open"))
                {
                    SkeletalMeshPreview.skeletalMesh.LoadMeshMetaFromFile(path);
                    SkeletalMeshPreview.skeletalMesh.ReloadHitboxes(SkeletalMeshPreview.instance);
                }

                if (ImGui.Button("Save"))
                    SkeletalMeshPreview.skeletalMesh.SaveMeshMetaToFile(path);


                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();

            ImGui.Begin("Skeletal Editor");
            ImGui.InputText("file path: ",ref path, 60);
            if(ImGui.Button("Load"))
            {
                SkeletalMeshPreview.skeletalMesh.LoadFromFile(path);
                SkeletalMeshPreview.skeletalMesh.texture = AssetRegistry.LoadTextureFromFile("__TB_empty.png");
            }

            ImGui.Spacing();
            ImGui.Spacing();

            var names = GetHitboxNames();
            ImGui.Text("selected: " + selectedHitbox.ToString());
            ImGui.ListBox("hitboxes", ref selectedHitbox, names, names.Length);

            if (ImGui.Button("Add"))
                SkeletalMeshPreview.skeletalMesh.hitboxes.Add(new HitboxInfo());

            if (SkeletalMeshPreview.skeletalMesh.hitboxes.Count > selectedHitbox)
            {
                ImGui.SameLine();
                if (ImGui.Button("Remove"))
                    SkeletalMeshPreview.skeletalMesh.hitboxes.RemoveAt(selectedHitbox);
            }
            ImGui.Spacing();
            ImGui.Spacing();
            if(ImGui.Button("refreshHitboxes") || Input.GetAction("r").Pressed())
            {
                SkeletalMeshPreview.skeletalMesh.ClearHitboxBodies();
                SkeletalMeshPreview.skeletalMesh.CreateHitboxBodies(SkeletalMeshPreview.instance);
            }

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Text("Animation Player");

            ImGui.InputText("animation file path: ", ref animationPath, 70);
            ImGui.InputInt("animation Index: ", ref animId);

            if (ImGui.Button("Load Animaion"))
            {
                SkeletalMeshPreview.Animation = new RetroEngine.Skeletal.Animation();
                SkeletalMeshPreview.Animation.LoadFromFile(animationPath);
                SkeletalMeshPreview.Animation.PlayAnimation(animId);
            }

            ImGui.End();

            HitboxEditor();
        }

        static Vector3 oldSize;

        void HitboxEditor()
        {

            if (SkeletalMeshPreview.skeletalMesh.hitboxes.Count <= selectedHitbox)
                return;

            HitboxInfo hitbox = SkeletalMeshPreview.skeletalMesh.hitboxes[selectedHitbox];

            ImGui.Begin("hitbox editor");

            ImGui.InputText("bone", ref hitbox.Bone, 25);
            ImGui.DragFloat3("size", ref hitbox.Size, 0.005f);
            ImGui.DragFloat3("position", ref hitbox.Position, 0.015f);
            ImGui.DragFloat3("rotation", ref hitbox.Rotation, 0.15f);

            ImGui.End();

            if(oldSize!= hitbox.Size)
            {
                SkeletalMeshPreview.skeletalMesh.ClearHitboxBodies();
                SkeletalMeshPreview.skeletalMesh.CreateHitboxBodies(SkeletalMeshPreview.instance);
            }

            oldSize = hitbox.Size;

        }

        string[] GetHitboxNames()
        {
            List<string> names = new List<string>();

            int i = 0;
            foreach(HitboxInfo hitbox in SkeletalMeshPreview.skeletalMesh.hitboxes)
            {
                names.Add(i.ToString() + "   " + hitbox.Bone);

                i++;
            }

            return names.ToArray();
        }

    }
}
