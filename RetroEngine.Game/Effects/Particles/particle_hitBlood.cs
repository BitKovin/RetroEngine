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

        particle_hitBlood particle_blood;

        public particle_system_hitBlood()
        {

            particle_blood = new particle_hitBlood();

            emitters.Add(particle_blood);
        }

        protected override void EmitAt(Vector3 position, Vector3 orientation, Vector3 Scale)
        {
            base.EmitAt(position, orientation, Scale);

            particle_blood.Position = position;
            particle_blood.Rotation = orientation;
            particle_blood.SpawnParticles((int)Scale.Z*3);

        }

    }


    public class particle_hitBlood : ParticleEmitter
    {
        public particle_hitBlood()
        {
            TexturePath = "textures/particles/blood.png";

            InitialSpawnCount = 0;
            SpawnRate = 0;
            BoundingRadius = 300000000;
            Emitting = true;
            

        }

        public override Particle UpdateParticle(Particle particle)
        {

            particle.velocity -= new Vector3(0, 10, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);

            if(particle.lifeTime < 0.3)
                particle.Scale += Time.DeltaTime * 2f;

            particle.Scale += Time.DeltaTime * 0.7f;

            particle.velocity -= new Vector3(0, 10, 0) * (Time.DeltaTime / 2f);

            particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime/4f, 0);

            //particle.SetRotationFromVelocity();

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = float.Lerp(0.4f,0.6f, (float)random.NextDouble());

            particle.Scale = 0.05f;

            Vector3 randPos = RandomPosition(0.4f);
            //particle.HasCollision = true;
            particle.BouncePower = 0.1f;
            particle.position += randPos;
            particle.velocity = RandomPosition(0.4f).Normalized() / 1f + Vector3.UnitY * float.Lerp(1,2,(float)random.NextDouble())  + Rotation.GetForwardVector()*0.7f;

            particle.velocity*= float.Lerp(0.3f, 1f, (float)random.NextDouble());

            particle.transparency = 1.2f;
            particle.deathTime = 5;


            particle.Rotation = random.NextSingle()*500f;

            Console.WriteLine(particle.Rotation);

            particle.color = new Vector4(0.8f, 0.8f, 0.8f, 1);

            //particle.OrientRotationToVelocity = true;

            return particle;
        }
    }
}
