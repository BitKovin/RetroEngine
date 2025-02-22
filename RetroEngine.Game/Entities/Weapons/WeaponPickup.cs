using BulletSharp;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.Game.Entities.Player;
using RetroEngine.Map;
using RetroEngine.PhysicsSystem;
using RetroEngine.SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Weapons
{

    [LevelObject("weaponPickup")]
    public class WeaponPickup : Entity
    {

        string typeName = "weapon_pistol_double";

        StaticMesh staticMesh = new StaticMesh();

        public WeaponPickup()
        {
            SaveGame = true;
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

        }


        public override void Start()
        {
            base.Start();


        }

        public void OnTriggerEnter(Entity entity)
        {

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

            GetOwner()?.OnAction("despawned");

            Destroy();

        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            if (PlayerCharacter.Instance != null)
            {
                if (Vector3.Distance(PlayerCharacter.Instance.Position, Position) < 1f)
                {
                    OnTriggerEnter(PlayerCharacter.Instance);
                }
            }

            staticMesh.Position = Position + ((float)Math.Sin((Time.GameTime - SpawnTime)*3)) * Vector3.Up * 0.3f;

            staticMesh.Rotation = new Vector3(0, (float)(Time.GameTime - SpawnTime) * 100,0);

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            staticMesh.LoadFromFile("models/cube.obj");
            staticMesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");
            meshes.Add(staticMesh);
            staticMesh.Position = Position;

        }

        public override void LoadData(EntitySaveData Data)
        {
            base.LoadData(Data);


        }

    }
}
