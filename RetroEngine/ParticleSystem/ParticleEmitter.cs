using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RetroEngine.Particles.ParticleEmitter;

namespace RetroEngine.Particles
{

    public class ParticleEmitter : StaticMesh
    {
        List<Particle> particles = new List<Particle>();

        public static Model particleModel = null;

        protected Random random = new Random();

        protected int InitialSpawnCount = 1;

        protected string TexturePath = null;

        public static ParticleEmitter RenderEmitter = new ParticleEmitter();

        public bool Destroyed = false;

        public bool Emitting = false;

        public ParticleEmitter()
        {
            CastShadows = false;
            Transperent = true;
        }

        List<Particle> finalizedParticles;

        int currentId = 0;

        public void Start()
        {
            for (int i = 0; i < InitialSpawnCount; i++) 
            {

                Particle particle = GetNewParticle();


                particles.Add(particle);
            }
        }

        public virtual void Update()
        {

            if (Destroyed) return;

            List<Particle> toRemove = new List<Particle>();

            int n = particles.Count;
            for (int i = 0; i < n; i++)
            {

                Particle particle = particles[i];

                particle.lifeTime += Time.deltaTime;

                particles[i] = particle;

                if (particle.lifeTime >= particle.deathTime)
                {
                    toRemove.Add(particle);
                    particles[i] = particle;
                }
            }

            foreach(Particle particle in toRemove)
            {
                particles.Remove(particle);
            }

            for (int i = 0; i < particles.Count; i++)
            {
                particles[i] = UpdateParticle(particles[i]);
            }

            if (Emitting == false && particles.Count == 0)
                Destroyed = true;
        }

        public virtual Particle UpdateParticle(Particle particle)
        {
            particle.position += particle.velocity * Time.deltaTime;

            return particle;
        }

        public virtual Particle GetNewParticle()
        {
            currentId++;
            return new Particle {position = Position, id = currentId, texturePath = TexturePath };
        }

        public override void RenderPreparation()
        {
            base.RenderPreparation();

            finalizedParticles = new List<Particle>(particles);
        }

        Matrix GetWorldForParticle(Particle particle)
        {

            if (particle.Scale > 0.5f)
                Console.WriteLine(particle.Scale);

            Matrix worldMatrix = Matrix.CreateScale(particle.Scale) *
                                            Matrix.CreateRotationZ(particle.Rotation) *
                                            Matrix.CreateRotationX(Camera.rotation.X / 180 * (float)Math.PI) *
                                            Matrix.CreateRotationY(Camera.rotation.Y / 180 * (float)Math.PI) *
                                            Matrix.CreateTranslation(particle.position);

            return worldMatrix;
        }
        public override void DrawUnified()
        {
            if (Destroyed) return;
            if (particleModel == null)
            {
                LoadFromFile("models/particle.obj");
                particleModel = model;
            }
            GameMain.inst.render.particlesToDraw.AddRange(finalizedParticles);
        }

        public static void LoadRenderEmitter()
        {
                RenderEmitter.model = particleModel;
                RenderEmitter.RenderPreparation();
        }

        public void DrawParticles(List<Particle> particleList)
        {
            particleList = particleList.OrderByDescending(p => Vector3.Dot(p.position - Camera.position, Camera.rotation.GetForwardVector())).ToList();

            foreach (var particle in particleList)
            {
                texture = AssetRegistry.LoadTextureFromFile(particle.texturePath);
                
                frameStaticMeshData.model = particleModel;
                frameStaticMeshData.World = GetWorldForParticle(particle);
                frameStaticMeshData.Transparency = particle.transparency;

                base.DrawUnified();
            }
        }

        public override void DrawShadow(bool close = false)
        {
            return; // no shadows for particles
        }

        public void LoadAssets()
        {
            texture = AssetRegistry.LoadTextureFromFile(TexturePath);
        }

        protected Vector3 RandomPosition(float radius)
        {
            // Generate random values for x, y, and z within the specified radius
            float x = random.NextSingle() - 0.5f;
            float y = random.NextSingle() - 0.5f;
            float z = random.NextSingle() - 0.5f;

            Vector3 dir = new Vector3(x, y, z);

            dir.Normalize();
            dir *= random.NextSingle();
            dir *= radius;

            return dir;
        }

        public void Destroy()
        {

        }

        public struct Particle
        {
            public Vector3 position = new Vector3();
            public Vector3 velocity = new Vector3();

            public int seed = 0;

            public int id = 0;

            public float lifeTime = 0;
            public float deathTime = 2;

            public float transparency = 1;

            public float Scale = 0;
            public float Rotation = 0;
            public bool OrientRotationToVelocity = false;

            public string texturePath = null; //do not set texture on game thread

            public Particle()
            {
            }
        }

    }
}
