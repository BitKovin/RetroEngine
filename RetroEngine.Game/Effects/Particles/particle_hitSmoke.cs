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
    public class particle_hitSmoke : RibbonEmitter
    {
        public particle_hitSmoke()
        {
            TexturePath = "cat.png";

            InitialSpawnCount = 1;
            SpawnRate = 50;
            BoundingRadius = 10;

            

        }

        public override Particle UpdateParticle(Particle particle)
        {

            particle.velocity -= new Vector3(0, 2, 0) * (Time.deltaTime / 2f);

            particle.velocity = new Vector3(0,-5,0);

            particle = base.UpdateParticle(particle);

            particle.velocity -= new Vector3(0, 2, 0) * (Time.deltaTime / 2f);

            particle.transparency = Math.Max(particle.transparency -= Time.deltaTime/1.5f, 0);

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = 0.2f;

            Vector3 randPos = RandomPosition(0.02f);
            //particle.HasCollision = true;
            particle.BouncePower = 0.1f;
            particle.position += randPos;
            particle.velocity = randPos.Normalized()*0.2f;
            particle.transparency = 0.8f;
            particle.deathTime = 1;

            return particle;
        }
    }
}
