using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Player
{
    internal class PlayerUI
    {

        PlayerCharacter player;

        public PlayerUI(PlayerCharacter plr) { player = plr; }

        Image crosshair = new Image();
        Text health = new Text();
        Text fps = new Text();

        bool loaded = false;

        StaticMesh crosshairMesh = new StaticMesh();

        public Vector3 crosshairOffset = new Vector3(0.1f,-0.15f,0);

        public static bool ShowCrosshair = true;

        public void Load()
        {
            crosshair.baseColor = new Color(1f, 1f, 1f) * 2f;
            crosshair.SetTexture("ui/crosshair.png");

            crosshair.originH = Origin.CenterH;
            crosshair.originV = Origin.CenterV;
            crosshair.size = new Vector2(4, 4);
            crosshair.position = -crosshair.size/2;
            //UiElement.Viewport.childs.Add(crosshair);

            health.originH = Origin.Left;
            health.originV = Origin.Bottom;
            health.position = new Vector2(10,-50);
            health.FontSize = 24;
            health.AlignProgress = new Vector2(0.0f, 0.5f);
            UiElement.Viewport.childs.Add(health);

            fps.originH = Origin.Left;
            fps.originV = Origin.Top;
            fps.position = new Vector2(10, 50);
            fps.FontSize = 24;
            fps.AlignProgress = new Vector2(0.0f, 0.5f);
            UiElement.Viewport.childs.Add(fps);

            LoadWorldCrosshair();

            loaded = true;
        }

        public void Update()
        {
            if (loaded == false) return;

            health.text = ((Vector3)player.body.LinearVelocity).XZ().Length().ToString();

            fps.text = ((int)(1f / Time.deltaTime)).ToString();

            UpdateWorldCrosshair();

            crosshairMesh.Visible = ShowCrosshair;

        }

        public void Destroy()
        {

        }

        void LoadWorldCrosshair()
        {
            crosshairMesh.LoadFromFile("models/ui/crosshair.obj");
            crosshairMesh.Shader = AssetRegistry.GetShaderFromName("unlit");
            crosshairMesh.texture = AssetRegistry.LoadTextureFromFile("engine/textures/white.png");
            player.meshes.Add(crosshairMesh);
        }

        void UpdateWorldCrosshair()
        {

            Vector3 cameraPosWithOffset = Camera.position + Camera.rotation.GetUpVector() * crosshairOffset.Y + Camera.rotation.GetRightVector() * crosshairOffset.X + Camera.rotation.GetForwardVector() * crosshairOffset.Z;



            var hit = Physics.LineTrace(cameraPosWithOffset.ToPhysics(), (cameraPosWithOffset + Camera.rotation.GetForwardVector() * 100).ToPhysics(), new List<CollisionObject>() {player.body });

            Vector3 crosshairPos = cameraPosWithOffset + Camera.rotation.GetForwardVector() * 100;

            if (hit.HasHit)
            {
                crosshairPos = hit.HitPointWorld;
            }

            float scale = MathHelper.Lerp(Vector3.Distance(crosshairPos, Camera.position)/10,1,0.3f);
            scale *= 0.03f;

            crosshairMesh.Position = Vector3.Lerp(crosshairPos,Camera.position,0.8f);
            crosshairMesh.Rotation = MathHelper.FindLookAtRotation(Camera.rotation.GetForwardVector(),Vector3.Zero);
            crosshairMesh.Scale = new Vector3(scale);
        }

    }
}
