using ImGuiNET;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class DevMenu
    {

        public List<string> log = new List<string>();


        bool scrolldown = false;

        string input = "";


        public virtual void Log(string text)
        {
            log.Add(text);
            scrolldown = true;
        }


        public virtual void Update()
        {

            

            // Inside your rendering loop or where you handle ImGui rendering
            ImGui.PushStyleColor(ImGuiCol.DockingEmptyBg, new Vector4(0));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0));
            ImGui.DockSpaceOverViewport();
            ImGui.PopStyleColor(2);

            DrawStats();

            ImGui.BeginMainMenuBar();
            ImGui.Text("fps: " + (int)(1f / Time.deltaTime) + "    entity count: " + Level.GetCurrent().entities.Count);
            ImGui.EndMainMenuBar();

            if (!GameMain.Instance.paused) return;

            ImGui.Begin("lighting");

            ImGui.SliderFloat("shadow bias", ref Graphics.ShadowBias, -0.005f, 0.005f);

            ImGui.SliderFloat("light direction X", ref Graphics.LightDirection.X, -1, 1);
            ImGui.SliderFloat("light direction Y", ref Graphics.LightDirection.Y, -1, 1);
            ImGui.SliderFloat("light direction Z", ref Graphics.LightDirection.Z, -1, 1);

            ImGui.End();

            ImGui.Begin("Graphics");

            ImGui.SliderFloat("resolution scale", ref Render.ResolutionScale, 0.2f, 3);

            ImGui.Checkbox("occlusion queries", ref Level.GetCurrent().OcclusionCullingEnabled);

            ImGui.Checkbox("opaque blending", ref Graphics.OpaqueBlending);

            ImGui.Checkbox("diasble backface culling", ref Graphics.DisableBackFaceCulling);

            ImGui.Checkbox("fxaa", ref Graphics.EnableAntiAliasing);

            ImGui.Checkbox("bloom", ref Graphics.EnableBloom);

            ImGui.Checkbox("merge meshes", ref MapData.MergeBrushes);

            ImGui.InputInt("shadowmap size", ref Graphics.shadowMapResolution);


            ImGui.Checkbox("dev menu", ref GameMain.Instance.DevMenuEnabled);

            ImGui.End();


            //string consoleContent = sb.ToString();

            ImGui.Begin("Console");
            

            //ImGui.InputTextMultiline("log", ref consoleContent, uint.MaxValue, new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight()-60),ImGuiInputTextFlags.ReadOnly);

            ImGui.BeginChild("ScrollingRegion", new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - 70));

            foreach(var item in log)
            {
                if(item is not null)
                if(ImGui.Selectable(item))
                {
                    ImGui.SetClipboardText(item);
                }
            }

            //ImGui.InputTextMultiline("log", ref consoleContent, uint.MaxValue, new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - 60), ImGuiInputTextFlags.ReadOnly);

            if (scrolldown)
            {
                ImGui.SetScrollHereY(0);
                scrolldown = false;
            }

            ImGui.EndChild();

            ImGui.PushItemWidth(ImGui.GetWindowWidth() - 100);
            if (ImGui.InputText("", ref input, 1000000,ImGuiInputTextFlags.EnterReturnsTrue)) submitInput();
            ImGui.PopItemWidth();
            ImGui.SameLine();
            if (ImGui.Button("SUBMIT")) submitInput();
            
            ImGui.End();
        }

        protected virtual void DrawStats()
        {

            var statsResults = Stats.GetResults();

            ImGui.Begin("Stats");

            ImGui.Text($"{Stats.RenderedMehses} meshes were rendered");

            foreach(var item in statsResults.Keys)
            {

                float value = statsResults[item];

                ImGui.InputFloat(item,ref value);
                

                value /= 10000;

                
                ImGui.ProgressBar(value, new Vector2(200,20));

                ImGui.Text("\n");

            }

            ImGui.End();
        }

        void submitInput()
        {
            Log(input);
            ConsoleCommands.ProcessCommand(input);

            input = "";
        }

        public virtual void Init() 
        {
            if (GameMain.Instance.DevMenuEnabled == false) return;

            ImGuiIOPtr io = ImGui.GetIO();
            ImGuiStylePtr style = ImGui.GetStyle();

            int n = style.Colors.Count;

            style.ScaleAllSizes(1.2f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(0.75f, 0.75f, 0.75f, 1.00f);
            style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.35f, 0.35f, 0.35f, 1.00f);
            style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.10f, 0.10f, 0.10f, 0.84f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
            style.Colors[(int)ImGuiCol.Border] = new Vector4(0.00f, 0.00f, 0.00f, 0.50f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.54f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.37f, 0.14f, 0.14f, 0.67f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.39f, 0.20f, 0.20f, 0.67f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.04f, 0.04f, 0.04f, 1.00f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.48f, 0.16f, 0.16f, 1.00f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.48f, 0.16f, 0.16f, 1.00f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.34f, 0.34f, 0.34f, 0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.56f, 0.10f, 0.10f, 1.00f);
            style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(1.00f, 0.19f, 0.19f, 0.40f);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.89f, 0.00f, 0.19f, 1.00f);
            style.Colors[(int)ImGuiCol.Button] = new Vector4(1.00f, 0.19f, 0.19f, 0.40f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.80f, 0.17f, 0.00f, 1.00f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.89f, 0.00f, 0.19f, 1.00f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.33f, 0.35f, 0.36f, 0.53f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.76f, 0.28f, 0.44f, 0.67f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.47f, 0.47f, 0.47f, 0.67f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.32f, 0.32f, 0.32f, 1.00f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.32f, 0.32f, 0.32f, 1.00f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.32f, 0.32f, 0.32f, 1.00f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(1.00f, 1.00f, 1.00f, 0.85f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(1.00f, 1.00f, 1.00f, 0.60f);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(1.00f, 1.00f, 1.00f, 0.90f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.07f, 0.07f, 0.07f, 0.51f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.86f, 0.23f, 0.43f, 0.67f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.19f, 0.19f, 0.19f, 0.57f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.05f, 0.05f, 0.05f, 0.90f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.13f, 0.13f, 0.13f, 0.74f);
            style.Colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.47f, 0.47f, 0.47f, 0.47f);
            style.Colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.07f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
        }

    }
}
