using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Graphic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Particles
{

    public class TrailEmitter : ParticleEmitter
    {
        // Initialize vertex and index arrays
        VertexData[] verticesFinal;
        short[] indicesFinal;

        int primitiveCount = 0;



        public TrailEmitter() : base()
        {
            Shader = new SurfaceShaderInstance(GameMain.Instance.DefaultShader);
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

            if (particles == null || particles.Count < 2 || destroyed)
            {
                return;
            }

            GraphicsDevice _graphicsDevice = GameMain.Instance.GraphicsDevice;


            // Calculate required vertex and index counts
            int requiredVertexCount = particles.Count * 2;
            int requiredIndexCount = (particles.Count - 1) * 6;

            // Initialize vertex and index lists
            List<VertexData> vertices = new List<VertexData>();
            List<short> indices = new List<short>();

            // Generate vertices for ribbon
            for (int i = 0; i < particles.Count; i++)
            {
                Particle particle = particles[i];

                Vector3 p = particle.position;

                p = Vector3.Transform(p, RelativeMatrix);

                Vector3 halfSize = particle.globalRotation.GetForwardVector() * particle.Scale / 2;

                Vector3 p1 = p + halfSize;

                Vector3 p2 = p - halfSize;


                // Calculate vertices for the quad
                Vector3 topLeft = p1;
                Vector3 topRight = p2;

                // Calculate texture coordinates
                float texCoordX = (float)i / (particles.Count - 1);
                float texCoordYTop = 0f;
                float texCoordYBottom = 1f;

                // Add vertices to the list with texture coordinates
                vertices.Add(new VertexData { Position = topLeft, TextureCoordinate = new Vector2(texCoordX, texCoordYTop), Color = particle.color });
                vertices.Add(new VertexData { Position = topRight, TextureCoordinate = new Vector2(texCoordX, texCoordYBottom), Color = particle.color });


                // Add indices to form the quad
                if (i > 1)
                {
                    indices.Add((short)(i * 2));
                    indices.Add((short)(i * 2 - 1));
                    indices.Add((short)(i * 2 - 2));

                    indices.Add((short)(i * 2));
                    indices.Add((short)(i * 2 + 1));
                    indices.Add((short)(i * 2 - 1));
                }
                else if (i == 0)
                {
                    indices.Add((short)((i + 1) * 2));
                    indices.Add((short)((i + 1) * 2 - 1));
                    indices.Add((short)((i + 1) * 2 - 2));

                    indices.Add((short)((i + 1) * 2));
                    indices.Add((short)((i + 1) * 2 + 1));
                    indices.Add((short)((i + 1) * 2 - 1));
                }
            }

            if (destroyed) return;
            // Set buffer data
            verticesFinal = vertices.ToArray();
            indicesFinal = indices.ToArray();

            // Calculate the number of primitives to draw
            primitiveCount = requiredIndexCount / 3;
        }

        public override void DrawUnified()
        {
            if (destroyed) return;

            //if (Camera.frustum.Contains(new BoundingSphere(Position, BoundingRadius)) != ContainmentType.Disjoint)

            if (Render.DrawOnlyOpaque) return;

            DrawRibbon();
        }

        public override void Destroyed()
        {
            base.Destroyed();

            destroyed = true;
        }

        public override void DrawDepth(bool pointLight = false, bool renderTransperent = false)
        {

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



            

        }

        public override bool IntersectsBoundingSphere(BoundingSphere sphere)
        {
            return true;
        }

        public override Particle GetNewParticle()
        {

            Particle particle = base.GetNewParticle();

            particle.Scale = ParticleSizeMultiplier;

            return particle;

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
