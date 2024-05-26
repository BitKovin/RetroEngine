using BulletSharp;
using RetroEngine.Game.Entities.Player;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Weapons
{

    [LevelObject("weaponPickup")]
    internal class WeaponPickup : Trigger
    {

        string typeName = "weapon_pistol_double";

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            RigidBody TriggerBody = Physics.CreateSphere(this, 0, 1, CollisionFlags.StaticObject| CollisionFlags.NoContactResponse);

            TriggerBody.SetPosition(Position);
            bodies.Add(TriggerBody);

            typeName = data.GetPropertyString("type", "weapon_pistol_double");

        }

        public override void OnTriggerEnter(Entity entity)
        {
            base.OnTriggerEnter(entity);

            if (entity.Tags.Contains("player") == false) return;

            PlayerCharacter character = (PlayerCharacter)entity;

            Assembly asm = typeof(Weapon).Assembly;
            Type type = asm.GetType(typeof(Weapon).Namespace+"."+typeName);
            if(type==null)
            {
                Logger.Log($"weapon with type name of {typeName} not found");
                return;
            }
            WeaponData weaponData = WeaponData.FromType(type);

            character.AddWeapon(weaponData);

            Destroy();

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            StaticMesh staticMesh = new StaticMesh();
            staticMesh.LoadFromFile("models/cube.obj");
            staticMesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");
            meshes.Add(staticMesh);
            staticMesh.Position = Position;

        }

    }
}
