using Microsoft.Xna.Framework;
using RetroEngine.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Effects.Particles
{
    public class particle_hitSmoke : ParticleEmitter
    {
        public particle_hitSmoke()
        {
            TexturePath = "textures/particles/smoke.png";

            InitialSpawnCount = 3;
        }

        public override Particle UpdateParticle(Particle particle)
        {
            particle = base.UpdateParticle(particle);

            particle.transparency = particle.transparency -= Time.deltaTime;

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = 0.2f;

            particle.position += RandomPosition(0.12f);
            particle.velocity = RandomPosition(1).Normalized()*0.5f;
            particle.transparency = 0.8f;
            particle.deathTime = 1;

            return particle;
        }
    }
}
