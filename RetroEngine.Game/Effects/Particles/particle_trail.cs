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

    [ParticleSystem("trail")]
    public class particle_system_trail : ParticleSystem
    {

        public particle_system_trail()
        {
            emitters.Add(new particle_trail());
        }

    }
    public class particle_trail : RibbonEmitter
    {
        public particle_trail():base()
        {
            TexturePath = "particles/trail.png";

            InitialSpawnCount = 2;
            SpawnRate = 30;
            //BoundingRadius = 30000;
            Emitting = true;
            

        }

        public override Particle UpdateParticle(Particle particle)
        {

            //particle.velocity -= new Vector3(0, 2, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);

            particle.Scale -= Time.DeltaTime/10;

            particle.Scale = Math.Clamp(particle.Scale, 0,1);

            //particle.color = Color.Red;

            //particle.velocity -= new Vector3(0, 2, 0) * (Time.DeltaTime / 2f);

            //particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime/1.5f, 0);

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = 0.05f;

            particle.BouncePower = 0.1f;

            particle.transparency = 0.8f;
            particle.deathTime = 1.3f;

            return particle;
        }
    }
}
