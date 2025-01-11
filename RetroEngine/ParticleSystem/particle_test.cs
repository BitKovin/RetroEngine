using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Particles
{

    [ParticleSys("testSystem")]
    public class particle_system_test : ParticleSystemEnt
    {

        public particle_system_test()
        {
            emitters.Add(new particle_test());
        }

    }

    public class particle_test : ParticleEmitter
    {
        public particle_test()
        {
            TexturePath = "textures/particles/wood.png";

            ModelPath = "models/particles/wood.obj";

            InitialSpawnCount = 1;
        }

        public override Particle UpdateParticle(Particle particle)
        {

            particle.velocity -= new Vector3(0, 10, 0) * (Time.DeltaTime / 2f);

            particle = base.UpdateParticle(particle);

            particle.velocity -= new Vector3 (0, 10, 0) * (Time.DeltaTime / 2f);

            particle.globalRotation += new Vector3(300,100, 0) * Time.DeltaTime;

            return particle;
        }

        public override Particle GetNewParticle()
        {
            Particle particle = base.GetNewParticle();

            particle.Scale = MathHelper.Lerp(0.5f,1, (float)random.NextDouble());

            Vector3 randPos = RandomPosition(0.3f);

            particle.position += randPos;
            particle.velocity = randPos.Normalized() * 0.2f + new Vector3(0,2,0);
            particle.deathTime = 3;

            particle.globalRotation = RandomPosition(500);

            particle.customModelPath = "models/particles/wood.obj";
            particle.useGlobalRotation = true;

            return particle;
        }
    }
}
