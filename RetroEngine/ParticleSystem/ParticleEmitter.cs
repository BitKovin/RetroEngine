using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.PhysicsSystem;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static RetroEngine.Particles.ParticleEmitter;

namespace RetroEngine.Particles
{

    public class ParticleEmitter : StaticMesh
    {

        public List<Particle> Particles = new List<Particle>();

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

        protected static List<VertexBuffer> freeIstanceBuffers = new List<VertexBuffer>();

        public Matrix RelativeMatrix = Matrix.Identity;

        public bool DisableSorting = false;

        protected VertexBuffer ReuseOrCreateInstanceBuffer(GraphicsDevice graphicsDevice, int requiredVertexCount)
        {
            freeIstanceBuffers.Remove(null);

            // Try to reuse a buffer if available
            VertexBuffer buffer = freeIstanceBuffers.FirstOrDefault(vb => vb.VertexCount >= requiredVertexCount);
            if (buffer != null)
            {
                freeIstanceBuffers.Remove(buffer);
                return buffer;
            }

            Logger.Log("Creating new instance buffer for emitter with size " + requiredVertexCount);

            return new VertexBuffer(graphicsDevice, InstanceData.VertexDeclaration, requiredVertexCount, BufferUsage.WriteOnly);
        }

        public ParticleEmitter()
        {
            CastShadows = false;
            Transperent = true;

            SimpleTransperent = true;

            OverrideBlendState = BlendState.NonPremultiplied;

            TwoSided = true;

            DisableOcclusionCulling = true;

        }

        static bool createdBuffers = false;
        void CreateInitialBuffers()
        {

            if (createdBuffers) return;

            for (int j = 0; j < 5; j++)
                for (int i = 1; i <= 50; i+=1)
                {
                    freeIstanceBuffers.Add(new VertexBuffer(GameMain.Instance.GraphicsDevice, InstanceData.VertexDeclaration, i, BufferUsage.WriteOnly));
                }

            for (int j = 0; j < 2; j++)
                for (int i = 1; i <= 100; i += 1)
                {
                    freeIstanceBuffers.Add(new VertexBuffer(GameMain.Instance.GraphicsDevice, InstanceData.VertexDeclaration, i, BufferUsage.WriteOnly));
                }

            for (int j = 0; j < 1; j++)
                for (int i = 1; i <= 1500; i += 1)
                {
                    freeIstanceBuffers.Add(new VertexBuffer(GameMain.Instance.GraphicsDevice, InstanceData.VertexDeclaration, i, BufferUsage.WriteOnly));
                }

            createdBuffers = true;


        }

        protected List<Particle> finalizedParticles;

        int currentId = 0;

        public void Start()
        {
            SpawnParticles(InitialSpawnCount);
        }

        public void SpawnParticles(int num)
        {
            for (int i = 0; i < num; i++)
            {
                Particle particle = GetNewParticle();

                AddParticle(particle);
            }
        }

        public void AddParticle(Particle particle)
        {
            lock(Particles)
            {
                Particles.Add(particle);
            }
        }

        public virtual void Update()
        {

            if (destroyed) return;

            elapsedTime += Time.DeltaTime;

            if (elapsedTime > Duration)
                Emitting = false;

            float spawnInterval = 1f / SpawnRate;
            lock (Particles)
            {
                if (SpawnRate > 0 && Emitting)
                    while (elapsedTime >= spawnInterval)
                    {
                        Particle particle = GetNewParticle();
                        Particles.Add(particle);
                        elapsedTime -= spawnInterval;
                    }

                

                List<Particle> removeList = new List<Particle>();

                int n = Particles.Count;
                for (int i = 0; i < n; i++)
                {

                    Particle particle = Particles[i];

                    particle.lifeTime += Time.DeltaTime;

                    if (particle.lifeTime >= particle.deathTime)
                    {
                        removeList.Add(particle);
                        Particles[i] = particle;
                    }
                }
                foreach(Particle particle in removeList)
                    Particles.Remove(particle);

                for (int i = 0; i < Particles.Count; i++)
                {
                    lock(Particles)
                        lock(Particles[i])
                            Particles[i] = UpdateParticle(Particles[i]);
                }

                finalizedParticles = Particles.ToList();

                if (Emitting == false && Particles.Count == 0)
                {
                    destroyed = true;
                }

            }

            UpdateInstancedData(finalizedParticles);

        }

