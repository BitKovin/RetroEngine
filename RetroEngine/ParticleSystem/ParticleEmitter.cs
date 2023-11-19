using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Particles
{

    

    public struct ParticleEmitterProfile
    {
        public ParticleEmitterProfile()
        {
        }
        public int InitialSpawnCount = 0;
        public float SpawnRate = 1;
        public float SpawnRadius = 1;
        public Vector3 velocity = new Vector3(0,1,0);

        public string texturePath = null;

    }

    public class ParticleEmitter : StaticMesh
    {
        List<Particle> particles = new List<Particle>();

        public ParticleEmitterProfile profile;

        static Model particleModel = null;

        public ParticleEmitter()
        {
            CastShadows = false;
        }

        List<Particle> finalizedParticles;

        int currentId = 0;

        public void Start()
        {
            for (int i = 0; i < profile.InitialSpawnCount; i++) 
            {
                particles.Add(new Particle {id = currentId });
                currentId++;
            }
        }

        public void Update()
        {

            int n = particles.Count;
            for (int i = 0; i < n; i++)
            {

                Particle particle = particles[i];

                particle.lifeTime += Time.deltaTime;

                particles[i] = particle;

                if (particle.lifeTime >= particle.deathTime)
                {
                    particles.Remove(particle);
                }
            }

            Parallel.ForEach(particles, particle =>
            {
                UpdateParticle(particle);
            });

        }

        public virtual void UpdateParticle(Particle particle)
        {
            particle.velocity = profile.velocity;
            particle.position += particle.velocity * Time.deltaTime;
            particle.transparency = (particle.deathTime - particle.lifeTime) / particle.deathTime;
        }

        public override void RenderPreparation()
        {
            base.RenderPreparation();

            finalizedParticles = new List<Particle>(particles);

        }

        Matrix GetWorldForParticle(Particle particle)
        {
            Matrix worldMatrix = Matrix.CreateScale(particle.Scale) *
                                            Matrix.CreateRotationX(Camera.rotation.X / 180 * (float)Math.PI) *
                                            Matrix.CreateRotationY(Camera.rotation.Y / 180 * (float)Math.PI) *
                                            Matrix.CreateRotationZ(Camera.rotation.Z / 180 * (float)Math.PI) *
                                            Matrix.CreateTranslation(particle.position);

            return worldMatrix;
        }
        public override void DrawUnified()
        {
            if (model == null)
            {
                if (particleModel == null)
                {
                    LoadFromFile("models/particle.obj");
                    particleModel = model;
                }

                model = particleModel;
            }


            foreach (var particle in finalizedParticles)
            {
                frameStaticMeshData.model = particleModel;
                frameStaticMeshData.World = GetWorldForParticle(particle);
                frameStaticMeshData.Transparency = particle.transparency;
                base.DrawUnified();
            }
        }

        public override void DrawShadow()
        {
            return; // no shadows for particles
        }

        public void LoadAssets()
        {
            texture = AssetRegistry.LoadTextureFromFile(profile.texturePath);
        }

        public struct Particle
        {
            public Vector3 position = new Vector3();
            public Vector3 velocity = new Vector3();

            public int seed = 0;

            public int id = 0;

            public float lifeTime = 0;
            public float deathTime = 1;

            public float transparency = 1;

            public float Scale = 1;

            public Particle()
            {
            }
        }

    }
}
