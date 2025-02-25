using BulletXNA;
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

    [LevelObject("heartPickup")]
    public class HeartPickup : Entity
    {

        StaticMesh staticMesh = new StaticMesh();

        public HeartPickup()
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

            if(character == null) return;

            if (character.Heal(20))
            {

                GetOwner()?.OnAction("despawned");

                Destroy();
            }
        }

        public override void AsyncUpdate()
        {

            base.AsyncUpdate();

            if(PlayerCharacter.Instance != null)
            {
                if(Vector3.Distance(PlayerCharacter.Instance.Position, Position) < 1f)
                {
                    OnTriggerEnter(PlayerCharacter.Instance);
                }
            }

            staticMesh.Position = Position + ((float)Math.Sin((Time.GameTime - SpawnTime)*3)) * Vector3.Up * 0.1f;

            staticMesh.Rotation = new Vector3(0, (float)(Time.GameTime - SpawnTime) * 30,0);


        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            staticMesh.LoadFromFile("models/items/heart.fbx");
            staticMesh.texture = AssetRegistry.LoadTextureFromFile("textures/items/heart.png");
            staticMesh.emisssiveTexture = AssetRegistry.LoadTextureFromFile("textures/items/heart.png");
            staticMesh.EmissionPower = 4f;
            staticMesh.Position = Position;

            meshes.Add(staticMesh);

        }

        public override void LoadData(EntitySaveData Data)
        {
            base.LoadData(Data);

        }

    }
}
