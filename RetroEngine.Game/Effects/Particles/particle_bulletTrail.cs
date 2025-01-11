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

    [ParticleSys("bulletTrail")]
    public class particle_system_bulletTrail : ParticleSystemEnt
    {

        public particle_system_bulletTrail()
        {
            emitters.Add(new particle_bulletTrail());
        }

    }
    public class particle_bulletTrail : RibbonEmitter
    {
        public particle_bulletTrail():base()
        {
            TexturePath = "particles/trail.png";


            InitialSpawnCount = 3;
            SpawnRate = 30;
            //BoundingRadius = 30000;
            Emitting = true;
            

        }

        public override Particle UpdateParticle(Particle particle)
        {

            //particle.velocity -= new Vector3(0, 2, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);

            //particle.Scale -= Time.DeltaTime/10;

            //particle.Scale = Math.Clamp(particle.Scale, 0.00001f,1);

            //particle.color -= new Vector4(1,1,1,1) * Time.DeltaTime*3;
            //particle.color = particle.color.Clamp(0,1);

            //particle.velocity -= new Vector3(0, 2, 0) * (Time.DeltaTime / 2f);

            //particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime/1.5f, 0);

            return particle;
        }


        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = 0.05f;
            particle.color = new Vector4(0.5f,0,0,0.8f);

            particle.BouncePower = 0.1f;

            particle.transparency = 0.8f;
            particle.deathTime = 1f;

            return particle;
        }
    }
}
