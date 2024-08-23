using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.PhysicsSystem;
using RetroEngine.UI;
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
        protected List<Particle> particles = new List<Particle>();

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

        public float ParticleSizeMultiplier = 1;

        protected static List<VertexBuffer> freeVertexBuffers = new List<VertexBuffer>();
        protected static List<VertexBuffer> freeIstanceBuffers = new List<VertexBuffer>();
        protected static List<IndexBuffer> freeIndexBuffers = new List<IndexBuffer>();

        protected VertexBuffer ReuseOrCreateVertexBuffer(GraphicsDevice graphicsDevice, int requiredVertexCount)
        {
            // Try to reuse a buffer if available
            VertexBuffer buffer = freeVertexBuffers.FirstOrDefault(vb => vb.VertexCount >= requiredVertexCount);
            if (buffer != null)
            {
                freeVertexBuffers.Remove(buffer);
                return buffer;
            }

            return new VertexBuffer(graphicsDevice, VertexData.VertexDeclaration, requiredVertexCount, BufferUsage.WriteOnly);
        }

        protected VertexBuffer ReuseOrCreateInstanceBuffer(GraphicsDevice graphicsDevice, int requiredVertexCount)
        {
            // Try to reuse a buffer if available
            VertexBuffer buffer = freeIstanceBuffers.FirstOrDefault(vb => vb.VertexCount >= requiredVertexCount);
            if (buffer != null)
            {
                freeIstanceBuffers.Remove(buffer);
                return buffer;
            }

            return new VertexBuffer(graphicsDevice, InstanceData.VertexDeclaration, requiredVertexCount, BufferUsage.WriteOnly);
        }

        protected IndexBuffer ReuseOrCreateIndexBuffer(GraphicsDevice graphicsDevice, int requiredIndexCount)
        {
            // Try to reuse a buffer if available
            IndexBuffer buffer = freeIndexBuffers.FirstOrDefault(ib => ib.IndexCount >= requiredIndexCount);
            if (buffer != null)
            {
                freeIndexBuffers.Remove(buffer);
                return buffer;
            }

            return new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, requiredIndexCount, BufferUsage.WriteOnly);
        }

        public ParticleEmitter()
        {
            CastShadows = false;
            Transperent = true;

            SimpleTransperent = true;

            OverrideBlendState = BlendState.NonPremultiplied;

            DisableOcclusionCulling = true;

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

            if (elapsedTime > Duration)
                Emitting = false;

            float spawnInterval = 1f / SpawnRate;
            lock (particles)
            {
                if (SpawnRate > 0 && Emitting)
                    while (elapsedTime >= spawnInterval)
                    {
                        Particle particle = GetNewParticle();
                        particles.Add(particle);
                        elapsedTime -= spawnInterval;
                    }

                

                int n = particles.Count;
                for (int i = 0; i < n; i++)
                {

                    Particle particle = particles[i];

                    particle.lifeTime += Time.DeltaTime;

                    if (particle.lifeTime >= particle.deathTime)
                    {
                        particle.destroyed = true;
                        particles[i] = particle;
                    }
                }

                particles = particles.Where(p=> p.destroyed ==false).ToList();

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

            if (particle.HasCollision == false) return particle;

            var hit = Physics.SphereTraceForStatic(oldPos.ToPhysics(), particle.position.ToPhysics(), particle.CollisionRadius);

            if (hit.HasHit == false) return particle;

            particle.Collided = true;

            particle.position = hit.HitPointWorld;

            if (Vector3.Dot(particle.velocity, hit.HitNormalWorld) < 0)
            {
                particle.velocity = Vector3.Reflect(particle.velocity * particle.BouncePower, hit.HitNormalWorld);
                particle.position = hit.HitPointWorld + hit.HitNormalWorld*(particle.CollisionRadius+0.02f);
            }
            return particle;

        }

        public virtual Particle GetNewParticle()
        {
            currentId++;
            return new Particle { position = Position, id = currentId, texturePath = TexturePath, globalRotation = Rotation };
        }

        public override void RenderPreparation()
        {
            base.RenderPreparation();

            UpdateInstancedData(finalizedParticles);

            if (instanceData == null) return;

            if(instanceBuffer == null || instanceBuffer.VertexCount != instanceData.Length)
            {
                FreeBuffers();

                instanceBuffer = ReuseOrCreateInstanceBuffer(GameMain.Instance.GraphicsDevice, instanceData.Length);

            }

            instanceBuffer.SetData(instanceData);

        }

        private void FreeBuffers()
        {
            // Return buffers to the pool for reuse
            if (instanceBuffer != null)
            {
                freeIstanceBuffers.Add(instanceBuffer);
                instanceBuffer = null;
            }

        }

        Matrix GetWorldForParticle(Particle particle)
        {

            if(particle.OrientRotationToVelocity)
            {

                // Normalize the velocity vector to use it as the right vector (X-axis)
                Vector3 right = Vector3.Normalize(particle.velocity);

                // Calculate the direction from the particle to the camera
                Vector3 cameraToParticle = Vector3.Normalize(Camera.position - particle.position) * -1;

                // Use the camera direction as the up vector (Y-axis), making sure it is perpendicular to the right vector
                Vector3 up = Vector3.Cross(cameraToParticle, right);

                // Recalculate the forward vector (Z-axis) to ensure it is perpendicular to both the right and up vectors
                Vector3 forward = Vector3.Cross(right, up);

                // If the forward vector is invalid, adjust it
                if (forward.LengthSquared() == 0)
                {
                    forward = cameraToParticle;
                }

                // Construct the rotation matrix from the right, up, and forward vectors
                Matrix rotationMatrix = new Matrix(
                    right.X, right.Y, right.Z, 0,
                    up.X, up.Y, up.Z, 0,
                    forward.X, forward.Y, forward.Z, 0,
                    0, 0, 0, 1
                );

                // Scale matrix
                Matrix scaleMatrix = Matrix.CreateScale(particle.Scale);

                // Translation matrix
                Matrix translationMatrix = Matrix.CreateTranslation(particle.position);

                // Combine scale, rotation, and translation matrices
                Matrix worldMatrix = scaleMatrix * rotationMatrix * translationMatrix;

                return worldMatrix;

            }

            if (particle.useGlobalRotation == false)
            {
                Matrix worldMatrix = Matrix.CreateScale(particle.Scale) *
                                                Matrix.CreateRotationZ(particle.Rotation) *
                                                Matrix.CreateRotationX(Camera.rotation.X / 180 * (float)Math.PI) *
                                                Matrix.CreateRotationY(Camera.rotation.Y / 180 * (float)Math.PI) *
                                                Matrix.CreateTranslation(particle.position);

                return worldMatrix;
            }
            else
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
            if (particleModel == null || particleModel.Meshes[0].MeshParts[0].IndexBuffer.IsDisposed)
            {
                particleModel = GetModelFromPath("models/particle.obj");
            }

            if (Camera.frustum.Contains(new BoundingSphere(Position, BoundingRadius)) != ContainmentType.Disjoint)
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

        public override void DrawDepth(bool pointLight = false, bool renderTransperent = false)
        {
            
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
            Effect effect = Shader.GetAndApply(Graphic.SurfaceShaderInstance.ShaderSurfaceType.Instanced);

            effect.Parameters["isParticle"]?.SetValue(isParticle);

            ApplyPointLights(effect);

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

                        if (meshPart.IndexBuffer.IsDisposed) return;

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

        public override void DrawShadow(bool close = false, bool veryClose = false, bool viewmodel = false)
        {
            return; // no shadows for particles
        }

        public void LoadAssets()
        {
            GetModelFromPath("models/particle.obj");

            texture = AssetRegistry.LoadTextureFromFile(TexturePath);
            if (ModelPath is not null)
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
            if (finalizedParticles == null) return Position;
            if (finalizedParticles.Count == 0) return Position;

            List<Particle> particles = finalizedParticles.OrderBy(p => Vector3.Distance(p.position, Camera.position)).ToList();

            return particles[0].position;

        }

        public override bool IntersectsBoundingSphere(BoundingSphere sphere)
        {
            return new BoundingSphere(Position, BoundingRadius).Intersects(sphere);
        }

        public class Particle
        {
            public Vector3 position = new Vector3();
            public Vector3 position2 = new Vector3(); //used for trails
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

            public bool destroyed = false;

            public bool OrientToVelocity = false;

            public void SetRotationFromVelocity()
            {

                Vector2 p1 = UiElement.WorldToScreenSpace(position);
                Vector2 p2 = UiElement.WorldToScreenSpace(position + velocity);

                Rotation = MathHelper.FindLookAtRotation(p1, p2);

            }

            public Particle()
            {
            }
        }

    }
}