        public override void UpdateCulling()
        {
            inFrustrum = true;

            frameStaticMeshData.IsRendered = true;
            isRendered = true;

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
            return new Particle { position = Position, id = currentId, globalRotation = Rotation, HasCollision = false };
        }

        public override void RenderPreparation()
        {

            inFrustrum = true;

            CreateInitialBuffers();

            base.RenderPreparation();

            if (finalizedParticles != null)
                if (finalizedParticles.Count > 0)
                {

                    

                    frameStaticMeshData.model = (ModelPath == null) ? particleModel : GetModelFromPath(ModelPath);

                    texture = AssetRegistry.LoadTextureFromFile(TexturePath);

                }
            if (instanceDataPending != null)
            {
                instanceData = instanceDataPending.ToArray();
            }
            else
            {
                instanceData = null;
            }





            if (instanceData == null) return;

            if(instanceBuffer == null || instanceBuffer.VertexCount != instanceData.Length)
            {
                FreeBuffers();

                instanceBuffer = ReuseOrCreateInstanceBuffer(GameMain.Instance.GraphicsDevice, instanceData.Length);

            }

            if (instanceData.Length > 0)
            {
                instanceBuffer.SetData(instanceData);
            }
            else
            {

            }
        }

        private void FreeBuffers()
        {
            // Return buffers to the pool for reuse
            if (instanceBuffer != null)
            {
                freeIstanceBuffers.Add(instanceBuffer);
            }

        }

        Matrix GetWorldForParticle(Particle particle, Vector3 CameraForward, Vector3 CameraUp)
        {

            if (particle.useGlobalRotation == false)
            {
                // Cache the camera position if it's reused
                Vector3 camPos = Camera.finalizedPosition;

                // Compute the vector from the camera to the particle once
                Vector3 diff = particle.position - camPos;
                Vector3 viewDir = Vector3.Normalize(diff);

                // If the particle has a non-zero rotation, compute the rotated up vector once.
                Vector3 rotatedUp = CameraUp.RotateVector(CameraForward, particle.Rotation);

                // Create the billboard matrix using the cached values.
                Matrix billboardMatrix = Matrix.CreateBillboard(
                    particle.position,
                    camPos,
                    rotatedUp,
                    viewDir);

                // Apply scaling by multiplying with the scale matrix.
                Matrix worldMatrix = Matrix.CreateScale(particle.Scale) * billboardMatrix;

                return worldMatrix;
            }
            else if(particle.OrientRotationToVelocity == false)
            {

                float yaw =  MathHelper.ToRadians(particle.globalRotation.Y);
                float pitch = MathHelper.ToRadians(particle.globalRotation.X);
                float roll = MathHelper.ToRadians(particle.globalRotation.Z);

                // Create a single rotation matrix.
                Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

                // Combine scale, rotation and translation.
                Matrix worldMatrix = Matrix.CreateScale(particle.Scale) *
                                       rotation *
                                       Matrix.CreateTranslation(particle.position);

                return worldMatrix;
            }
            else
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


        }
        public override void DrawUnified()
        {
            if (destroyed) return;
            if (particleModel == null || particleModel.Meshes[0].MeshParts[0].IndexBuffer.IsDisposed)
            {
                particleModel = GetModelFromPath("models/particle.obj");
            }

            if (Render.DrawOnlyOpaque) return;


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

        InstanceData[] instanceDataPending;

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

            Vector3 cameraForward = Camera.finalizedRotation.GetForwardVector();
            Vector3 cameraUp = Camera.Up;

            particles = particles.Where(p=> Vector3.Distance(p.position, Camera.finalizedPosition) < p.MaxDrawDistance).OrderByDescending(p => DisableSorting ? 0 : Vector3.Dot(p.position - Camera.finalizedPosition, cameraForward)).ToList();

            var instanceData_new = new InstanceData[particles.Count];

            if(particles.Count == 0)
            {
                instanceDataPending = instanceData_new;
                return;
            }



            bool isParticle = ModelPath == null;


            int i = -1;
            Parallel.For(0, particles.Count,new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount}, i =>
            {
                var particle = particles[i];

                if (particle.transparency <= 0 || particle.lifeTime >= particle.deathTime)
                    return;


                Matrix world = GetWorldForParticle(particle, cameraForward, cameraUp);



                InstanceData data = new InstanceData
                {
                    Row1 = new Vector4(world.M11, world.M12, world.M13, world.M14),
                    Row2 = new Vector4(world.M21, world.M22, world.M23, world.M24),
                    Row3 = new Vector4(world.M31, world.M32, world.M33, world.M34),
                    Row4 = new Vector4(world.M41, world.M42, world.M43, world.M44),
                    Color = particle.color
                };

                data.Color.W *= particle.transparency;

                instanceData_new[i] = data;
            });

