using RetroEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Weapon CreateFromData(WeaponData data, Player owner = null)
        {
            Weapon weapon = Activator.CreateInstance(data.weaponType) as Weapon;

            weapon.data = data;
            weapon.player = owner;
            weapon.Start();

            return weapon;
        }

        

    }
}
