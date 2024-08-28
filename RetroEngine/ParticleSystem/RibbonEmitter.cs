using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Particles
{

    public class RibbonEmitter : ParticleEmitter
    {

        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        int primitiveCount = 0;

        public RibbonEmitter() : base()
        {
            Shader = new Graphic.SurfaceShaderInstance("UnifiedOutput");
            isParticle = true;

            CastShadows = false;
            Transperent = true;
            Transparency = 1;
            SimpleTransperent = true;

            OverrideBlendState = BlendState.NonPremultiplied;
        }


        public void GenerateBuffers(List<Particle> particles)
        {

            primitiveCount = 0;

            lock (this)
            {
                if (particles == null || particles.Count < 2 || destroyed)
                {
                    FreeBuffers();
                    return;
                }

                GraphicsDevice _graphicsDevice = GameMain.Instance.GraphicsDevice;

                // Calculate required vertex and index counts
                int requiredVertexCount = particles.Count * 2;
                int requiredIndexCount = (particles.Count - 1) * 6;

                // Check if buffers need resizing (allocate with some slack, e.g., 25%)
                int vertexCapacityThreshold = vertexBuffer?.VertexCount ?? 0;
                int indexCapacityThreshold = indexBuffer?.IndexCount ?? 0;

                if (vertexBuffer == null || requiredVertexCount > vertexCapacityThreshold)
                {
                    vertexCapacityThreshold = (int)(requiredVertexCount * 2f); // 25% extra space
                    vertexBuffer = ReuseOrCreateVertexBuffer(_graphicsDevice, vertexCapacityThreshold);
                }

                if (indexBuffer == null || requiredIndexCount > indexCapacityThreshold)
                {
                    indexCapacityThreshold = (int)(requiredIndexCount * 2f); // 25% extra space
                    indexBuffer = ReuseOrCreateIndexBuffer(_graphicsDevice, indexCapacityThreshold);
                }

                // Initialize vertex and index arrays
                VertexData[] vertices = new VertexData[requiredVertexCount];
                short[] indices = new short[requiredIndexCount];

                // Generate vertices and indices
                for (int i = 0; i < particles.Count; i++)
                {
                    Particle particle = particles[i];
                    Vector3 p1 = particle.position;

                    Vector3 dir;
                    if (i < particles.Count - 1)
                    {
                        dir = MathHelper.FastNormalize(particles[i].position - particles[i + 1].position);
                    }
                    else
                    {
                        dir = MathHelper.FastNormalize(particles[i - 1].position - particles[i].position);
                    }

                    Vector3 cameraForward = p1 - Camera.position;
                    cameraForward = cameraForward.Normalized();
                    Vector3 perp = Vector3.Cross(dir, cameraForward);
                    perp = perp.Normalized();

                    // Calculate vertices for the quad
                    Vector3 topLeft = p1 + perp * (particle.Scale / 2);
                    Vector3 topRight = p1 - perp * (particle.Scale / 2);

                    // Calculate texture coordinates
                    float texCoordX = (float)i / (particles.Count - 1);
                    float texCoordYTop = 0f;
                    float texCoordYBottom = 1f;

                    // Add vertices to the array
                    vertices[i * 2] = new VertexData { Position = topLeft, TextureCoordinate = new Vector2(texCoordX, texCoordYTop), Color = particle.color };
                    vertices[i * 2 + 1] = new VertexData { Position = topRight, TextureCoordinate = new Vector2(texCoordX, texCoordYBottom), Color = particle.color };

                    // Add indices to form the quad
                    if (i > 0)
                    {
                        int indexOffset = (i - 1) * 6;
                        int vertexOffset = i * 2;

                        indices[indexOffset] = (short)vertexOffset;
                        indices[indexOffset + 1] = (short)(vertexOffset - 1);
                        indices[indexOffset + 2] = (short)(vertexOffset - 2);

                        indices[indexOffset + 3] = (short)vertexOffset;
                        indices[indexOffset + 4] = (short)(vertexOffset + 1);
                        indices[indexOffset + 5] = (short)(vertexOffset - 1);
                    }
                }
                if (destroyed) return;
                // Set buffer data
                vertexBuffer.SetData(vertices, 0, requiredVertexCount);
                indexBuffer.SetData(indices, 0, requiredIndexCount);

                // Calculate the number of primitives to draw
                primitiveCount = requiredIndexCount / 3;
            }
        }

        private void FreeBuffers()
        {
            // Return buffers to the pool for reuse
            if (vertexBuffer != null)
            {
                freeVertexBuffers.Add(vertexBuffer);
                //vertexBuffer = null;
            }

            if (indexBuffer != null)
            {
                freeIndexBuffers.Add(indexBuffer);
                //indexBuffer = null;
            }
        }


        public override void Update()
        {

            var p = particles.LastOrDefault();

            if (p != null)
                p.position = Position;

            base.Update();
        }


        public override void DrawUnified()
        {
            if (destroyed) return;

            //if (Camera.frustum.Contains(new BoundingSphere(Position, BoundingRadius)) != ContainmentType.Disjoint)
            DrawRibbon();
        }

        public override void Destroyed()
        {
            base.Destroyed();

            destroyed = true;

            FreeBuffers();

        }

        void DrawRibbon()
        {
            GraphicsDevice _graphicsDevice = GameMain.Instance.GraphicsDevice;



            GenerateBuffers(finalizedParticles);

            if (vertexBuffer == null || indexBuffer == null || vertexBuffer.IsDisposed || indexBuffer.IsDisposed)
            { Console.WriteLine("empty vertex buffer"); return; }


            Effect effect = Shader.GetAndApply(Graphic.SurfaceShaderInstance.ShaderSurfaceType.Transperent);

            SetupBlending();
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            ApplyPointLights(effect);
            ApplyShaderParams(effect, null);
            effect.Parameters["isParticle"]?.SetValue(true);

            _graphicsDevice.SetVertexBuffer(vertexBuffer);
            _graphicsDevice.Indices = indexBuffer;

            if (vertexBuffer == null) return;

            Stats.RenderedMehses++;
            if (vertexBuffer == null) return;

            lock (vertexBuffer)
            {
                if (vertexBuffer == null || indexBuffer == null || vertexBuffer.IsDisposed || indexBuffer.IsDisposed)
                { Console.WriteLine("empty vertex buffer"); return; }

                if (destroyed == false)
                {
                    effect.CurrentTechnique.Passes[0].Apply();
                    if(primitiveCount>0)
                        _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
                }

            }

            effect.Parameters["isParticle"]?.SetValue(false);


        }

        public override void RenderPreparation()
        {

            WorldMatrix = GetWorldMatrix();

            frameStaticMeshData.Projection = Camera.projection;
            frameStaticMeshData.ProjectionViewmodel = Camera.projectionViewmodel;
            frameStaticMeshData.model = model;
            frameStaticMeshData.Transperent = Transperent;
            frameStaticMeshData.EmissionPower = EmissionPower;
            frameStaticMeshData.View = Camera.finalizedView;
            frameStaticMeshData.World = WorldMatrix;
            frameStaticMeshData.Viewmodel = Viewmodel;
            frameStaticMeshData.LightView = Graphics.LightView;
            frameStaticMeshData.LightViewClose = Graphics.LightCloseView;
            frameStaticMeshData.LightViewVeryClose = Graphics.LightVeryCloseView;
            frameStaticMeshData.LightProjection = Graphics.LightProjection;
            frameStaticMeshData.LightProjectionVeryClose = Graphics.LightVeryCloseProjection;
            frameStaticMeshData.Transparency = 1;
            frameStaticMeshData.LightProjectionClose = Graphics.LightCloseProjection;
            frameStaticMeshData.IsRendered = isRendered;
            frameStaticMeshData.IsRenderedShadow = isRenderedShadow;
            frameStaticMeshData.InFrustrum = inFrustrum;
        }

        public override Matrix GetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateScale(1) *
                            Matrix.CreateTranslation(Vector3.Zero);
            return worldMatrix;
        }

    }
}