            instanceDataPending = instanceData_new.ToArray();

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

            if(destroyed) return;

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


                            if (destroyed) return;
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
                texture = AssetRegistry.LoadTextureFromFile(TexturePath);

                frameStaticMeshData.model = (ModelPath == null) ? particleModel : GetModelFromPath(ModelPath);
                frameStaticMeshData.World = GetWorldForParticle(particle, Camera.Forward, Camera.Up);
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
                texture = AssetRegistry.LoadTextureFromFile(TexturePath);

                frameStaticMeshData.model = (ModelPath == null) ? particleModel : GetModelFromPath(ModelPath);
                frameStaticMeshData.World = GetWorldForParticle(particle, Camera.Forward, Camera.Up);
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

        public virtual ParticleEmitterSaveData GetSaveData()
        {
            return new ParticleEmitterSaveData { particles = Particles.ToArray() };
        }

        public virtual void LoadData(ParticleEmitterSaveData data)
        {
            Particles = data.particles.ToList();
        }

        public struct ParticleEmitterSaveData()
        {
            [JsonInclude]
            public Particle[] particles;
        }

        public class Particle
        {
            [JsonInclude]
            public Vector3 position = new Vector3();
            [JsonInclude]
            public Vector3 position2 = new Vector3(); //used for trails
            [JsonInclude]
            public Vector3 velocity = new Vector3();

            [JsonInclude]
            public Vector3 globalRotation = new Vector3();

            [JsonInclude]
            public bool useGlobalRotation = false;


            [JsonInclude]
            public int seed = 0;

            [JsonInclude]
            public int id = 0;

            [JsonInclude]
            public float lifeTime = 0;
            [JsonInclude]
            public float deathTime = 2;

            [JsonInclude]
            public float transparency = 1;

            [JsonInclude]
            public Vector4 color = Vector4.One;

            [JsonInclude]
            public float Scale = 0;
            [JsonInclude]
            public float Rotation = 0;

            [JsonInclude]
            public bool OrientRotationToVelocity = false;

            [JsonInclude]
            public bool HasCollision = false;
            [JsonInclude]
            public float CollisionRadius = 0.02f;
            [JsonInclude]
            public float BouncePower = 0.8f;

            [JsonInclude]
            public bool Collided = false;

            [JsonInclude]
            public bool destroyed = false;

            [JsonInclude]
            public float UserData1 = 0;
            [JsonInclude]
            public float UserData2 = 0;
            [JsonInclude]
            public float UserData3 = 0;
            [JsonInclude]
            public float MaxDrawDistance = 30;

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
