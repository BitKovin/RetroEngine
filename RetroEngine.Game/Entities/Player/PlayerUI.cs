using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Game.Entities.Weapons;
using RetroEngine.PhysicsSystem;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Player
{
    internal class PlayerUI : UiCanvas
    {

        PlayerCharacter player;

        public PlayerUI(PlayerCharacter plr) { player = plr; }

        Image crosshair = new Image();
        UiText health = new UiText();
        UiText fps = new UiText();
        WeaponSlots weaponSlots = new WeaponSlots();

        bool loaded = false;

        StaticMesh crosshairMesh = new StaticMesh();

        public Vector3 crosshairOffset = new Vector3(0.1f,-0.15f,0);

        public static bool ShowCrosshair = true;

        static PlayerUI instance;

        public void Load()
        {
            crosshair.baseColor = new Color(1f, 1f, 1f) * 2f;
            crosshair.SetTexture("ui/crosshair.png");

            crosshair.Origin = new Vector2(0.5f, 0.5f);
            crosshair.size = new Vector2(10, 10);
            crosshair.Pivot = new Vector2(0.5f, 0.5f);

            AddChild(crosshair);

            health.Origin = new Vector2(0, 1);
            health.position = new Vector2(30,-50);
            health.FontSize = 24;
            health.AlignProgress = new Vector2(0.0f, 0.5f);
            AddChild(health);

            fps.Origin = new Vector2(0, 0);
            fps.position = new Vector2(30, 50);
            fps.FontSize = 24;
            fps.AlignProgress = new Vector2(0.0f, 0.5f);
            AddChild(fps);

            weaponSlots.player = player;
            weaponSlots.Origin = new Vector2(0.5f, 1f);
            weaponSlots.Pivot = new Vector2(0.5f, 1f);
            weaponSlots.position = new Vector2(0, -20);
            AddChild(weaponSlots);

            Viewport.AddChild(this);

            //LoadWorldCrosshair();

            instance = this;

            loaded = true;
        }

        public override void Update()
        {

            base.Update();

            if (loaded == false) return;

            health.text = Math.Ceiling(player.Health).ToString();// ((Vector3)((ICharacter)player).GetPhysicsBody().LinearVelocity).XZ().Length().ToString();

            fps.text = ((int)(1f / (Time.DeltaTime/Time.TimeScale))).ToString();


            //UpdateWorldCrosshair();

            crosshairMesh.Visible = ShowCrosshair;

        }

        public void Destroy()
        {
            ClearChild();
            Viewport.RemoveChild(this);
        }



        void LoadWorldCrosshair()
        {
            crosshairMesh.LoadFromFile("models/ui/crosshair.obj");
            crosshairMesh.Shader = new Graphic.SurfaceShaderInstance("unlit");
            crosshairMesh.texture = AssetRegistry.LoadTextureFromFile("engine/textures/white.png");
            player.meshes.Add(crosshairMesh);
        }

        void UpdateWorldCrosshair()
        {

            Vector3 cameraPosWithOffset = Camera.position + Camera.rotation.GetUpVector() * crosshairOffset.Y + Camera.rotation.GetRightVector() * crosshairOffset.X + Camera.rotation.GetForwardVector() * crosshairOffset.Z;
            cameraPosWithOffset = Camera.position;


            var hit = Physics.LineTrace(cameraPosWithOffset.ToPhysics(), (cameraPosWithOffset + Camera.rotation.GetForwardVector() * 100).ToPhysics(), new List<CollisionObject>() {((ICharacter)player).GetPhysicsBody() });

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

            crosshair.position = UiElement.WorldToScreenSpace(crosshairPos) - crosshair.size/2;

        }

        [ConsoleCommand("hud.show")]
        public static void SetVisibility(bool value)
        {

            if (instance == null) return;

            instance.Visible = value;
        }

        class WeaponSlots : ContentBox
        {

            public PlayerCharacter player;

            public override void Update()
            {


                childs.Clear();

                foreach(WeaponData weaponData in player.weapons)
                {
                    if(weaponData==null) continue;

                    Image img = new Image();
                    img.SetTexture(weaponData.iconPath);
                    img.size = new Vector2(120,120);
                    AddChild(img);
                    img.position = new Vector2((childs.Count-1) * 125,0);

                    if (weaponData.Slot == player.currentSlot)
                        img.DrawBorder = true;


                }

                base.Update();

            }

        }


    }
}
