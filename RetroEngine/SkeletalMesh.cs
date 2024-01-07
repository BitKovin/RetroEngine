using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RetroEngine.Skeletal;
using static RetroEngine.Skeletal.RiggedModel;
using System;
using System.Xml.Linq;

namespace RetroEngine
{
    public class SkeletalMesh : StaticMesh
    {
        RiggedModel RiggedModel;

        static RiggedModelLoader modelReader = new RiggedModelLoader(GameMain.content, null);

        public override void LoadFromFile(string filePath)
        {
            RiggedModel = modelReader.LoadAsset(AssetRegistry.FindPathForFile(filePath),30);

            RiggedModel.CreateBuffers();

            RiggedModel.overrideAnimationFrameTime = -1;
        }

        public void Update(float deltaTime)
        {
            RiggedModel?.Update(deltaTime);
        }

        public void PlayAnimation(int id = 0)
        {
            RiggedModel.BeginAnimation(id);
        }

        public Matrix GetBoneMatrix(int id)
        {
            if (RiggedModel == null) return Matrix.Identity;

            foreach (var bone in RiggedModel.flatListToBoneNodes)
            {
                if (bone.boneShaderFinalTransformIndex == id)
                    return bone.CombinedTransformMg * GetWorldMatrix();
            }

            return Matrix.Identity;

        }

        public int GetBoneId(string name)
        {
            if (RiggedModel == null) return -1;

            foreach(var bone in RiggedModel.flatListToBoneNodes)
            {
                if (bone.name.ToLower() == name.ToLower())
                    return bone.boneShaderFinalTransformIndex;
            }

            return -1;
        }


        protected override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(0.01f) * base.GetWorldMatrix();
        }

        public override void DrawUnified()
        {

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.UnifiedEffect;


            effect.Parameters["Bones"].SetValue(RiggedModel.globalShaderMatrixs);
            if (RiggedModel != null)
            {
                foreach (RiggedModelMesh meshPart in RiggedModel.meshes)
                {

                    // Set the vertex buffer and index buffer for this mesh part
                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    effect.Parameters["viewDir"]?.SetValue(Camera.rotation.GetForwardVector());
                    effect.Parameters["viewPos"]?.SetValue(Camera.position);

                    // Set effect parameters
                    effect.Parameters["World"]?.SetValue(frameStaticMeshData.World);
                    effect.Parameters["View"]?.SetValue(frameStaticMeshData.View);
                    effect.Parameters["Projection"]?.SetValue(frameStaticMeshData.Viewmodel ? frameStaticMeshData.ProjectionViewmodel : frameStaticMeshData.Projection);

                    effect.Parameters["depthScale"]?.SetValue(frameStaticMeshData.Viewmodel ? 0.04f : 1);

                    effect.Parameters["DirectBrightness"]?.SetValue(Graphics.DirectLighting);
                    effect.Parameters["GlobalBrightness"]?.SetValue(Graphics.GlobalLighting);
                    effect.Parameters["LightDirection"]?.SetValue(Graphics.LightDirection.Normalized());

                    effect.Parameters["ShadowMapViewProjection"]?.SetValue(Graphics.LightViewProjection);
                    effect.Parameters["ShadowMapViewProjectionClose"]?.SetValue(Graphics.LightViewProjectionClose);

                    effect.Parameters["ShadowBias"]?.SetValue(Graphics.ShadowBias);
                    effect.Parameters["ShadowMapResolution"]?.SetValue((float)Graphics.shadowMapResolution);

                    //effect.Parameters["DepthMap"].SetValue(GameMain.inst.render.DepthOutput);

                    effect.Parameters["Transparency"]?.SetValue(frameStaticMeshData.Transparency);

                    effect.Parameters["isParticle"]?.SetValue(isParticle);

                    MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                    if (meshPartData is not null && textureSearchPaths.Count > 0)
                    {
                        effect.Parameters["Texture"]?.SetValue(FindTexture(meshPartData.textureName));
                        effect.Parameters["EmissiveTexture"]?.SetValue(FindTextureWithSufix(meshPartData.textureName, def: emisssiveTexture));
                        effect.Parameters["NormalTexture"]?.SetValue(FindTextureWithSufix(meshPartData.textureName, "_n", normalTexture));
                        effect.Parameters["ORMTexture"]?.SetValue(FindTextureWithSufix(meshPartData.textureName, "_orm", ormTexture));
                    }
                    else
                    {
                        effect.Parameters["Texture"]?.SetValue(texture);
                        effect.Parameters["EmissiveTexture"]?.SetValue(GameMain.Instance.render.black);
                        effect.Parameters["NormalTexture"]?.SetValue(normalTexture);
                        effect.Parameters["ORMTexture"]?.SetValue(ormTexture);
                    }
                    effect.Parameters["EmissionPower"].SetValue(EmissionPower);

                    Vector3[] LightPos = new Vector3[LightManager.MAX_POINT_LIGHTS];
                    Vector3[] LightColor = new Vector3[LightManager.MAX_POINT_LIGHTS];
                    float[] LightRadius = new float[LightManager.MAX_POINT_LIGHTS];

                    for (int i = 0; i < LightManager.MAX_POINT_LIGHTS; i++)
                    {
                        LightPos[i] = LightManager.FinalPointLights[i].Position;
                        LightColor[i] = LightManager.FinalPointLights[i].Color;
                        LightRadius[i] = LightManager.FinalPointLights[i].Radius;
                    }

                    effect.Parameters["LightPositions"]?.SetValue(LightPos);
                    effect.Parameters["LightColors"]?.SetValue(LightColor);
                    effect.Parameters["LightRadiuses"]?.SetValue(LightRadius);

                    // Draw the primitives using the custom effect
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        meshPart.Draw(graphicsDevice);

                    }
                }

            }
        }
    }
}

