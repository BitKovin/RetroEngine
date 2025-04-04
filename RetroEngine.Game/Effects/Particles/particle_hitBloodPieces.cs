﻿using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.Particles;
using RetroEngine.ParticleSystem;
using RetroEngine.PhysicsSystem;
using RetroEngine.SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Game.Effects.Particles
{


    [ParticleSys("hitBlood")]
    [LevelObject("hitBlood")]
    public class particle_system_hitBlood : GlobalParticleSystem
    {

        particle_hitBloodPieces particle_blood;
        particle_hitBloodDrips particle_bloodDrips;

        public particle_system_hitBlood() : base()
        {

            particle_blood = new particle_hitBloodPieces();
            particle_bloodDrips = new particle_hitBloodDrips();

            particle_blood.drips = particle_bloodDrips;

            emitters.Add(particle_blood);
            emitters.Add(particle_bloodDrips);

            SaveGame = true;

        }

        protected override void EmitAt(Vector3 position, Vector3 orientation, Vector3 Scale)
        {
            base.EmitAt(position, orientation, Scale);

            particle_blood.Position = position;
            particle_blood.Rotation = orientation;
            particle_blood.SpawnParticles((int)(Scale.Z * 1f));

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            ParticleSystemEnt.Preload("decal_blood");

        }


        [JsonInclude]
        public ParticleEmitter.ParticleEmitterSaveData bloodSaveData = new ParticleEmitter.ParticleEmitterSaveData();

        [JsonInclude]
        public ParticleEmitter.ParticleEmitterSaveData piecesSaveData = new ParticleEmitter.ParticleEmitterSaveData();

        protected override EntitySaveData SaveData(EntitySaveData baseData)
        {

            bloodSaveData = particle_bloodDrips.GetSaveData();
            piecesSaveData = particle_blood.GetSaveData();

            return base.SaveData(baseData);
        }

        public override void LoadData(EntitySaveData Data)
        {
            base.LoadData(Data);

            particle_bloodDrips.LoadData(bloodSaveData);
            particle_blood.LoadData(piecesSaveData);

        }

    }


    public class particle_hitBloodPieces : ParticleEmitter
    {

        public particle_hitBloodDrips drips;

        public particle_hitBloodPieces()
        {
            TexturePath = "textures/particles/blood.png";


            InitialSpawnCount = 0;
            SpawnRate = 0;
            BoundingRadius = 300000000;
            Emitting = true;

        }

        public override Particle UpdateParticle(Particle particle)
        {
            if(particle.lifeTime<1.5)
            particle.UserData1 += Time.DeltaTime;

            const float spawnInterval = 0.1f;

            
            if (particle.UserData1 > spawnInterval)
            {
                particle.UserData1 -= spawnInterval;

                drips.Position = particle.position;
                drips.Rotation = particle.velocity/3; 
                drips.SpawnParticles(1);
                //DrawDebug.Sphere(0.1f, drips.Position, Vector3.UnitX);
            }

            particle.velocity -= new Vector3(0, 10, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);

            if(particle.position.Y < particle.UserData3 && particle.UserData2 < 2)
            {

                if (random.NextSingle() < ((particle.UserData2 == 0) ? 0.5f : 0.5f))
                {

                    var hit = Physics.LineTraceForStatic((particle.position - particle.velocity.Normalized() * 0.2f).ToPhysics(), (particle.position + particle.velocity.Normalized()).ToPhysics());

                    if (hit.HasHit)
                    {

                        if(Vector3.Distance(hit.HitPointWorld, particle.position2) > 0.25f)

                        particle_system_decal_blood.EmitAt("decal_blood", hit.HitPointWorld, hit.HitNormalWorld, Vector3.One);
                    }

                }

                if (particle.UserData2 < 1)
                {
                    particle.position.Y = particle.UserData3 + particle.CollisionRadius;
                    particle.velocity.Y = particle.velocity.Y * particle.BouncePower * -1;
                }
                particle.UserData2++;

            }

            //particle.Scale += Time.DeltaTime * 0.3f;

            particle.velocity -= new Vector3(0, 10, 0) * (Time.DeltaTime / 2f);

            particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime / 4f, 0);

            //particle.SetRotationFromVelocity();

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            //particle.HasCollision = (0.3f < random.NextSingle());
            particle.BouncePower = 0.5f;
            particle.CollisionRadius = 0.2f;
            particle.Scale = 0.6f;// float.Lerp(0.03f, 0.06f, (float)random.NextDouble());

            particle.UserData3 = -10000000;

            particle.position2 = Position;

            Vector3 randPos = RandomPosition(0.1f);

            particle.position += randPos;
            particle.velocity = RandomPosition(1f).Normalized()*2.5f + Vector3.UnitY * 2f * float.Lerp(1, 2, (float)random.NextDouble()) + Rotation.GetForwardVector() * 2.5f;

            particle.velocity *= float.Lerp(0.3f, 1f, (float)random.NextDouble());

            particle.transparency = 1.4f;
            particle.deathTime = 3;


            particle.Rotation = random.NextSingle() * 500f;


            particle.color = new Vector4(0.65f, 0.65f, 0.65f, 1);


            Vector3 floorCheckPos = particle.position + particle.velocity/2f;

            var hit = Physics.LineTraceForStatic(floorCheckPos.ToPhysics(), (floorCheckPos - Vector3.UnitY * 10).ToPhysics());

            if (hit.HasHit)
            {
                particle.UserData3 = hit.HitPointWorld.Y;
            }

            //particle.OrientRotationToVelocity = true;

            return particle;
        }

    }


    public class particle_hitBloodDrips : ParticleEmitter
    {
        public particle_hitBloodDrips()
        {
            TexturePath = "textures/particles/blood.png";

            InitialSpawnCount = 0;
            SpawnRate = 0;
            BoundingRadius = 300000000;
            Emitting = true;


        }

        public override Particle UpdateParticle(Particle particle)
        {

            particle.velocity -= new Vector3(0, 6, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);


            //particle.Scale += Time.DeltaTime * 0.3f;

            particle.velocity -= new Vector3(0, 6, 0) * (Time.DeltaTime / 2f);

            particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime / 1.5f, 0);

            //particle.SetRotationFromVelocity();

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.velocity = Rotation;

            particle.Scale = float.Lerp(0.15f, 0.2f, (float)random.NextDouble());

            //particle.position += randPos;
            //particle.velocity = RandomPosition(0.4f).Normalized() + Vector3.UnitY * 1.5f * float.Lerp(1, 2, (float)random.NextDouble()) + Rotation.GetForwardVector() * 2.5f;

            //particle.velocity *= float.Lerp(0.6f, 1f, (float)random.NextDouble());

            particle.transparency = 1.4f;
            particle.deathTime = 1f;


            particle.Rotation = random.NextSingle() * 500f;


            particle.color = new Vector4(0.55f, 0.55f, 0.55f, 1);

            //particle.OrientRotationToVelocity = true;

            return particle;
        }

    }
}
