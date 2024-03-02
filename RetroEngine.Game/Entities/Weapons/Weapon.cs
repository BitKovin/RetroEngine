using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Game.Entities.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Weapons
{

    public class WeaponData
    {

        public Type weaponType;
        public int ammo = 50;

        public WeaponData() 
        {

        }
    }
    
    public class Weapon : Entity
    {

        protected WeaponData data;
        protected PlayerCharacter player;

        protected float DrawTime = 0.35f;

        protected bool Drawing = true;

        protected Vector3 DrawRotation = Vector3.Zero;

        protected Vector3 Sway = new Vector3();

        public static Weapon CreateFromData(WeaponData data, PlayerCharacter owner = null)
        {
            Weapon weapon = Activator.CreateInstance(data.weaponType) as Weapon;

            weapon.data = data;
            weapon.player = owner;
            weapon.Start();

            return weapon;
        }

        public override void Update()
        {
            base.Update();

            LateUpdateWhilePaused = true;

            if(Time.gameTime - SpawnTime > DrawTime)
                Drawing = false;

            UpdateSway();

        }

        public static void PreloadAllWeapons()
        {
            Type[] types = Assembly.GetAssembly(typeof(Weapon)).GetTypes().Where(t => t.IsSubclassOf(typeof(Weapon))).ToArray();

            foreach (Type type in types) 
            {
                Weapon weapon = Activator.CreateInstance(type) as Weapon;
                weapon.data = new WeaponData();
                weapon.LoadAssetsIfNeeded();
                foreach(object model in weapon.meshes)
                {
                    AssetRegistry.ConstantCache.Add(model); 
                }
            }

            types = null;

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            float progress = Math.Clamp((Time.gameTime - SpawnTime)/DrawTime, 0, 1);

            DrawRotation = new Vector3((1 - progress) * 20, (1 - progress) * 20, (1 - progress) * 30);



        }


        void UpdateSway()
        {
            Sway -= new Vector3(Input.MouseDelta.X,-Input.MouseDelta.Y,0)/3000;
            Sway = Vector3.Lerp(Sway,Vector3.Zero, Time.deltaTime*12);

            Sway.X = Math.Clamp(Sway.X,-0.05f, 0.05f);
            Sway.Y = Math.Clamp(Sway.Y, -0.03f, 0.03f);

            Sway.Z = Math.Abs(Sway.X) + Math.Abs(Sway.Y);
            Sway.Z /= -1.2f;

            //Console.WriteLine(Sway);

        }

        protected Vector3 GetWorldSway()
        {
            return Camera.rotation.GetRightVector()*Sway.X + Camera.rotation.GetUpVector()*Sway.Y + Camera.rotation.GetForwardVector()*Sway.Z;
        }

    }
}
