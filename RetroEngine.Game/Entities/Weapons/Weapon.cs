﻿using Microsoft.Xna.Framework;
using RetroEngine.Entities;
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
        protected Player player;

        protected float DrawTime = 0.3f;

        protected bool Drawing = true;

        protected Vector3 DrawRotation = Vector3.Zero;


        public static Weapon CreateFromData(WeaponData data, Player owner = null)
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
                weapon.Start();
                weapon.Destroy();
            }

            types = null;

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            float progress = Math.Clamp((Time.gameTime - SpawnTime)/DrawTime, 0, 1);

            DrawRotation = new Vector3((1 - progress) * 30, (1 - progress) * -10, 0);

        }

    }
}