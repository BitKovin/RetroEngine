using Microsoft.Xna.Framework;
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

        }

        public static void PreloadAllWeapons()
        {
            Type[] types = Assembly.GetAssembly(typeof(Weapon)).GetTypes().Where(t => t.IsSubclassOf(typeof(Weapon))).ToArray();

            foreach (Type type in types) 
            {
                Weapon weapon = Activator.CreateInstance(type) as Weapon;
                weapon.data = new WeaponData();
                weapon.LoadAssetsIfNeeded();
            }

            types = null;

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            float progress = Math.Clamp((Time.gameTime - SpawnTime)/DrawTime, 0, 1);

            DrawRotation = new Vector3((1 - progress) * 20, (1 - progress) * 20, (1 - progress) * 30);

        }

    }
}
