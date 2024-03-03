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

        static Model particleModel = null;

        protected Random random = new Random();

        protected int InitialSpawnCount = 1;

        protected string TexturePath = null;

        protected string ModelPath = null;

        public static ParticleEmitter RenderEmitter = new ParticleEmitter();

        public bool Destroyed = false;

        public bool Emitting = false;

        public ParticleEmitter()
        {
            CastShadows = false;
            Transperent = true;
            isParticle = true;

            SimpleTransperent = true;

            OverrideBlendState = BlendState.NonPremultiplied;

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
            if (particle.useGlobalRotation == false)
            {
                Matrix worldMatrix = Matrix.CreateScale(particle.Scale) *
                                                Matrix.CreateRotationZ(particle.Rotation) *
                                                Matrix.CreateRotationX(Camera.rotation.X / 180 * (float)Math.PI) *
                                                Matrix.CreateRotationY(Camera.rotation.Y / 180 * (float)Math.PI) *
                                                Matrix.CreateTranslation(particle.position);

                return worldMatrix;
            }else
            {
                Matrix worldMatrix = Matrix.CreateScale(particle.Scale) *
                                Matrix.CreateRotationX(particle.globalRotation.X / 180 * (float)Math.PI) *
                                Matrix.CreateRotationY(particle.globalRotation.Y / 180 * (float)Math.PI) *
                                Matrix.CreateRotationZ(particle.globalRotation.Z / 180 * (float)Math.PI) *
                                Matrix.CreateTranslation(particle.position);

                return worldMatrix;
            }
        }
        public override void DrawUnified()
        {
            if (Destroyed) return;
            if (particleModel == null)
            {
                particleModel = GetModelFromPath("models/particle.obj");
            }

            DrawParticles(finalizedParticles);

            //GameMain.Instance.render.particlesToDraw.AddRange(finalizedParticles);
        }

        public override void Draw()
        {
            if (Destroyed) return;
            if (particleModel == null)
            {
                particleModel = GetModelFromPath("models/particle.obj");
            }

            DrawParticles(finalizedParticles);

            //GameMain.Instance.render.particlesToDraw.AddRange(finalizedParticles);
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

                frameStaticMeshData.model = (particle.customModelPath == null) ? particleModel : GetModelFromPath(particle.customModelPath);
                frameStaticMeshData.World = GetWorldForParticle(particle);
                frameStaticMeshData.Transparency = particle.transparency;
                frameStaticMeshData.Transperent = true;
                frameStaticMeshData.IsRendered = true;

                isParticle = particle.customModelPath == null;

                base.DrawUnified();
            }
        }

        public void DrawParticlesPathes(List<Particle> particleList)
        {
            particleList = particleList.OrderByDescending(p => Vector3.Dot(p.position - Camera.position, Camera.rotation.GetForwardVector())).ToList();

            foreach (var particle in particleList)
            {
                texture = AssetRegistry.LoadTextureFromFile(particle.texturePath);
                
                frameStaticMeshData.model = (particle.customModelPath == null) ? particleModel : GetModelFromPath(particle.customModelPath);
                frameStaticMeshData.World = GetWorldForParticle(particle);
                frameStaticMeshData.Transparency = particle.transparency;

                DrawColor();
            }
        }

        public void DrawColor()
        {
            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.ParticleColorEffect;

            if (model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        effect.Parameters["Texture"].SetValue(texture);

                        effect.Parameters["WorldViewProjection"].SetValue(frameStaticMeshData.World * frameStaticMeshData.View * frameStaticMeshData.Projection);

                        effect.Parameters["Transparency"].SetValue(frameStaticMeshData.Transparency);

                        // Draw the primitives using the custom effect
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount);
                        }
                    }
                }
            }
        }

        public void DrawParticlesNormal(List<Particle> particleList)
        {
            particleList = particleList.OrderByDescending(p => Vector3.Dot(p.position - Camera.position, Camera.rotation.GetForwardVector())).ToList();

            foreach (var particle in particleList)
            {
                texture = AssetRegistry.LoadTextureFromFile(particle.texturePath);

                frameStaticMeshData.model = (particle.customModelPath == null) ? particleModel : GetModelFromPath(particle.customModelPath);
                frameStaticMeshData.World = GetWorldForParticle(particle);
                frameStaticMeshData.Transparency = particle.transparency;

                base.DrawNormals();
            }
        }

        public override void DrawShadow(bool close = false)
        {
            return; // no shadows for particles
        }

        public void LoadAssets()
        {
            GetModelFromPath("models/particle.obj");

            texture = AssetRegistry.LoadTextureFromFile(TexturePath);
            if(ModelPath is not null)
                GetModelFromPath(ModelPath);
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

            public Vector3 globalRotation = new Vector3();
            public bool useGlobalRotation = false;

            public string customModelPath = null;

            public int seed = 0;

            public int id = 0;

            public float lifeTime = 0;
            public float deathTime = 2;

            public float transparency = 1;

            public float Scale = 0;
            public float Rotation = 0;
            public bool OrientRotationToVelocity = false;

            public string texturePath = null;

            public Particle()
            {
            }
        }

    }
}
