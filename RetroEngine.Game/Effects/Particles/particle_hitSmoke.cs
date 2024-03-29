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
        }

        public override Particle UpdateParticle(Particle particle)
        {
            particle = base.UpdateParticle(particle);

            particle.transparency = Math.Max(particle.transparency -= Time.deltaTime, 0);

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = 0.2f;

            Vector3 randPos = RandomPosition(0.12f);

            particle.position += randPos;
            particle.velocity = randPos.Normalized()*0.5f;
            particle.transparency = 0.8f;
            particle.deathTime = 1;

            return particle;
        }
    }
}
