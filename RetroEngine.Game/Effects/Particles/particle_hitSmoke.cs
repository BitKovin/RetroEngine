using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Effects.Particles
{

    [ParticleSystem("hitSmoke")]
    public class particle_system_hitSmoke : ParticleSystem
    {

        public particle_system_hitSmoke()
        {
            emitters.Add(new particle_hitSmoke());
        }

    }
    public class particle_hitSmoke : ParticleEmitter
    {
        public particle_hitSmoke()
        {
            TexturePath = "textures/particles/smoke.png";

            InitialSpawnCount = 10;
            //SpawnRate = 1000;
            BoundingRadius = 3;

            

        }

        public override Particle UpdateParticle(Particle particle)
        {

            particle.velocity -= new Vector3(0, 2, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);

            particle.velocity -= new Vector3(0, 2, 0) * (Time.DeltaTime / 2f);

            particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime/1.5f, 0);

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = 0.2f;

            Vector3 randPos = RandomPosition(0.12f);
            //particle.HasCollision = true;
            particle.BouncePower = 0.1f;
            particle.position += randPos;
            particle.velocity = randPos.Normalized()*0.5f;
            particle.transparency = 0.8f;
            particle.deathTime = 2;

            return particle;
        }
    }
}
