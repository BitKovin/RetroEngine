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
using MonoGame.Extended.Framework.Media;
using System.Threading;

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

        VideoPlayer videoPlayer;
        Video video;
        public BoxDynamic() : base()
        {            

            SaveGame = true;

            soundPlayer = (SoundPlayer)Level.GetCurrent().AddEntity(new SoundPlayer());


            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Master.strings.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/Dialogue_CN.bank");
            AssetRegistry.LoadFmodBankIntoMemory("Sounds/banks/SFX.bank");


            FmodEventInstance = FmodEventInstance.Create("event:/Character/Dialogue");
            FmodEventInstance.SoundTableKey = "welcome";
            //FmodEventInstance.SetProgrammerSound("welcome", AssetRegistry.LoadSoundFmodNativeFromFile("sounds/test.wav"));
            soundPlayer.SetSound(FmodEventInstance);

            soundPlayer.Position = Position;

            //videoPlayer = new VideoPlayer(GameMain.Instance.GraphicsDevice);


        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile("models/cube.obj");

            mesh.texture = AssetRegistry.LoadTextureFromFile("textures/brushes/cat.png");
            //mesh.normalTexture = AssetRegistry.LoadTextureFromFile("textures/foil_n.png");
            //mesh.ormTexture = AssetRegistry.LoadTextureFromFile("textures/foil_orm.png");

            mesh.CastShadows = true;

            //mesh.GenerateSmoothNormals();

            //mesh.CastGeometricShadow = true;

            //video = AssetRegistry.LoadVideoFromFile("test.mp4");

            


            meshes.Add(mesh);

        }

        bool pendingPlay = false;

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

            return;
            if(Thread.CurrentThread == GameMain.RenderThread)
            {

                if(pendingPlay)
                {
                    videoPlayer.Play(video);
                    pendingPlay = false;
                }

                try
                {
                    if(videoPlayer.Video != null)
                        mesh.texture = videoPlayer.GetTexture();
                }catch
                { videoPlayer.Play(video); }
            }

            

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            pendingPlay = true;

            soundPlayer.Position = Position;
            soundPlayer.Play();

        }

        public override void Destroy()
        {

            base.Destroy();

            videoPlayer?.Dispose();
            video?.Dispose();

        }

        public override void Start()
        {
            base.Start();


            body = Physics.CreateBox(this, scale.ToNumerics());
            body.SetPosition(Position.ToNumerics());
            body.SetMassProps(scale.Length() * 10, body.CollisionShape.CalculateLocalInertia(scale.Length() * 10));
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
