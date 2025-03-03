using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Game.Entities.Player;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Assimp.Metadata;

namespace RetroEngine.Game.Entities.Weapons
{

    [JsonSerializable(typeof(WeaponData))]
    public class WeaponData
    {
        [JsonInclude]
        public Type weaponType;

        [JsonInclude]
        public int ammo = 0;

        public int Priority = 0;

        public int Slot = 0;

        public string iconPath = "cat.png";

        public WeaponData() 
        {

        }

        public static WeaponData FromType(Type type)
        {
            Weapon weapon = Activator.CreateInstance(type) as Weapon;

            return weapon.GetDefaultWeaponData();

        }

    }
    
    public class Weapon : Entity
    {

        public WeaponData Data;
        protected Entity player;

        protected double DrawTime = 0.35f;

        protected bool Drawing = true;

        protected Vector3 DrawRotation = Vector3.Zero;

        protected Vector3 Sway = new Vector3();

        public Vector3 Offset = new Vector3();

        public float BobScale = 1;

        public bool ShowHandR = false;

        public bool ShowHandL = true;

        public bool IsMelee = false;


        public static Weapon CreateFromData(WeaponData data, Entity owner = null)
        {
            Weapon weapon = Activator.CreateInstance(data.weaponType) as Weapon;

            weapon.Data = data;
            weapon.player = owner;

            weapon.LoadAssetsIfNeeded(true);

            weapon.Start();
            weapon.LateUpdate();

            return weapon;
        }

        public virtual void Blocked()
        {



        }

        public override void Update()
        {
            base.Update();

            LateUpdateWhilePaused = true;

            if(Time.GameTime - SpawnTime > DrawTime)
                Drawing = false;

            

        }

        public static void PreloadAllWeapons()
        {
            Type[] types = Assembly.GetAssembly(typeof(Weapon)).GetTypes().Where(t => t.IsSubclassOf(typeof(Weapon))).ToArray();

            foreach (Type type in types) 
            {
                Weapon weapon = Activator.CreateInstance(type) as Weapon;
                weapon.Data = new WeaponData();
                weapon.LoadAssetsIfNeeded();
                foreach(object model in weapon.meshes)
                {
                    AssetRegistry.ConstantCache.Add(model); 
                }
            }

            types = null;

        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

            foreach(var mesh in meshes)
            {
                mesh.DistanceSortingRadius = 10000000000;
            }

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            UpdateSway();

            Rotation.Z *= -2;

            float progress = Math.Clamp((float)((Time.GameTime - SpawnTime)/DrawTime), 0, 1);

            //DrawRotation = new Vector3((1 - progress) * 20, (1 - progress) * 20, (1 - progress) * 30);



        }

        public virtual AnimationPose ApplyWeaponAnimation(AnimationPose inPose)
        {
            return inPose;
        }

        void UpdateSway()
        {
            if (GameMain.Instance.paused) return;

            //return;

            Sway -= new Vector3(Input.MouseDelta.X,-Input.MouseDelta.Y,0)/3000;
            Sway = Vector3.Lerp(Sway,Vector3.Zero, Time.DeltaTime*12);

            Sway.X = Math.Clamp(Sway.X,-0.05f, 0.05f);
            Sway.Y = Math.Clamp(Sway.Y, -0.03f, 0.03f);

            Sway.Z = Math.Abs(Sway.X) + Math.Abs(Sway.Y);
            Sway.Z /= -1.2f;

            Sway.Z -= 0.03f;


        }

        public Vector3 GetWorldSway()
        {
            return Camera.rotation.GetRightVector()*Sway.X + Camera.rotation.GetUpVector()*Sway.Y + Camera.rotation.GetForwardVector()*Sway.Z;
        }

        public Vector3 GetWorldOffset(bool leftHand = false)
        {
            return Camera.rotation.GetRightVector() * Offset.X * (leftHand ? -1 : 1) + Camera.rotation.GetUpVector() * Offset.Y + Camera.rotation.GetForwardVector() * Offset.Z;
        }
        protected override void LoadAssets()
        {
            base.LoadAssets();

            WeaponData weaponData = GetDefaultWeaponData();

            if(weaponData != null)
                AssetRegistry.LoadTextureFromFile(weaponData.iconPath);


        }

        public virtual WeaponData GetDefaultWeaponData()
        {
            return null;
        }

        [ConsoleCommand("weapon.give")]
        public static void GiveToPlayer(string typeName)
        {
            try
            {
                PlayerCharacter character = Level.GetCurrent().FindEntityByName("player") as PlayerCharacter;

                Assembly asm = typeof(Weapon).Assembly;
                Type type = asm.GetType(typeof(Weapon).Namespace + "." + typeName);
                if (type == null)
                {
                    Logger.Log($"weapon with type name of {typeName} not found");
                    return;
                }
                WeaponData weaponData = WeaponData.FromType(type);

                character.AddWeapon(weaponData);
            }catch (Exception ex) { Logger.Log(ex.Message); }
        }

        public virtual bool ShouldHideLeftHand()
        {
            return false;
        }

        protected void SetHideLeftHand(SkeletalMesh mesh, bool hide)
        {

            Matrix scale = hide ? Matrix.CreateScale(0) : Matrix.Identity;

            mesh.SetBoneLocalTransformModification("clavicle_l", scale);
        }

        public virtual bool CanChangeSlot()
        {
            return true;
        }

    }
}
