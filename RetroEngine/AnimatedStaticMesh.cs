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

        Dictionary<int, VertexData[]> frameVertexData = new Dictionary<int, VertexData[]>();

        public bool isLoaded = false;

        public void Update()
        {
            if(isLoaded==false) return;

            if (loop)
            {
                while (frames.Count * frameTime < animationTime)
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

            if(frames.Count>0)
            model = frames[currentFrame];
        }

        public void AddTime(float time)
        {
            if(playing)
                animationTime += time;
        }

        public void AddFrame(string name)
        {
            if (_disposed) return;
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
            if (_disposed) return;
            if (frameStaticMeshData.IsRendered == false && frameStaticMeshData.Viewmodel == false) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = Shader;

            SetupBlending();

            AddFrameVertexData();
            
                if (frameStaticMeshData.model is not null)
                {
                    if (frameStaticMeshData.model.Meshes == null) return;
                    for (int j = 0; j < frameStaticMeshData.model.Meshes.Count; j++)
                    {
                        if (frameStaticMeshData.model.Meshes[j].MeshParts == null) continue;

                        for (int i = 0; i < frameStaticMeshData.model.Meshes[j].MeshParts.Count; i++)
                        {
                            ModelMeshPart meshPart1 = frameStaticMeshData.model.Meshes[j].MeshParts[i];
                            ModelMeshPart meshPart2 = frameStaticMeshData.model2.Meshes[j].MeshParts[i];

                            VertexBuffer result = CreateVertexBufferIfNeeded($"m_{j}_p_{i}", meshPart1.VertexBuffer, graphicsDevice);

                            LerpVertexBuffers(graphicsDevice, result, meshPart1.VertexBuffer, meshPart2.VertexBuffer, finalAnimationTime / frameTime - (float)Math.Truncate((double)(finalAnimationTime / frameTime)));

                            // Set the vertex buffer and index buffer for this mesh part
                            graphicsDevice.SetVertexBuffer(result);
                            graphicsDevice.Indices = meshPart1.IndexBuffer;


                            MeshPartData meshPartData = meshPart1.Tag as MeshPartData;

                            ApplyShaderParams(effect, meshPartData);

                            if (_disposed) return;

                            Stats.RenderedMehses++;

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

        public override void DrawPathes()
        {
            AddFrameVertexData();

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;

            Effect effect = GameMain.Instance.render.BuffersEffect;

            if (frameStaticMeshData.model is not null)
            {
                for (int j = 0; j < model.Meshes.Count; j++)
                {
                    for (int i = 0; i < model.Meshes[j].MeshParts.Count; i++)
                    {
                        ModelMeshPart meshPart1 = frameStaticMeshData.model.Meshes[j].MeshParts[i];
                        ModelMeshPart meshPart2 = frameStaticMeshData.model2.Meshes[j].MeshParts[i];

                        VertexBuffer result = CreateVertexBufferIfNeeded($"m_{j}_p_{i}", meshPart1.VertexBuffer, graphicsDevice);

                        LerpVertexBuffers(graphicsDevice, result, meshPart1.VertexBuffer, meshPart2.VertexBuffer, finalAnimationTime / frameTime - (float)Math.Truncate((double)(finalAnimationTime / frameTime)));

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(result);
                        graphicsDevice.Indices = meshPart1.IndexBuffer;

                        MeshPartData meshPartData = (MeshPartData)meshPart1.Tag;

                        effect.Parameters["ColorTexture"].SetValue(FindTexture(meshPartData.textureName));
                        effect.Parameters["EmissiveTexture"].SetValue(FindTextureWithSufix(meshPartData.textureName));


                        effect.Parameters["DepthScale"].SetValue(frameStaticMeshData.Viewmodel ? 0.02f : 1);

                        Matrix projection;

                        if (frameStaticMeshData.Viewmodel)
                            projection = frameStaticMeshData.ProjectionViewmodel;
                        else
                            projection = frameStaticMeshData.Projection;

                        effect.Parameters["WorldViewProjection"].SetValue(frameStaticMeshData.World * frameStaticMeshData.View * projection);
                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);

                        
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
        public void AddFrameVertexData()
        {
            if (addedFrames == true) return;
            for (int f = 0; f < frames.Count; f++)
                for (int j = 0; j < frames[f].Meshes.Count; j++)
                {
                    for (int i = 0; i < frames[f].Meshes[j].MeshParts.Count; i++)
                    {

                        int vertexCount = frames[f].Meshes[j].MeshParts[i].VertexBuffer.VertexCount;
                        //VertexPositionNormalTexture[] data = new VertexPositionNormalTexture[vertexCount];

                        //VertexData[] data = ((MeshPartData)frames[f].Meshes[j].MeshParts[i].Tag).Vertices;
                        //frameVertexData.TryAdd(frames[f].Meshes[j].MeshParts[i].VertexBuffer.GetHashCode(), data);
                    }
                }

            addedFrames = true;
        }

        void LerpVertexBuffers(GraphicsDevice graphicsDevice, VertexBuffer resultBuffer, VertexBuffer buffer1, VertexBuffer buffer2, float amount)
        {
            return;
            if (buffer1.VertexDeclaration != buffer2.VertexDeclaration || resultBuffer.VertexDeclaration != buffer1.VertexDeclaration)
            {
                throw new InvalidOperationException("Vertex buffers must have the same vertex declaration.");
            }

            

            // Get the data from the vertex buffers
            VertexData[] data1 = frameVertexData[buffer1.GetHashCode()];
            resultBuffer.SetData(data1);
            return;
            VertexData[] data2 = frameVertexData[buffer2.GetHashCode()];

            if (data1 is null) return;
            int vertexCount = data1.Length;

            VertexData[] resultData = new VertexData[vertexCount];

            // Interpolate positions and normals
            for (int i = 0; i < vertexCount; i++)
            {
                resultData[i].Position = Vector3.Lerp(data1[i].Position, data2[i].Position, amount);
                resultData[i].Normal = Vector3.Lerp(data1[i].Normal, data2[i].Normal, amount);
                resultData[i].Tangent = Vector3.Lerp(data1[i].Tangent, data2[i].Tangent, amount);
                resultData[i].TextureCoordinate = data1[i].TextureCoordinate;
            }

            if (_disposed) return;

            // Set the interpolated data to the result buffer
            resultBuffer.SetData(resultData);
        }

        protected override void Unload()
        {
            _disposed = true;

            frames.Clear();
            
            foreach(VertexBuffer buffer in vertexBuffers.Values)
            {
                buffer.Dispose();
            }

            Console.WriteLine("unloaded anim mesh");

            vertexBuffers.Clear();
            frameVertexData.Clear();

        }
    }
}
