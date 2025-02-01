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


    [ParticleSys("hitDust")]
    public class particle_system_hitSmokeGlobal : GlobalParticleSystem
    {

        particle_hitSmokeGlobal particle_blood;

        public particle_system_hitSmokeGlobal()
        {

            particle_blood = new particle_hitSmokeGlobal();

            emitters.Add(particle_blood);
        }

        protected override void EmitAt(Vector3 position, Vector3 orientation, Vector3 Scale)
        {
            base.EmitAt(position, orientation, Scale);

            particle_blood.Position = position;
            particle_blood.Rotation = orientation;
            particle_blood.SpawnParticles((int)Scale.Z);

        }

    }


    public class particle_hitSmokeGlobal : ParticleEmitter
    {
        public particle_hitSmokeGlobal()
        {
            TexturePath = "textures/particles/smoke.png";

            InitialSpawnCount = 0;
            SpawnRate = 0;
            BoundingRadius = 300000000;
            Emitting = true;
            

        }

        public override Particle UpdateParticle(Particle particle)
        {

            particle.velocity -= new Vector3(0, 3, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);

            particle.Scale += Time.DeltaTime * 0.5f;

            particle.velocity -= new Vector3(0, 3, 0) * (Time.DeltaTime / 2f);

            particle.transparency = Math.Max(particle.transparency -= Time.DeltaTime/1f, 0);

            //particle.SetRotationFromVelocity();

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = 0.2f;

            Vector3 randPos = RandomPosition(0.6f);
            //particle.HasCollision = true;
            particle.BouncePower = 0.1f;
            particle.velocity = randPos.Normalized() / 6f + Vector3.UnitY*0.5f + Rotation.GetForwardVector()*0.3f;
            particle.transparency = 1.2f;
            particle.deathTime = 5;


            particle.Rotation = random.NextSingle()*500f;

            particle.color = new Vector4(0.8f, 0.8f, 0.8f, 0.6f);

            //particle.OrientRotationToVelocity = true;

            return particle;
        }
    }
}
