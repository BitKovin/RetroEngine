using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.Particles;
using RetroEngine.ParticleSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Effects.Particles
{


    [ParticleSys("hitBlood")]
    public class particle_system_hitBlood : GlobalParticleSystem
    {

        particle_hitBloodPieces particle_blood;
        particle_hitBloodDrips particle_bloodDrips;

        public particle_system_hitBlood()
        {

            particle_blood = new particle_hitBloodPieces();
            particle_bloodDrips = new particle_hitBloodDrips();

            particle_blood.drips = particle_bloodDrips;

            emitters.Add(particle_blood);
            emitters.Add(particle_bloodDrips);
        }

        protected override void EmitAt(Vector3 position, Vector3 orientation, Vector3 Scale)
        {
            base.EmitAt(position, orientation, Scale);

            particle_blood.Position = position;
            particle_blood.Rotation = orientation;
            particle_blood.SpawnParticles((int)Scale.Z * 2);

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
            if(particle.lifeTime<1)
            particle.UserData1 += Time.DeltaTime;

            if (particle.UserData1 > 0.1)
            {
                particle.UserData1 -= 0.1f;

                drips.Position = particle.position;
                drips.Rotation = particle.velocity/3; 
                drips.SpawnParticles(1);
                //DrawDebug.Sphere(0.1f, drips.Position, Vector3.UnitX);
            }

            particle.velocity -= new Vector3(0, 5, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);


            //particle.Scale += Time.DeltaTime * 0.3f;

            particle.velocity -= new Vector3(0, 10, 0) * (Time.DeltaTime / 2f);

            particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime / 4f, 0);

            //particle.SetRotationFromVelocity();

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();



            particle.Scale = 0.7f;// float.Lerp(0.03f, 0.06f, (float)random.NextDouble());

            Vector3 randPos = RandomPosition(0.1f);

            particle.position += randPos;
            particle.velocity = RandomPosition(1f).Normalized()*1.5f + Vector3.UnitY * 2f * float.Lerp(1, 2, (float)random.NextDouble()) + Rotation.GetForwardVector() * 2.5f;

            particle.velocity *= float.Lerp(0.6f, 1f, (float)random.NextDouble());

            particle.transparency = 1.3f;
            particle.deathTime = 3;


            particle.Rotation = random.NextSingle() * 500f;


            particle.color = new Vector4(0.8f, 0.8f, 0.8f, 1);

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

            particle.velocity -= new Vector3(0, 7, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);


            //particle.Scale += Time.DeltaTime * 0.3f;

            particle.velocity -= new Vector3(0, 7, 0) * (Time.DeltaTime / 2f);

            particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime / 3f, 0);

            //particle.SetRotationFromVelocity();

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.velocity = Rotation;

            particle.Scale = float.Lerp(0.15f, 0.25f, (float)random.NextDouble());

            //particle.position += randPos;
            //particle.velocity = RandomPosition(0.4f).Normalized() + Vector3.UnitY * 1.5f * float.Lerp(1, 2, (float)random.NextDouble()) + Rotation.GetForwardVector() * 2.5f;

            //particle.velocity *= float.Lerp(0.6f, 1f, (float)random.NextDouble());

            particle.transparency = 1.3f;
            particle.deathTime = 3;


            particle.Rotation = random.NextSingle() * 500f;


            particle.color = new Vector4(0.8f, 0.8f, 0.8f, 1);

            //particle.OrientRotationToVelocity = true;

            return particle;
        }

    }
}
