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

        public bool destroyed = false;

        public bool Emitting = false;

        public float BoundingRadius = 1000;

        float elapsedTime = 0;

        public float Duration = 10000000000000;

        public float SpawnRate = 0;

        public ParticleEmitter()
        {
            CastShadows = false;
            Transperent = true;

            SimpleTransperent = true;

            OverrideBlendState = BlendState.NonPremultiplied;

            Shader = AssetRegistry.GetShaderFromName("ParticleUnifiedOutput");

        }

        protected List<Particle> finalizedParticles;

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

            if (destroyed) return;

            elapsedTime += Time.DeltaTime;

            if(elapsedTime>Duration)
                Emitting = false;

            float spawnInterval = 1f / SpawnRate;

            if(SpawnRate>0 && Emitting)
            while (elapsedTime >= spawnInterval)
            {
                Particle particle = GetNewParticle();
                particles.Add(particle);
                elapsedTime -= spawnInterval;
            }

            List<Particle> toRemove = new List<Particle>();

            int n = particles.Count;
            for (int i = 0; i < n; i++)
            {

                Particle particle = particles[i];

                particle.lifeTime += Time.DeltaTime;

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

            finalizedParticles = new List<Particle>(particles);

            if (Emitting == false && particles.Count == 0)
            {
                destroyed = true;
            }
        }

        public override void UpdateCulling()
        {
            inFrustrum = true;
        }
        public virtual Particle UpdateParticle(Particle particle)
        {

            Vector3 oldPos = particle.position;

            particle.position += particle.velocity * Time.DeltaTime;

            particle.Collided = false;

            if(particle.HasCollision == false) return particle;

            var hit = Physics.SphereTraceForStatic(oldPos.ToPhysics(), particle.position.ToPhysics(), particle.CollisionRadius);

            if(hit.HasHit == false) return particle;

            particle.Collided = true;

            particle.position = hit.HitPointWorld;

            if(Vector3.Dot(particle.velocity, hit.HitNormalWorld)<0)

            particle.velocity = Vector3.Reflect(particle.velocity * particle.BouncePower, hit.HitNormalWorld);
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

            UpdateInstancedData(finalizedParticles);

            if (instanceData == null) return;

            instanceBuffer?.Dispose();

            instanceBuffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, InstanceData.VertexDeclaration, instanceData.Length, BufferUsage.None);

            instanceBuffer.SetData(instanceData);

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
            if (destroyed) return;
            if (particleModel == null)
            {
                particleModel = GetModelFromPath("models/particle.obj");
            }

            if(Camera.frustum.Contains(new BoundingSphere(Position, BoundingRadius))!= ContainmentType.Disjoint)
            DrawParticles();

            //GameMain.Instance.render.particlesToDraw.AddRange(finalizedParticles);
        }

        public override void Draw()
        {
            if (destroyed) return;
            if (particleModel == null)
            {
                particleModel = GetModelFromPath("models/particle.obj");
            }

            DrawParticles();

            //GameMain.Instance.render.particlesToDraw.AddRange(finalizedParticles);
        }

        public static void LoadRenderEmitter()
        {
                RenderEmitter.model = particleModel;
                RenderEmitter.RenderPreparation();
        }

        VertexBuffer instanceBuffer;
        InstanceData[] instanceData;
        public void DrawParticles()
        {
            if (instanceBuffer != null)
            DrawInstanced(instanceBuffer, instanceData.Length);

        }

        void UpdateInstancedData(List<Particle> particleList)
        {

            if (particleList == null) return;
            if (particleList.Count == 0) return;

            var particles = particleList;

            particles = particles.OrderByDescending(p => Vector3.Dot(p.position - Camera.position, Camera.rotation.GetForwardVector())).ToList();

            var instanceData_new = new InstanceData[particles.Count];


            texture = AssetRegistry.LoadTextureFromFile(particles[0].texturePath);
            frameStaticMeshData.model = (particles[0].customModelPath == null) ? particleModel : GetModelFromPath(particles[0].customModelPath);


            int i = -1;
            foreach (var particle in particles)
            {
                i++;
                if (particle.transparency <= 0)
                    continue;

                if (particle.lifeTime >= particle.deathTime)
                    continue;

                Matrix world = GetWorldForParticle(particle);

                isParticle = particle.customModelPath == null;

                InstanceData data = new InstanceData();
                data.Row1 = world.GetRow(0);
                data.Row2 = world.GetRow(1);
                data.Row3 = world.GetRow(2);
                data.Row4 = world.GetRow(3);

                data.Color = particle.color;

                data.Color.W = data.Color.W * particle.transparency;

                instanceData_new[i] = data;

            }

            instanceData = instanceData_new.ToArray();

        }

        void DrawInstanced(VertexBuffer instanceBuffer, int count)
        {
            if ((frameStaticMeshData.IsRendered == false) && frameStaticMeshData.Viewmodel == false || occluded) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = Shader;

            effect.Parameters["isParticle"].SetValue(isParticle);

            GameMain.Instance.render.UpdateDataForShader((RetroEngine.Graphic.Shader)effect);

            SetupBlending();

            if (frameStaticMeshData.model is not null)
            {
                foreach (ModelMesh mesh in frameStaticMeshData.model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {

                        // Set the vertex buffer and index buffer for this mesh part
                        //graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        var bindings = new VertexBufferBinding[2];
                        bindings[0] = new VertexBufferBinding(meshPart.VertexBuffer);
                        bindings[1] = new VertexBufferBinding(instanceBuffer, 0, 1);

                       

                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                        ApplyShaderParams(effect, meshPartData);

                        Stats.RenderedMehses++;


                        // Draw the primitives using the custom effect
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            graphicsDevice.SetVertexBuffers(bindings);

                            graphicsDevice.DrawInstancedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart.VertexOffset,
                                meshPart.StartIndex,
                                meshPart.PrimitiveCount, count);
                        }
                    }
                }
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

        public override void DrawShadow(bool close = false, bool veryClose = false)
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

        public override Vector3 GetClosestToCameraPosition()
        {

            if (finalizedParticles.Count == 0) return Vector3.Zero;

            List<Particle> particles = finalizedParticles.OrderBy(p => Vector3.Distance( p.position, Camera.position)).ToList();

            return particles[0].position;

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

            public Vector4 color = Vector4.One;

            public float Scale = 0;
            public float Rotation = 0;
            public bool OrientRotationToVelocity = false;

            public string texturePath = null;

            public bool HasCollision = false;
            public float CollisionRadius = 0.02f;
            public float BouncePower = 0.8f;

            public bool Collided = false;

            public Particle()
            {
            }
        }

    }
}
