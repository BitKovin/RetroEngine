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


    [ParticleSys("decal_blood")]
    public class particle_system_decal_blood : GlobalParticleSystem
    {

        particle_decal_blood particle_blood;

        public particle_system_decal_blood()
        {

            particle_blood = new particle_decal_blood();

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


    public class particle_decal_blood : ParticleEmitter
    {
        public particle_decal_blood()
        {
            TexturePath = "textures/particles/smoke.png";

            InitialSpawnCount = 0;
            SpawnRate = 0;
            BoundingRadius = 300000000;
            Emitting = true;

            DisableSorting = true;

        }

        public override Particle UpdateParticle(Particle particle)
        {

            base.UpdateParticle(particle);

            float incScale = particle.lifeTime / 9f;

            particle.Scale += Time.DeltaTime * MathF.Max(float.Lerp(1, 0.1f, incScale * incScale), 0) * 0.3f;

            const float despawnTime = 4;

            if (particle.deathTime - particle.lifeTime - 0.1f < despawnTime)
                particle.transparency -= Time.DeltaTime/ despawnTime;

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.useGlobalRotation = true;
            particle.globalRotation = MathHelper.FindLookAtRotation(Vector3.Zero, -Rotation);

            particle.position += particle.globalRotation.GetForwardVector() * -0.01f;

            particle.Scale = 1.4f;


            particle.BouncePower = 0.1f;

            particle.deathTime = 30 * random.NextSingle()*3;

            particle.color = new Vector4(1f, 0.0f, 0.0f, 0.7f);

            //particle.OrientRotationToVelocity = true;

            return particle;
        }
    }
}
