﻿using ImGuiNET;
using RetroEngine;
using RetroEngine.PhysicsSystem;
using RetroEngine.Skeletal;
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

            DrawConsole();

            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("File"))
            {

                if (ImGui.Button("New"))
                {
                    SkeletalMeshPreview.instance.skeletalMesh.ClearRagdollBodies();
                    SkeletalMeshPreview.instance.skeletalMesh.hitboxes = new List<HitboxInfo>();
                }

                //if (ImGui.Button("Open"))
                //{
                //    SkeletalMeshPreview.instance.skeletalMesh.LoadMeshMetaFromFile(path);
                //    SkeletalMeshPreview.instance.skeletalMesh.ReloadHitboxes(SkeletalMeshPreview.instance);
                //}

                if (ImGui.Button("Save"))
                    SkeletalMeshPreview.instance.skeletalMesh.SaveMeshMetaToFile(path);


                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();

            ImGui.Begin("Skeletal Editor");
            ImGui.InputText("file path: ",ref path, 60);
            if(ImGui.Button("Load"))
            {

                SkeletalMeshPreview.instance?.Destroy();

                SkeletalMesh.LoadedRigModels.Clear();

                Level.GetCurrent().AddEntity(new SkeletalMeshPreview());

                SkeletalMesh.LoadedMeta.Clear();

                SkeletalMeshPreview.instance.skeletalMesh.LoadFromFile(path);
                SkeletalMeshPreview.instance.skeletalMesh.texture = AssetRegistry.LoadTextureFromFile("cat.png"); //"__TB_empty.png"
            }

            ImGui.Spacing();
            ImGui.Spacing();

            var names = GetHitboxNames();
            ImGui.Text("selected: " + selectedHitbox.ToString());
            ImGui.ListBox("hitboxes", ref selectedHitbox, names, names.Length, 20);

            if (ImGui.Button("Add"))
                SkeletalMeshPreview.instance.skeletalMesh.hitboxes.Add(new HitboxInfo());

            if (SkeletalMeshPreview.instance.skeletalMesh.hitboxes.Count > selectedHitbox)
            {
                ImGui.SameLine();
                if (ImGui.Button("Remove"))
                {
                    Physics.Remove(SkeletalMeshPreview.instance.skeletalMesh.hitboxes[selectedHitbox].RagdollRigidBodyRef);
                    SkeletalMeshPreview.instance.skeletalMesh.hitboxes.RemoveAt(selectedHitbox);
                }    
            }
            ImGui.Spacing();
            ImGui.Spacing();
            if(ImGui.Button("refreshHitboxes") || Input.GetAction("r").Pressed())
            {
                SkeletalMeshPreview.instance.skeletalMesh.ClearRagdollBodies();
                SkeletalMeshPreview.instance.skeletalMesh.CreateRagdollBodies(SkeletalMeshPreview.instance);
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
                SkeletalMeshPreview.Animation.LoadFromFile(animationPath);
                SkeletalMeshPreview.Animation.PlayAnimation(animId);
            }

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.Text("Ragdoll Tools");

            ImGui.Spacing();

            ImGui.Text("rest_pose->reset->ragdoll before saving!");

            ImGui.Spacing();

            ImGui.SliderFloat("hinge limit", ref SkeletalMeshPreview.instance.skeletalMesh.RagdollHingeForce, 0, 1);

            ImGui.Spacing();

            if (ImGui.Button("Ragdoll"))
            {
                SkeletalMeshPreview.instance.CreateRagdoll();
            }
            if (ImGui.Button("Stop Ragdoll"))
            {
                SkeletalMeshPreview.instance.StopRagdoll();
            }

            if (ImGui.Button("reset"))
            {

                foreach (var box in SkeletalMeshPreview.instance.skeletalMesh.hitboxes)
                {

                    box.ConstrainLocal1 = Microsoft.Xna.Framework.Matrix.Identity;
                    box.ConstrainLocal2 = Microsoft.Xna.Framework.Matrix.Identity;
                    box.RigidBodyMatrix = Microsoft.Xna.Framework.Matrix.Identity;

                }

                    foreach (HitboxInfo hbox in SkeletalMeshPreview.instance.skeletalMesh.hitboxes)
                {
                    bool found = false;

                    RiggedModel.RiggedModelNode riggedModelNode = null;

                    riggedModelNode = SkeletalMeshPreview.instance.skeletalMesh.GetBoneByName(hbox.Bone);

                    while (found == false)
                    {

                        if (riggedModelNode.parent == null)
                            break;

                        foreach (var box in SkeletalMeshPreview.instance.skeletalMesh.hitboxes)
                        {

                            if (riggedModelNode.parent == null)
                                continue;

                            if (box.Bone.ToLower() == riggedModelNode.parent.name.ToLower())
                            {

                                hbox.Parrent = box.Bone;
                                found = true;
                                break;

                            }
                        }

                        riggedModelNode = riggedModelNode.parent;


                    }
                }


            }

            ImGui.End();


            AnimationEventEditor();

            HitboxEditor();
        }

        static Vector3 oldSize;
        static Vector3 oldPos;
        static Vector3 oldRot;


        int selectedEvent = 0;

        int animationFrame = 0;

        int animationIndex = 0;

        bool playingAnimation = false;

        void AnimationEventEditor()
        {
            ImGui.Begin("animaiton event editor");


            ImGui.Text(SkeletalMeshPreview.instance.skeletalMesh.GetCurrentAnimationName());
            ImGui.InputInt("current animation", ref animationIndex);

            animationIndex = int.Clamp(animationIndex, 0, int.Max(SkeletalMeshPreview.instance.skeletalMesh.GetNumOfAnimations() - 1,0));

            ImGui.SameLine();


            if (SkeletalMeshPreview.instance.skeletalMesh.GetCurrentAnimationIndex() != animationIndex)
            {
                SkeletalMeshPreview.instance.skeletalMesh.PlayAnimation(animationIndex);
                SkeletalMeshPreview.Animation = null;
            }

            SkeletalMeshPreview.instance.skeletalMesh.SetIsPlayingAnimation(playingAnimation);

            var names = GetEventsNames();
            ImGui.Text("selected: " + selectedEvent.ToString());
            ImGui.ListBox("  ", ref selectedEvent, names, names.Length, 3);
            ImGui.SameLine();
            ImGui.BeginGroup();
            if(ImGui.Button("add"))
            {
                SkeletalMeshPreview.instance.skeletalMesh.CurrentAnimationInfo.AddEvent(animationFrame, "event_"+ animationFrame.ToString());
            }

            if (ImGui.Button("remove"))
            {

                var e = SkeletalMeshPreview.instance.skeletalMesh.CurrentAnimationInfo.GetFromIndex(selectedEvent);

                SkeletalMeshPreview.instance.skeletalMesh.CurrentAnimationInfo.RemoveEvent(e);
            }

            ImGui.EndGroup();

            if(SkeletalMeshPreview.instance.skeletalMesh.IsAnimationPlaying())
                animationFrame = SkeletalMeshPreview.instance.skeletalMesh.GetCurrentAnimationFrame();
            else
                SkeletalMeshPreview.instance.skeletalMesh.SetCurrentAnimationFrame(animationFrame);

            ImGui.SliderInt("CurrentFrame", ref animationFrame, 0, SkeletalMeshPreview.instance.skeletalMesh.GetCurrentAnimationFrameDuration()-1);
            if (ImGui.Button(">"))
            {
                playingAnimation = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("||"))
            {
                playingAnimation = false;
            }

            var ev = SkeletalMeshPreview.instance.skeletalMesh.CurrentAnimationInfo.GetFromIndex(selectedEvent);

            if (ev != null)
            {
                ImGui.Spacing();
                ImGui.Text("Event Data: ");


                ImGui.InputText("event name", ref ev.Name, 30);

                ImGui.InputInt("animation frame", ref ev.AnimationFrame);
            }
            ImGui.End();

        }

        void HitboxEditor()
        {

            if (SkeletalMeshPreview.instance.skeletalMesh.hitboxes.Count <= selectedHitbox)
                return;

            HitboxInfo hitbox = SkeletalMeshPreview.instance.skeletalMesh.hitboxes[selectedHitbox];

            ImGui.Begin("hitbox editor");


            ImGui.InputText("bone", ref hitbox.Bone, 25);
            ImGui.DragFloat3("size", ref hitbox.Size, 0.005f);
            ImGui.DragFloat3("position", ref hitbox.Position, 0.015f);
            ImGui.DragFloat3("rotation", ref hitbox.Rotation, 0.15f);

            ImGui.Text("Constrains");
            ImGui.InputText("parrent", ref hitbox.Parrent, 25);
            
            if(ImGui.Button("auto parent"))
            {

                bool found = false;

                RiggedModel.RiggedModelNode riggedModelNode = null;

                riggedModelNode = SkeletalMeshPreview.instance.skeletalMesh.GetBoneByName(hitbox.Bone);

                while (found == false)
                {

                    if (riggedModelNode.parent == null)
                        break;

                    foreach(var box in SkeletalMeshPreview.instance.skeletalMesh.hitboxes)
                    {

                        if (riggedModelNode.parent == null)
                            continue;

                        if(box.Bone.ToLower() == riggedModelNode.parent.name.ToLower())
                        {

                            hitbox.Parrent = box.Bone;
                            found = true;
                            break;

                        }
                    }

                    riggedModelNode = riggedModelNode.parent;


                }
            }

            ImGui.Text("Constrains");
            ImGui.DragFloat3("Min Angular Rotation", ref hitbox.AngularLowerLimit, 0.15f);
            ImGui.DragFloat3("Max Angular Rotation", ref hitbox.AngularUpperLimit, 0.15f);

            DebugVisualizeHitboxConstrains(hitbox);

            //DrawDebug.Sphere(0.1f, hitbox.ConstrainPosition, Vector3.One, Time.DeltaTime * 2);

            ImGui.End();

            if(oldSize!= hitbox.Size || oldRot!= hitbox.Rotation || oldPos != hitbox.Position)
            {
                SkeletalMeshPreview.instance.skeletalMesh.ClearRagdollBodies();
                SkeletalMeshPreview.instance.skeletalMesh.CreateRagdollBodies(SkeletalMeshPreview.instance);
            }

            oldSize = hitbox.Size;
            oldPos = hitbox.Position;
            oldRot = hitbox.Rotation;

        }

        void DebugVisualizeHitboxConstrains(HitboxInfo hitbox)
        {
            if (hitbox == null) return;

            if(hitbox.RagdollRigidBodyRef == null) return;


            var trans = SkeletalMeshPreview.instance.skeletalMesh.GetBoneMatrix(hitbox.Bone).DecomposeMatrix();

            var bodyRotation = ((Microsoft.Xna.Framework.Matrix)hitbox.RagdollRigidBodyRef.WorldTransform).DecomposeMatrix().Rotation;

            trans.Scale = Vector3.One;

            var boneMatrix = trans.ToMatrix();

            var bonePosition = trans.Position;

            

            //Around Z
            for (int i = 0; i <= 30; i++)
            {

                float progress = (float)i / (float)30;

                Microsoft.Xna.Framework.Vector3 rotation = Vector3.Lerp(hitbox.AngularLowerLimit, hitbox.AngularUpperLimit, progress);

                //Microsoft.Xna.Framework.Matrix rotationMatrix = MathHelper.GetRotationMatrix(rotation);
                //var vector = Microsoft.Xna.Framework.Vector3.Transform(Vector3.UnitZ, rotationMatrix);

                var vector = Microsoft.Xna.Framework.Vector3.UnitX;

                vector = vector.RotateVector(Microsoft.Xna.Framework.Vector3.UnitZ, rotation.Z);

                vector = Microsoft.Xna.Framework.Vector3.Transform(vector, MathHelper.GetRotationMatrix(bodyRotation));

                vector.FastNormalize();

                DrawDebug.Line(bonePosition, bonePosition + vector/2, Vector3.UnitZ, Time.DeltaTime * 2);

            }

            //Around Y
            for (int i = 0; i <= 30; i++)
            {

                float progress = (float)i / (float)30;

                Microsoft.Xna.Framework.Vector3 rotation = Vector3.Lerp(hitbox.AngularLowerLimit, hitbox.AngularUpperLimit, progress);

                //Microsoft.Xna.Framework.Matrix rotationMatrix = MathHelper.GetRotationMatrix(rotation);
                //var vector = Microsoft.Xna.Framework.Vector3.Transform(Vector3.UnitZ, rotationMatrix);

                var vector = Microsoft.Xna.Framework.Vector3.UnitX;

                vector = vector.RotateVector(Microsoft.Xna.Framework.Vector3.UnitY, rotation.Y);

                vector = Microsoft.Xna.Framework.Vector3.Transform(vector, MathHelper.GetRotationMatrix(bodyRotation));

                vector.FastNormalize();

                DrawDebug.Line(bonePosition, bonePosition + vector / 2, Vector3.UnitY, Time.DeltaTime * 2);

            }

            //Around X
            for (int i = 0; i <= 30; i++)
            {

                float progress = (float)i / (float)30;

                Microsoft.Xna.Framework.Vector3 rotation = Vector3.Lerp(hitbox.AngularLowerLimit, hitbox.AngularUpperLimit, progress);

                //Microsoft.Xna.Framework.Matrix rotationMatrix = MathHelper.GetRotationMatrix(rotation);
                //var vector = Microsoft.Xna.Framework.Vector3.Transform(Vector3.UnitZ, rotationMatrix);

                var vector = Microsoft.Xna.Framework.Vector3.UnitY;

                vector = vector.RotateVector(Microsoft.Xna.Framework.Vector3.UnitX, rotation.X);

                vector = Microsoft.Xna.Framework.Vector3.Transform(vector, MathHelper.GetRotationMatrix(bodyRotation));

                vector.FastNormalize();

                DrawDebug.Line(bonePosition, bonePosition + vector / 2, Vector3.UnitX, Time.DeltaTime * 2);

            }

        }

        string[] GetHitboxNames()
        {
            List<string> names = new List<string>();

            int i = 0;
            foreach(HitboxInfo hitbox in SkeletalMeshPreview.instance.skeletalMesh.hitboxes)
            {
                names.Add(i.ToString() + "   " + hitbox.Bone);

                i++;
            }

            return names.ToArray();
        }

        string[] GetEventsNames()
        {

            List <string> names = new List<string>();

            foreach(var anim in SkeletalMeshPreview.instance.skeletalMesh.CurrentAnimationInfo.AnimationEvents)
            {
                names.Add(anim.ToString());
            }

            return names.ToArray();

        }

    }
}
