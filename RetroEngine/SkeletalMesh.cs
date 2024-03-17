using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RetroEngine.Skeletal;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using BulletSharp.SoftBody;
using static RetroEngine.Skeletal.RiggedModel;


namespace RetroEngine
{
    public class SkeletalMesh : StaticMesh
    {
        protected RiggedModel RiggedModel;

        protected RiggedModelLoader modelReader = new RiggedModelLoader(GameMain.content, null);

        BoundingSphere boundingSphere = new BoundingSphere();

        protected static Dictionary<string, RiggedModel> LoadedRigModels = new Dictionary<string, RiggedModel>();

        protected Dictionary<string, Matrix> additionalLocalOffsets = new Dictionary<string, Matrix>();
        protected Dictionary<string, Matrix> additionalMeshOffsets = new Dictionary<string, Matrix>();

        protected AnimationPose animationPose = new AnimationPose();

        public SkeletalMesh()
        {
            CastShadows = true;
        }

        public override void LoadFromFile(string filePath)
        {

            string path = AssetRegistry.FindPathForFile(filePath);

            if (LoadedRigModels.ContainsKey(path))
            {
                RiggedModel = LoadedRigModels[path].MakeCopy();
            }
            else
            {
                RiggedModel = modelReader.LoadAsset(path, 30);

                RiggedModel.CreateBuffers();

                LoadedRigModels.Add(path, RiggedModel);
            }

            RiggedModel = LoadedRigModels[path].MakeCopy();


            RiggedModel.Update(0);

            CalculateBoundingSphere();

            GetBoneMatrix("");

            additionalLocalOffsets = RiggedModel.additionalLocalOffsets;

            additionalMeshOffsets = RiggedModel.additionalMeshOffsets;

            RiggedModel.overrideAnimationFrameTime = -1;
        }

        protected void CalculateBoundingSphere()
        {
            List<Vector3> points = new List<Vector3>();

            if (RiggedModel != null)
            {
                foreach (var b in RiggedModel.flatListToBoneNodes)
                {
                    points.Add(b.CombinedTransformMg.Translation/100);
                }
            }

            boundingSphere = BoundingSphere.CreateFromPoints(points);

            boundingSphere.Radius *= 1.5f;

        }

        public void SetBoneLocalTransformModification(string name,Matrix tranform)
        {
            if (additionalLocalOffsets.ContainsKey(name))
            {
                additionalLocalOffsets[name] = tranform;
                return;
            }

            additionalLocalOffsets.Add(name, tranform);

        }

        public void SetBoneMeshTransformModification(string name, Matrix tranform)
        {
            if (additionalMeshOffsets.ContainsKey(name))
            {
                additionalMeshOffsets[name] = tranform;
                return;
            }

            additionalMeshOffsets.Add(name, tranform);

        }


        public AnimationPose GetPose()
        {

            if(RiggedModel ==null) return new AnimationPose();

            Dictionary<string, Matrix> boneNamesToTransforms = new Dictionary<string, Matrix>();

            RiggedModel.UpdatePose();

            foreach(var bone in RiggedModel.flatListToAllNodes)
            {
                boneNamesToTransforms.TryAdd(bone.name, bone.CombinedTransformMg);
            }

            animationPose.Pose = boneNamesToTransforms;

            return animationPose;
        }

        public AnimationPose GetPoseLocal()
        {

            if (RiggedModel == null) return new AnimationPose();

            Dictionary<string, Matrix> boneNamesToTransforms = new Dictionary<string, Matrix>();


            foreach (var bone in RiggedModel.flatListToAllNodes)
            {
                boneNamesToTransforms.TryAdd(bone.name, bone.LocalTransformMg);
            }

            animationPose.Pose = boneNamesToTransforms;

            return animationPose;
        }

        public void PastePose(AnimationPose animPose)
        {
            if (RiggedModel == null) return;

            var pose = animPose.Pose;

            if (pose == null) return;

            Dictionary<string, Matrix> p = new Dictionary<string, Matrix>(pose);

            foreach (string key in p.Keys)
            {
                if (namesToBones.ContainsKey(key) == false) continue;

                var node = namesToBones[key];

                if(node.isThisARealBone)
                {
                    if (p.ContainsKey(key) == false) continue;
                    node.LocalTransformMg = p[key];

                    RiggedModel.globalShaderMatrixs[node.boneShaderFinalTransformIndex] = node.OffsetMatrixMg * p[key];
                }


            }

        }

