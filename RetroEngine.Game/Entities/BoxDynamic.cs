using BulletSharp;
using RetroEngine;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using RetroEngine.Audio;
using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.SaveSystem;
using RetroEngine.PhysicsSystem;
using BulletSharp.SoftBody;

namespace RetroEngine.Game.Entities
{

    [LevelObject("ent_cat_point")]
    public class BoxDynamic : Entity
    {
        StaticMesh mesh = new StaticMesh();

        public Vector3 scale = new Vector3(1);

        SoundPlayer soundPlayer;

        RigidBody body;

        FmodEventInstance FmodEventInstance;

        public BoxDynamic() : base()
        {            

            SaveGame = true;

            soundPlayer = (SoundPlayer)Level.GetCurrent().AddEntity(new SoundPlayer());


            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.strings.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Dialogue_EN.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/SFX.bank");


            FmodEventInstance = FmodEventInstance.Create("event:/Character/Dialogue");
            FmodEventInstance.SoundTableKey = "welcome";
            //FmodEventInstance.SetProgrammerSound("welcome", AssetRegistry.LoadSoundFmodNativeFromFile("sounds/test.wav"));
            soundPlayer.SetSound(FmodEventInstance);

            soundPlayer.Position = Position;

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile("models/cube.obj");

            mesh.texture = AssetRegistry.LoadTextureFromFile("textures/brushes/cat.png");
            //mesh.normalTexture = AssetRegistry.LoadTextureFromFile("textures/foil_n.png");
            //mesh.ormTexture = AssetRegistry.LoadTextureFromFile("textures/foil_orm.png");

            mesh.CastShadows = true;

            meshes.Add(mesh);

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            soundPlayer.Position = Position;
            soundPlayer.Play();

        }

        public override void Start()
        {
            base.Start();


            body = Physics.CreateBox(this, scale.ToNumerics());
            body.SetPosition(Position.ToNumerics());
            body.SetMassProps(scale.Length(), body.CollisionShape.CalculateLocalInertia(scale.Length()));
            //body.UserIndex = (int)BodyType.HitTest;
            mesh.Scale = scale;

            bodies.Add(body);

        }

        Delay delay = new Delay();

        public override void Update()
        {
            base.Update();


            //skeletalMesh.Update(Time.deltaTime);

            body.Activate();

            if (Input.GetAction("test").Holding())
                body.ApplyCentralImpulse(new System.Numerics.Vector3(0,100,0)*Time.DeltaTime);

            mesh.Position = Position;
            mesh.Rotation = Rotation;

            soundPlayer.Position = Position;

        }

        public override void LoadData(EntitySaveData Data)
        {
            Console.WriteLine(Position);
            base.LoadData(Data);

            body.SetPosition(Position);

        }

    }
}
