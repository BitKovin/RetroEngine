﻿using Microsoft.Xna.Framework;
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

        public RibbonEmitter():base()
        {
            Shader = AssetRegistry.GetShaderFromName("UnifiedOutput");
            isParticle = true;

            CastShadows = false;
            Transperent = true;
            Transparency = 1;
            SimpleTransperent = false;

            OverrideBlendState = BlendState.NonPremultiplied;
        }

        public void GenerateBuffers(List<Particle> particles)
        {
            vertexBuffer?.Dispose();
            indexBuffer?.Dispose();
            if (particles == null) return;
           
            if (particles.Count < 2)
                return;

            GraphicsDevice _graphicsDevice = GameMain.Instance.GraphicsDevice;

            // Initialize vertex and index lists
            List<VertexData> vertices = new List<VertexData>();
            List<short> indices = new List<short>();

            // Generate vertices for ribbon
            for (int i = 0; i < particles.Count; i++)
            {
                Particle particle = particles[i];
                Vector3 p1 = particle.position;

                Vector3 dir;

                if(i<particles.Count-1)
                {
                    dir = Vector3.Normalize(particles[i].position - particles[i+1].position);
                }else
                {
                    dir = Vector3.Normalize(particles[i-1].position - particles[i].position);
                }

                Vector3 cameraForward = p1 - Camera.position;
                cameraForward.Normalize();
                Vector3 perp = Vector3.Cross(dir, cameraForward);
                perp.Normalize();

                // Calculate vertices for the quad
                Vector3 topLeft = p1 + perp * (particle.Scale / 2);
                Vector3 topRight = p1 - perp * (particle.Scale / 2);

                // Calculate texture coordinates
                float texCoordX = (float)i / (particles.Count - 1);
                float texCoordYTop = 0f;
                float texCoordYBottom = 1f;

                // Add vertices to the list with texture coordinates
                vertices.Add(new VertexData { Position = topLeft, TextureCoordinate = new Vector2(texCoordX, texCoordYTop), Color = particle.color});
                vertices.Add(new VertexData { Position = topRight, TextureCoordinate = new Vector2(texCoordX, texCoordYBottom), Color = particle.color });


                // Add indices to form the quad
                if (i > 0)
                {
                    indices.Add((short)(i * 2));
                    indices.Add((short)(i * 2 - 1));
                    indices.Add((short)(i * 2 - 2));

                    indices.Add((short)(i * 2));
                    indices.Add((short)(i * 2 + 1));
                    indices.Add((short)(i * 2 - 1));
                }
            }
            

            // Create and set vertex buffer
            vertexBuffer = new VertexBuffer(_graphicsDevice, VertexData.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices.ToArray());

            // Create and set index buffer
            indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());

            // Set vertex and index buffers
            _graphicsDevice.SetVertexBuffer(vertexBuffer);
            _graphicsDevice.Indices = indexBuffer;
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

            GameMain.pendingDispose.Add(vertexBuffer);
            GameMain.pendingDispose.Add(indexBuffer);
        }

        void DrawRibbon()
        {
            
                GraphicsDevice _graphicsDevice = GameMain.Instance.GraphicsDevice;
                GenerateBuffers(finalizedParticles);

                if (vertexBuffer == null || indexBuffer == null || vertexBuffer.IsDisposed || indexBuffer.IsDisposed)
                { Console.WriteLine("empty vertex buffer"); return; }


                Effect effect = Shader;

                SetupBlending();
                ApplyShaderParams(effect, null);


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
                    _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount);
                }

            }

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

        protected override Matrix GetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateScale(1) *
                            Matrix.CreateTranslation(Vector3.Zero);
            return worldMatrix;
        }

    }
}