        public void PastePoseLocal(AnimationPose animPose)
        {
            if (RiggedModel == null) return;

            var pose = animPose.Pose;

            if (pose == null) return;

            foreach (string key in pose.Keys)
            {
                if (namesToBones.ContainsKey(key) == false) continue;

                var node = namesToBones[key];

                if (node.isThisARealBone)
                {
                    if (pose.ContainsKey(key) == false) continue;
                    node.LocalTransformMg = pose[key];
                }
            }

            RiggedModel.animationPose = animPose;

            RiggedModel.UpdatePose();
        }

        public RiggedModelNode GetBoneByName(string name)
        {
            foreach(var bone in RiggedModel.flatListToBoneNodes)
            {
                if(bone.name == name)
                    return bone;
            }

            return null;
        }

        public virtual void Update(float deltaTime)
        {
            if (RiggedModel is null) return;

            RiggedModel.UpdateVisual = isRendered;
            RiggedModel.Update(deltaTime);
        }

        public void SetInterpolationEnabled(bool enabled)
        {
            if (RiggedModel is null) return;

            RiggedModel.UseStaticGeneratedFrames = !enabled;
        }

        public void PlayAnimation(int id = 0, bool looped = true)
        {

            if (RiggedModel is null) return;

            RiggedModel.BeginAnimation(id);
            RiggedModel.loopAnimation = looped;
        }

        public Matrix GetBoneMatrix(int id)
        {
            if (RiggedModel == null) return Matrix.Identity;

            foreach (var bone in RiggedModel.flatListToAllNodes)
            {
                if (bone.boneShaderFinalTransformIndex == id)
                    return bone.CombinedTransformMg * GetWorldMatrix();
            }

            return Matrix.Identity;

        }

        Dictionary<string, RiggedModel.RiggedModelNode> namesToBones = new Dictionary<string, RiggedModel.RiggedModelNode> ();

        public Matrix GetBoneMatrix(string name)
        {
            if (RiggedModel == null) return Matrix.Identity;

            if(namesToBones.ContainsKey(name))
                return namesToBones[name].CombinedTransformMg* GetWorldMatrix();

            foreach (var bone in RiggedModel.flatListToAllNodes)
            {
                namesToBones.TryAdd(bone.name, bone);


                if (bone.name == name)
                {
                    return bone.CombinedTransformMg * GetWorldMatrix();
                }
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

        Matrix[] finalizedBones = new Matrix[128];

        public override void RenderPreparation()
        {
            base.RenderPreparation();

            if (RiggedModel is null) return;

            finalizedBones = new List<Matrix>(RiggedModel.globalShaderMatrixs).ToArray();

        }

        public override void DrawShadow(bool closeShadow = false, bool veryClose = false)
        {
            if (!CastShadows) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;

            Effect effect = GameMain.Instance.render.ShadowMapEffect;

            effect.Parameters["Bones"].SetValue(finalizedBones);

            if (RiggedModel != null)
            {

                if (closeShadow)
                    if (Graphics.DirectionalLightFrustrumClose.Contains(boundingSphere.Transform(GetWorldMatrix())) == ContainmentType.Disjoint) return;

                if (veryClose)
                    if (Graphics.DirectionalLightFrustrumVeryClose.Contains(boundingSphere.Transform(GetWorldMatrix())) == ContainmentType.Disjoint) return;

                foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                {
                    // Set the vertex buffer and index buffer for this mesh part
                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    graphicsDevice.Indices = meshPart.IndexBuffer;


                    if (closeShadow)
                        Graphics.LightViewProjectionClose = frameStaticMeshData.LightViewClose * frameStaticMeshData.LightProjectionClose;
                    else if (veryClose)
                        Graphics.LightViewProjectionVeryClose = frameStaticMeshData.LightViewVeryClose * frameStaticMeshData.LightProjectionVeryClose;
                    else
                        Graphics.LightViewProjection = frameStaticMeshData.LightView * frameStaticMeshData.LightProjection;

                    // Set effect parameters
                    effect.Parameters["World"].SetValue(frameStaticMeshData.World);
                    
                    if (closeShadow)
                    {
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjectionClose);
                        effect.Parameters["View"].SetValue(frameStaticMeshData.LightViewClose);
                    }
                    else if (veryClose)
                    {
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjectionVeryClose);
                        effect.Parameters["View"].SetValue(frameStaticMeshData.LightViewVeryClose);
                    }else
                    {
                        effect.Parameters["View"].SetValue(frameStaticMeshData.LightView);
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightProjection);
                    }

                    

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

        public override void DrawDepth()
        {


            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;

            Effect effect = GameMain.Instance.render.OcclusionEffect;

            effect.Parameters["Bones"].SetValue(finalizedBones);

            effect.Parameters["Viewmodel"].SetValue(Viewmodel);

            if (Viewmodel)
                effect.Parameters["Projection"].SetValue(Camera.finalizedProjectionViewmodel);

            if (RiggedModel != null)
            {
                if (GameMain.Instance.render.BoundingSphere.Radius == 0 || IntersectsBoubndingSphere(GameMain.Instance.render.BoundingSphere))
                    foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                    {
                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        effect.Parameters["World"].SetValue(frameStaticMeshData.World);


                        effect.Techniques[0].Passes[0].Apply();

                        graphicsDevice.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            meshPart.VertexOffset,
                            meshPart.StartIndex,
                            meshPart.PrimitiveCount);

                    }
            }
        }

        public override void DrawUnified()
        {

            if(frameStaticMeshData.IsRendered == false && occluded) { return; }

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = Shader;

            SetupBlending();

            effect.Parameters["Bones"].SetValue(finalizedBones);
            if (RiggedModel != null)
            {
                foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                {

                    // Set the vertex buffer and index buffer for this mesh part
                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                    ApplyShaderParams(effect, meshPartData);

                    Stats.RenderedMehses++;

                    // Draw the primitives using the custom effect
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        meshPart.Draw(graphicsDevice);

                    }
                }

            }
        }


