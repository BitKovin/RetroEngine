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

        int primitiveCount = 0;

        public RibbonEmitter() : base()
        {
            Shader = new Graphic.SurfaceShaderInstance("TrailUnlit");
            isParticle = true;

            CastShadows = false;
            Transperent = true;
            Transparency = 1;
            SimpleTransperent = true;

            OverrideBlendState = BlendState.NonPremultiplied;
        }

        // Initialize vertex and index arrays
        VertexData[] verticesFinal;
        short[] indicesFinal;

        // Class-level cache variables
        private VertexData[] _cachedVertices;
        private short[] _cachedIndices;
        private int _lastParticleCount = -1;

        public void GenerateBuffers(List<Particle> particles)
        {
            primitiveCount = 0;

            lock (this)
            {
                if (particles == null || particles.Count < 2 || destroyed)
                {
                    verticesFinal = null;
                    indicesFinal = null;
                    return;
                }

                particles = new List<Particle>(particles);

                if (Emitting)
                {

                    Particle newParticle = GetNewParticle();

                    newParticle.position = Position + particles.Last().globalRotation.GetForwardVector() * 0.001f;

                    particles.Add(newParticle);

                    newParticle = GetNewParticle();

                    newParticle.position = Position + particles.Last().globalRotation.GetForwardVector() * 0.001f;

                    particles.Add(newParticle);
                }

                int currentParticleCount = particles.Count;
                int requiredVertexCount = currentParticleCount * 2;
                int requiredIndexCount = (currentParticleCount - 1) * 6;

                // Reuse or recreate buffers only when particle count changes
                if (_lastParticleCount != currentParticleCount)
                {
                    // Reallocate vertex buffer if size changed
                    if (_cachedVertices == null || _cachedVertices.Length != requiredVertexCount)
                    {
                        _cachedVertices = new VertexData[requiredVertexCount];
                    }

                    // Reallocate and regenerate index buffer if size changed
                    if (_cachedIndices == null || _cachedIndices.Length != requiredIndexCount)
                    {
                        _cachedIndices = new short[requiredIndexCount];
                        GenerateIndices(_cachedIndices, currentParticleCount);
                    }

                    _lastParticleCount = currentParticleCount;
                }

                Vector3 cameraPos = Camera.position; // Cache camera position once

                // Update vertex data
                for (int i = 0; i < currentParticleCount; i++)
                {
                    Particle particle = particles[i];
                    Vector3 position = particle.position;

                    // Calculate direction vector
                    Vector3 dir = i < currentParticleCount - 1
                        ? MathHelper.FastNormalize(position - particles[i + 1].position)
                        : MathHelper.FastNormalize(particles[i - 1].position - position);

                    // Calculate orientation vectors
                    Vector3 cameraForward = MathHelper.FastNormalize(position - cameraPos);
                    Vector3 perp = MathHelper.FastNormalize(Vector3.Cross(dir, cameraForward));

                    float halfScale = particle.Scale / 2;
                    int vertexIndex = i * 2;

                    // Calculate and store vertices
                    _cachedVertices[vertexIndex] = new VertexData
                    {
                        Position = position + perp * halfScale,
                        TextureCoordinate = new Vector2((float)i / (currentParticleCount - 1), 0f),
                        Color = particle.color
                    };

                    _cachedVertices[vertexIndex + 1] = new VertexData
                    {
                        Position = position - perp * halfScale,
                        TextureCoordinate = new Vector2((float)i / (currentParticleCount - 1), 1f),
                        Color = particle.color
                    };
                }

                if (destroyed) return;

                // Assign final buffers
                verticesFinal = _cachedVertices;
                indicesFinal = _cachedIndices;
                primitiveCount = requiredIndexCount / 3;
            }
        }

        private void GenerateIndices(short[] indices, int particleCount)
        {
            for (int i = 1; i < particleCount; i++)
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

        public void SetLastParticlePos()
        {

            if (Particles.Count < 1) return;

            Particle particle;

            lock (Particles)
            {
                particle = Particles.Last();
            }

            particle.position = Position;
            
        }


        public override void Update()
        {

            base.Update();
        }


        public override void DrawUnified()
        {
            if (destroyed) return;

            if (Render.DrawOnlyOpaque) return;

            //if (Camera.frustum.Contains(new BoundingSphere(Position, BoundingRadius)) != ContainmentType.Disjoint)
            DrawRibbon();
        }

        public override void Destroyed()
        {
            base.Destroyed();

            destroyed = true;


        }

        void DrawRibbon()
        {


            GraphicsDevice _graphicsDevice = GameMain.Instance.GraphicsDevice;



            GenerateBuffers(finalizedParticles);


            Effect effect = Shader.GetAndApply(Graphic.SurfaceShaderInstance.ShaderSurfaceType.Transperent);

            SetupBlending();
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            ApplyPointLights(effect);
            ApplyShaderParams(effect, null);
            effect.Parameters["isParticle"]?.SetValue(true);
            effect.Parameters["Projection"]?.SetValue(Viewmodel ? Camera.projectionViewmodel : Camera.projection);



            Stats.RenderedMehses++;


            if (indicesFinal == null || verticesFinal == null)
            {
                effect.Parameters["isParticle"]?.SetValue(false);
                effect.Parameters["Projection"]?.SetValue(Camera.projection);
                return;
            }

            if (destroyed == false)
            {
                effect.CurrentTechnique.Passes[0].Apply();
                if (primitiveCount > 0)
                {
                    //_graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
                    _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verticesFinal, 0, verticesFinal.Length, indicesFinal, 0, primitiveCount);
                }
            }


            effect.Parameters["isParticle"]?.SetValue(false);
            effect.Parameters["Projection"]?.SetValue(Camera.projection);


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
