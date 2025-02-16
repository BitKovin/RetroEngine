using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Entities;
using RetroEngine.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Effects.Particles
{
    [ParticleSys("meleeTrail")]
    public class particle_system_meleeTrail : ParticleSystemEnt
    {

        public Vector3 Position2;

        particle_meleeTrail particle_MeleeTrail;

        public particle_system_meleeTrail()
        {

            particle_MeleeTrail = new particle_meleeTrail();

            emitters.Add(particle_MeleeTrail);
        }

        public override void AsyncUpdate()
        {

            particle_MeleeTrail.pos2 = Position2;

            base.AsyncUpdate();
        }

    }
    public class particle_meleeTrail : TrailEmitter
    {

        public Vector3 pos2;

        public particle_meleeTrail() : base()
        {
            TexturePath = "particles/trail.png";

            InitialSpawnCount = 2;
            SpawnRate = 60;
            //BoundingRadius = 30000;
            Emitting = true;

            Viewmodel = true;

        }

        public override Particle UpdateParticle(Particle particle)
        {

            //particle.velocity -= new Vector3(0, 2, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);

            //particle.Scale -= Time.DeltaTime;

            particle.Scale = Math.Clamp(particle.Scale, 0, 1);


            //particle.color -= new Vector4(0, 1, 1, 0) * Time.DeltaTime * 10;

            //particle.velocity -= new Vector3(0, 2, 0) * (Time.DeltaTime / 2f);

            //particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime/1.5f, 0);

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.position2 = pos2;


            particle.BouncePower = 0.1f;

            particle.transparency = 1f;
            particle.color = new Vector4(1,0.2f,0.2f,0.4f)/1.5f;
            particle.deathTime = 0.2f;

            return particle;
        }
    }
}