        public override void PreloadTextures()
        {
            if (RiggedModel != null)
            {
                foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                {

                    MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                    if (meshPartData is not null && textureSearchPaths.Count > 0)
                    {
                        FindTexture(meshPartData.textureName);

                        FindTextureWithSufix(meshPartData.textureName, def: emisssiveTexture);
                        FindTextureWithSufix(meshPartData.textureName, "_n", normalTexture);
                        FindTextureWithSufix(meshPartData.textureName, "_orm", ormTexture);
                    }
                }
            }
        }

        public override bool IntersectsBoubndingSphere(BoundingSphere sphere)
        {
            bool intersects = false;

            if (RiggedModel is not null)
            {

                intersects = boundingSphere.Transform(base.GetWorldMatrix()).Intersects(sphere);
            }

            return intersects;
        }

        public override void UpdateCulling()
        {
            //isRendered = Camera.frustum.Contains(boundingSphere.Transform(base.GetWorldMatrix())) != ContainmentType.Disjoint;
            isRendered = false;
            isRenderedShadow = false;

            if (Visible == false) return;

            inFrustrum = false;


            if (Camera.frustum.Contains(boundingSphere.Transform(base.GetWorldMatrix())) != ContainmentType.Disjoint)
            {
                inFrustrum = true;
            }

            if (Graphics.DirectionalLightFrustrum.Contains(boundingSphere.Transform(base.GetWorldMatrix())) != ContainmentType.Disjoint)
            {
                isRenderedShadow = true;

            }

            isRendered = inFrustrum&& (occluded==false) || Viewmodel;
            frameStaticMeshData.IsRendered = isRendered;
        }

        public override void Destroyed()
        {

            if (RiggedModel == null) return;
            namesToBones = null;
            RiggedModel.Destroy();
            RiggedModel = null;

        }

    }



    public struct AnimationPose
    {
        public Dictionary<string, Matrix> Pose = new Dictionary<string, Matrix>();
        public Dictionary<string, BonePoseBlend> BoneOverrides = new Dictionary<string, BonePoseBlend>();

        public AnimationPose() { }

        public void LayeredBlend(RiggedModelNode node, AnimationPose pose, float meshSpaceRotaion = 1)
        {
            if (node == null) return;
            ApplyNodeChildrenOnPose(node, pose);

            if(meshSpaceRotaion>0)
            {
                BoneOverrides.TryAdd(node.name, new BonePoseBlend { progress = meshSpaceRotaion, transform = node.LocalTransformMg * node.parent.CombinedTransformMg});
            }

        }

        void ApplyNodeChildrenOnPose(RiggedModelNode node, AnimationPose pose)
        {
            foreach(RiggedModelNode n in node.children)
            {

                ApplyNodeChildrenOnPose(n, pose);

                if (pose.Pose.ContainsKey(n.name) == false) continue;

                if(Pose.ContainsKey(n.name) == false)
                    Pose.Add(n.name, Matrix.Identity);

                Pose[n.name] = pose.Pose[n.name];

            }
        }

    }

    

    public struct BonePoseBlend
    {
        public Matrix transform;
        public float progress;
    }

}

