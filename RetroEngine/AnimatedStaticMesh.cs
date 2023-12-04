using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class AnimatedStaticMesh : StaticMesh
    {

        public List<Model> frames = new List<Model>();

        public float frameTime = 0.06666666666f;

        public float animationTime = 0;

        public bool loop = false;
        public bool playing = false;

        float finalAnimationTime = 0;

        Dictionary<string, VertexBuffer> vertexBuffers = new Dictionary<string, VertexBuffer>();

        Dictionary<VertexBuffer, VertexPositionNormalTexture[]> frameVertexData = new Dictionary<VertexBuffer, VertexPositionNormalTexture[]>();

        public void Update()
        {
            if (loop)
            {
                while (frames.Count * frameTime <= animationTime)
                {
                    animationTime -= frames.Count * frameTime;
                }
            }
            else if(animationTime> frames.Count * frameTime)
            {
                animationTime = frames.Count * frameTime;
            }

            int currentFrame = (int)Math.Floor((double)(animationTime / frameTime));

            if(currentFrame > frames.Count - 1)
                currentFrame -= frames.Count;

            model = frames[currentFrame];
        }

        public void AddTime(float time)
        {
            if(playing)
                animationTime += time;
        }

        public void AddFrame(string name)
        {
            frames.Add(GetModelFromPath(name,true));
        }

        public void Play(float time = 0)
        {
            playing = true;

            animationTime = time;
        }

        public override void PreloadTextures()
        {
            foreach(Model m in frames)
            {
                model = m;
                LoadCurrentTextures();
            }
            
        }

        public override void RenderPreparation()
        {
            base.RenderPreparation();
            frameStaticMeshData.model2 = frames[(int)Math.Min(Math.Ceiling((double)(animationTime / frameTime)),frames.Count-1)];
            finalAnimationTime = animationTime;
        }

        public override void DrawUnified()
        {
            if (frameStaticMeshData.IsRendered == false && frameStaticMeshData.Viewmodel == false) return;

            GraphicsDevice graphicsDevice = GameMain.inst._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.inst.render.UnifiedEffect;

            AddFrameVertexData();

            if (frameStaticMeshData.model is not null)
            {
                for (int j = 0; j < model.Meshes.Count; j++)
                {
                    for(int i = 0; i < model.Meshes[j].MeshParts.Count; i++)
                    {
                        ModelMeshPart meshPart1 = frameStaticMeshData.model.Meshes[j].MeshParts[i];
                        ModelMeshPart meshPart2 = frameStaticMeshData.model2.Meshes[j].MeshParts[i];

                        VertexBuffer result = CreateVertexBufferIfNeeded($"m_{j}_p_{i}", meshPart1.VertexBuffer, graphicsDevice);

                        LerpVertexBuffers(graphicsDevice,result,meshPart1.VertexBuffer, meshPart2.VertexBuffer, finalAnimationTime / frameTime - (float)Math.Truncate((double)(finalAnimationTime / frameTime)));

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(result);
                        graphicsDevice.Indices = meshPart1.IndexBuffer;


                        // Set effect parameters
                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);
                        effect.Parameters["View"].SetValue(frameStaticMeshData.View);
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.Viewmodel ? frameStaticMeshData.ProjectionViewmodel : frameStaticMeshData.Projection);

                        effect.Parameters["depthScale"].SetValue(frameStaticMeshData.Viewmodel ? 0.04f : 1);

                        effect.Parameters["DirectBrightness"].SetValue(Graphics.DirectLighting);
                        effect.Parameters["GlobalBrightness"].SetValue(Graphics.GlobalLighting);
                        effect.Parameters["LightDirection"].SetValue(Graphics.LightDirection.Normalized());

                        effect.Parameters["ShadowMapViewProjection"].SetValue(Graphics.LightViewProjection);
                        effect.Parameters["ShadowMapViewProjectionClose"].SetValue(Graphics.LightViewProjectionClose);
                        effect.Parameters["ShadowMap"].SetValue(GameMain.inst.render.shadowMap);

                        effect.Parameters["ShadowBias"].SetValue(Graphics.ShadowBias);
                        effect.Parameters["ShadowMapResolution"].SetValue((float)Graphics.shadowMapResolution);

                        //effect.Parameters["DepthMap"].SetValue(GameMain.inst.render.DepthOutput);

                        effect.Parameters["Transparency"].SetValue(frameStaticMeshData.Transparency);

                        effect.Parameters["isParticle"].SetValue(isParticle);

                        MeshPartData meshPartData = meshPart1.Tag as MeshPartData;

                        if (meshPartData is not null)
                        {
                            effect.Parameters["Texture"].SetValue(FindTexture(meshPartData.textureName));
                        }
                        else
                        {
                            effect.Parameters["Texture"].SetValue(texture);
                        }

                        if (meshPartData is not null)
                        {
                            effect.Parameters["EmissiveTexture"].SetValue(FindEmissiveTexture(meshPartData.textureName));
                        }
                        else
                        {
                            effect.Parameters["EmissiveTexture"].SetValue(GameMain.inst.render.black);
                        }
                        effect.Parameters["EmissionPower"].SetValue(EmissionPower);

                        Vector3[] LightPos = new Vector3[LightManager.MAX_POINT_LIGHTS];
                        Vector3[] LightColor = new Vector3[LightManager.MAX_POINT_LIGHTS];
                        float[] LightRadius = new float[LightManager.MAX_POINT_LIGHTS];

                        for (int l = 0; l < LightManager.MAX_POINT_LIGHTS; l++)
                        {
                            LightPos[l] = LightManager.FinalPointLights[l].Position;
                            LightColor[l] = LightManager.FinalPointLights[l].Color;
                            LightRadius[l] = LightManager.FinalPointLights[l].Radius;
                        }

                        effect.Parameters["LightPositions"].SetValue(LightPos);
                        effect.Parameters["LightColors"].SetValue(LightColor);
                        effect.Parameters["LightRadiuses"].SetValue(LightRadius);

                        // Draw the primitives using the custom effect
                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            graphicsDevice.DrawIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                meshPart1.VertexOffset,
                                meshPart1.StartIndex,
                                meshPart1.PrimitiveCount);
                        }
                    }
                }
            }
        }

        VertexBuffer CreateVertexBufferIfNeeded(string name, VertexBuffer def, GraphicsDevice graphicsDevice)
        {
            if (vertexBuffers.ContainsKey(name))
                return vertexBuffers[name];

            VertexBuffer newBuffer = new VertexBuffer(graphicsDevice, def.VertexDeclaration, def.VertexCount, BufferUsage.None);
            vertexBuffers.Add(name, newBuffer);
            return newBuffer;
        }

        bool addedFrames = false;
        void AddFrameVertexData()
        {
            if (addedFrames == true) return;
            for (int f = 0; f < frames.Count; f++)
                for (int j = 0; j < frames[f].Meshes.Count; j++)
                {
                    for (int i = 0; i < frames[f].Meshes[j].MeshParts.Count; i++)
                    {
                        int vertexCount = frames[f].Meshes[j].MeshParts[i].VertexBuffer.VertexCount;
                        VertexPositionNormalTexture[] data = new VertexPositionNormalTexture[vertexCount];
                        frames[f].Meshes[j].MeshParts[i].VertexBuffer.GetData(data);
                        frameVertexData.TryAdd(frames[f].Meshes[j].MeshParts[i].VertexBuffer, data);
                    }
                }

            addedFrames = true;
        }

        void LerpVertexBuffers(GraphicsDevice graphicsDevice, VertexBuffer resultBuffer, VertexBuffer buffer1, VertexBuffer buffer2, float amount)
        {
            if (buffer1.VertexDeclaration != buffer2.VertexDeclaration || resultBuffer.VertexDeclaration != buffer1.VertexDeclaration)
            {
                throw new InvalidOperationException("Vertex buffers must have the same vertex declaration.");
            }

            int vertexCount = buffer1.VertexCount;
            int vertexSizeInBytes = buffer1.VertexDeclaration.VertexStride;

            // Get the data from the vertex buffers
            VertexPositionNormalTexture[] data1 = frameVertexData[buffer1];
            VertexPositionNormalTexture[] data2 = frameVertexData[buffer2];
            VertexPositionNormalTexture[] resultData = new VertexPositionNormalTexture[vertexCount];

            // Interpolate positions and normals
            for (int i = 0; i < vertexCount; i++)
            {
                resultData[i].Position = Vector3.Lerp(data1[i].Position, data2[i].Position, amount);
                resultData[i].Normal = Vector3.Lerp(data1[i].Normal, data2[i].Normal, amount);
                resultData[i].TextureCoordinate = Vector2.Lerp(data1[i].TextureCoordinate, data2[i].TextureCoordinate, amount);
            }

            // Set the interpolated data to the result buffer
            resultBuffer.SetData(resultData);
        }

    }
}
