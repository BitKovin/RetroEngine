using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RetroEngine.Skeletal;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using BulletSharp.SoftBody;
using static RetroEngine.Skeletal.RiggedModel;
using BulletSharp;
using System.Text.Json.Serialization;
using System.IO;
using System.Text.Json;
using System.Linq;
using RetroEngine.PhysicsSystem;
using System.Threading.Tasks;
using RetroEngine.Graphic;
using SharpFont;
using System.Collections.Concurrent;


namespace RetroEngine
{

    public delegate void AnimationEventPlayed(AnimationEvent animationEvent);
    public class SkeletalMesh : StaticMesh
    {
        protected RiggedModel RiggedModel;

        protected RiggedModelLoader modelReader = new RiggedModelLoader(GameMain.content, null);

        public BoundingSphere boundingSphere = new BoundingSphere();

        public static ConcurrentDictionary<string, RiggedModel> LoadedRigModels = new ConcurrentDictionary<string, RiggedModel>();


        protected Dictionary<string, Matrix> additionalLocalOffsets = new Dictionary<string, Matrix>();
        protected Dictionary<string, Matrix> additionalMeshOffsets = new Dictionary<string, Matrix>();

        protected AnimationPose animationPose = new AnimationPose();

        public AnimationInfo CurrentAnimationInfo = new AnimationInfo();

        public SkeletalMesh ParrentBounds;

        public bool UpdatePose = true;

        public List<HitboxInfo> hitboxes = new List<HitboxInfo>();
        public List<AnimationInfo> animationInfos = new List<AnimationInfo>();

        public bool AlwaysUpdateVisual = false;

        float newAnimInterpolationProgress = 0;
        float newAnimInterpolationSpeed = 0;

        AnimationPose oldAnimPose;

        Vector3 OldRootMotion = new Vector3();
        Vector3 OldRootMotionRot = new Vector3();

        Vector3 RootMotionPositionOffset = new Vector3();
        Vector3 RootMotionRotationOffset = new Vector3();

        public event AnimationEventPlayed OnAnimationEvent;

        Vector3 boundingSphereOffset = new Vector3();
        public bool isRagdoll { get; protected set; }

        public string Name = "";

        public bool IgnoreAnimationEvents = false;

        public float MaxRenderDistance = 300;
        public SkeletalMesh() : base()
        {

            CastShadows = true;

            CastGeometricShadow = true;

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

                GenerateSmoothNormals();

                LoadedRigModels.TryAdd(path, RiggedModel);
            }

            RiggedModel = LoadedRigModels[path].MakeCopy();


            RiggedModel.Update(0);


            GetBoneMatrix("");

            additionalLocalOffsets = RiggedModel.additionalLocalOffsets;

            additionalMeshOffsets = RiggedModel.additionalMeshOffsets;

            LoadMeshMetaFromFile(path);

            CalculateBoundingSphere();

            Bounds = new MeshBounds { Position = boundingSphere.Center, InnerRadius = 0, OuterRadius = boundingSphere.Radius };

            RiggedModel.overrideAnimationFrameTime = -1;
        }

        public string GetCurrentAnimationName()
        {

            if (RiggedModel == null)
                return "";

            return RiggedModel.GetCurrentAnimationName(); 
        }
        
        public int GetNumOfAnimations()
        {
            if(RiggedModel == null) return 0;

            return RiggedModel.originalAnimations.Count;

        }

        public MathHelper.Transform PullRootMotion()
        {

            if (RiggedModel == null) return new MathHelper.Transform();

            Vector3 rootMotion = RiggedModel.TotalRootMotion - OldRootMotion + RiggedModel.RootMotionOffset;

            Vector3 rootMotionRot = RiggedModel.TotalRootMotionRot - OldRootMotionRot + RiggedModel.RootMotionOffsetRot;

            RiggedModel.RootMotionOffset = Vector3.Zero;
            RiggedModel.RootMotionOffsetRot = Vector3.Zero;



            OldRootMotion = RiggedModel.TotalRootMotion;
            OldRootMotionRot = RiggedModel.TotalRootMotionRot;

            RootMotionPositionOffset = -RiggedModel.TotalRootMotion;

            RootMotionRotationOffset = -RiggedModel.TotalRootMotionRot;

            MathHelper.Transform transform = new MathHelper.Transform();

            transform.Position = Vector3.Transform(rootMotion, (-RiggedModel.TotalRootMotionRot).GetRotationMatrix());
            transform.Rotation = rootMotionRot;

            return transform;

        }

        public int GetCurrentAnimationFrame()
        {
            if (RiggedModel == null) return 0;

            return RiggedModel.currentFrame;

        }

        public int GetCurrentAnimationIndex()
        {

            if (RiggedModel == null) return 0;

            return RiggedModel.currentAnimation;

        }

        public int GetCurrentAnimationFrameDuration()
        {
            if (RiggedModel == null) return 0;

            if (RiggedModel.currentAnimation >= 0 && RiggedModel.currentAnimation < RiggedModel.originalAnimations.Count)
                return RiggedModel.originalAnimations[RiggedModel.currentAnimation].TotalFrames;

            return 0;

        }

        public void SetCurrentAnimationFrame(float frame)
        {
            if(frame< 0) frame = 0;
            if (RiggedModel == null) return;

            RiggedModel.SetFrame(frame);
            RiggedModel.Update(0, true);
        }

        public bool IsPlayingAnimation()
        {

            if(RiggedModel == null) return false;

            return RiggedModel.animationRunning;
        }

        public void SetIsPlayingAnimation(bool playing)
        {

            if (RiggedModel == null) return;

            RiggedModel.animationRunning = playing;
        }

        long boundSphereUpdateTick = 0;

        public void CalculateBoundingSphereWithInterval(int tickInternval = 3)
        {
            boundSphereUpdateTick++;

            if (boundSphereUpdateTick % tickInternval != 0) return;

            CalculateBoundingSphere();

        }

        public void CalculateBoundingSphere()
        {
            List<Vector3> points = new List<Vector3>();

            Vector3 pos = new Vector3();

            int n = 0;

            if (RiggedModel != null)
            {
                lock (RiggedModel)
                {
                    foreach (var b in RiggedModel.flatListToBoneNodes)
                    {
                        if(b.children.Count == 0)
                            points.Add(b.CombinedTransformMg.Translation / 100);
                    }
                }
            }
            if (points.Count > 0)
            {
                boundingSphere = BoundingSphere.CreateFromPoints(points).Transform(base.GetWorldMatrix());
                boundingSphere.Radius *= 1.5f;
                boundingSphere.Radius += 0.3f;

                boundingSphereOffset = boundingSphere.Center - Position;

            }
        }

        public void SetBoneLocalTransformModification(string name, Matrix tranform)
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

        public void SetWorldPositionOverride(string name, Matrix transform)
        {

            if (RiggedModel == null) return;

            lock (RiggedModel.finalOverrides)
            {

                if (RiggedModel.finalOverrides.ContainsKey(name) == false)
                {
                    RiggedModel.finalOverrides.Add(name, transform);
                }
                else
                {
                    RiggedModel.finalOverrides[name] = transform;
                }

                RiggedModel.World = GetWorldMatrix();
            }
        }

        public void RemoveWorldPositionOverride(string name)
        {
            RiggedModel.finalOverrides.Remove(name);
        }

        public AnimationPose GetPose()
        {

            if (RiggedModel == null) return new AnimationPose();

            Dictionary<string, Matrix> boneNamesToTransforms = new Dictionary<string, Matrix>();

            

            foreach (var bone in RiggedModel.flatListToAllNodes)
            {
                boneNamesToTransforms.TryAdd(bone.name, bone.CombinedTransformMg);
            }

            animationPose.Pose = boneNamesToTransforms;

            return animationPose;
        }

        public virtual AnimationPose GetPoseLocal()
        {

            if (RiggedModel == null) return new AnimationPose();

            Dictionary<string, Matrix> boneNamesToTransforms = new Dictionary<string, Matrix>();


            foreach (var bone in RiggedModel.flatListToAllNodes)
            {
                boneNamesToTransforms.TryAdd(bone.name, bone.LocalFinalTransformMg);
            }

            animationPose.Pose = boneNamesToTransforms;

            if(newAnimInterpolationProgress>0&& newAnimInterpolationProgress<1)
            {
                animationPose = Animation.LerpPose(oldAnimPose, animationPose, newAnimInterpolationProgress);   
            }

            return animationPose;
        }

        public virtual void PastePose(AnimationPose animPose)
        {
            if (RiggedModel == null) return;

            var pose = animPose.Pose;

            if (pose == null) return;

            Dictionary<string, Matrix> p = new Dictionary<string, Matrix>(pose);
            lock (namesToBones)
            {
                foreach (string key in p.Keys)
                {
                    if (namesToBones.ContainsKey(key) == false) continue;
                    var node = namesToBones[key];

                    if (node.isThisARealBone)
                    {
                        if (p.ContainsKey(key) == false) continue;
                        node.LocalTransformMg = p[key];

                        RiggedModel.globalShaderMatrixs[node.boneID] = node.OffsetMatrixMg * p[key];
                    }


                }
            }

        }

        public void PastePoseLocal(AnimationPose animPose, bool ignoreRoot = false, bool applyTransformModifiers = true)
        {
            if (RiggedModel == null) return;

            //RiggedModel.animationPose.BoneOverrides = new Dictionary<string, BonePoseBlend>();

            var pose = animPose.Pose;

            if (pose == null) return;

            foreach (string key in pose.Keys)
            {
                if (namesToBones.ContainsKey(key) == false) continue;

                if(ignoreRoot)
                {
                    if (key == "root")
                        continue;
                }

                var node = namesToBones[key];

                if (node.isThisARealBone)
                {
                    if (pose.ContainsKey(key) == false) continue;
                    node.LocalTransformMg = pose[key];
                }
            }

            RiggedModel.animationPose = animPose;

            if (applyTransformModifiers == false)
            {

                var savedLocal = RiggedModel.additionalLocalOffsets;
                var savedMesh = RiggedModel.additionalMeshOffsets;

                RiggedModel.animationPose.BoneOverrides.Clear();

                RiggedModel.additionalLocalOffsets = new Dictionary<string, Matrix>();
                RiggedModel.additionalMeshOffsets = new Dictionary<string, Matrix>();

                RiggedModel.UpdatePose();

                RiggedModel.additionalLocalOffsets = savedLocal;
                RiggedModel.additionalMeshOffsets = savedMesh;

            }
            else
            {
                RiggedModel.UpdatePose();
            }

            

            if(applyTransformModifiers == false)
            {
                
            }

        }

        public void UpdateAnimationPose()
        {
            RiggedModel.UpdatePose();
        }

        public RiggedModelNode GetBoneByName(string name)
        {
            if (RiggedModel != null)
                foreach (var bone in RiggedModel.flatListToBoneNodes)
                {
                    if (bone.name == name)
                        return bone;
                }

            return null;
        }

        private int OldFrame = -1;
        protected void UpdateAnimationEvents(bool negativeDelta)
        {


            int newFrame = GetCurrentAnimationFrame();

            if (CurrentAnimationInfo.AnimationEvents.Length == 0)
            {
                OldFrame = newFrame;
                return;
            }

            if (newFrame == OldFrame) return;

            int animFrameDuration = GetCurrentAnimationFrameDuration();

            if (newFrame >= animFrameDuration)
                newFrame = animFrameDuration - 1;

            if (newFrame == OldFrame) return;

            int currentFrame = OldFrame;

            List<AnimationEvent> events = new List<AnimationEvent>();

            while (currentFrame != newFrame)
            {

                if (negativeDelta)
                {
                    currentFrame--;
                }
                else
                {
                    currentFrame++;
                }

                if (currentFrame >= animFrameDuration)
                    currentFrame = 0;

                if (currentFrame < 0)
                    currentFrame = animFrameDuration - 1;


                foreach (var e in CurrentAnimationInfo.AnimationEvents)
                {
                    if (e.AnimationFrame == currentFrame)
                        events.Add(e);
                }


            }

            OldFrame = currentFrame;

            if (IgnoreAnimationEvents) return;

            foreach (var e in events)
                OnAnimationEvent?.Invoke(e);

        }

        public virtual void Update(float deltaTime)
        {
            if (RiggedModel is null) return;

            RiggedModel.animationPose.BoneOverrides = new Dictionary<string, BonePoseBlend>();

            RiggedModel.additionalMeshOffsets = additionalMeshOffsets;
            RiggedModel.additionalLocalOffsets = additionalLocalOffsets;

            RiggedModel.UpdateVisual = (isRendered && UpdatePose) || AlwaysUpdateVisual || playingRootMotion || Level.ChangingLevel || RiggedModel.loopAnimation == false;
            RiggedModel.Update(deltaTime);

            newAnimInterpolationProgress += deltaTime * newAnimInterpolationSpeed;

            if(RiggedModel.UpdateVisual && newAnimInterpolationProgress>0 && newAnimInterpolationProgress<1)
            {
                PastePoseLocal(GetPoseLocal(), true, false);

            }


            if (deltaTime != 0)
                UpdateAnimationEvents(deltaTime<0);


        }

        public void SetInterpolationEnabled(bool enabled)
        {
            if (RiggedModel is null) return;

            RiggedModel.UseStaticGeneratedFrames = !enabled;
        }

        public void PlayAnimation(int id, bool looped = true, float interpolationTime = 0.2f)
        {

            if (RiggedModel is null) return;

            if (interpolationTime > 0.001)
                oldAnimPose = GetPoseLocal();

            newAnimInterpolationSpeed = 1 / interpolationTime;
            newAnimInterpolationProgress = 0;

            bool firstAnim = RiggedModel.currentAnimation == -1;

            OldFrame = 0;

            RiggedModel.SetAnimation(id);
            RiggedModel.BeginAnimation(RiggedModel.CurrentPlayingAnimationIndex);
            RiggedModel.loopAnimation = looped;

            if (firstAnim)
            {
                RiggedModel.Update(0);
                newAnimInterpolationProgress = 1;
            }

            RiggedModel.Update(0f);
            RiggedModel.ResetRootMotion();
            RootMotionPositionOffset = Vector3.Zero;
            OldRootMotion = Vector3.Zero;

           

            SetCurrentAnimationInfo();
        }

        bool playingRootMotion = false;

        public void PlayAnimation(string name, bool looped = true, float interpolationTime = 0.2f, bool rootMotion = false)
        {

            if (RiggedModel is null) return;

            if (interpolationTime > 0.001)
                oldAnimPose = GetPoseLocal();

            newAnimInterpolationSpeed = 1/interpolationTime;
            newAnimInterpolationProgress = 0;

            bool firstAnim = RiggedModel.currentAnimation == -1;

            OldFrame = 0;

            playingRootMotion = rootMotion;

            RiggedModel.SetAnimation(name);
            RiggedModel.BeginAnimation(RiggedModel.CurrentPlayingAnimationIndex);
            RiggedModel.loopAnimation = looped;

            if (firstAnim)
            {
                RiggedModel.Update(0);
                newAnimInterpolationProgress = 1;
            }

            RiggedModel.Update(0f);

            RiggedModel.ResetRootMotion();
            RootMotionPositionOffset = Vector3.Zero;
            OldRootMotion = Vector3.Zero;

            PullRootMotion();

            SetCurrentAnimationInfo();

            if(interpolationTime > 0f)
                Update(0.001f);

        }

        protected void SetCurrentAnimationInfo()
        {

            int id = RiggedModel.currentAnimation;

            string name = RiggedModel.GetCurrentAnimationName();

            foreach (var anim in animationInfos)
            {
                if(anim.AnimationName == "")
                {
                    if(anim.AnimationIndex< RiggedModel.originalAnimations.Count)
                    anim.AnimationName = RiggedModel.originalAnimations[anim.AnimationIndex].animationName;
                }
            }

            foreach (var anim in animationInfos)
            {
                if (anim.AnimationName == name)
                {

                    CurrentAnimationInfo = anim;

                    return;
                }
            }



            foreach (var anim in animationInfos)
            {
                if(anim.AnimationIndex == id)
                {

                    CurrentAnimationInfo = anim;
                    

                    return;
                }
            }

            var newAnim = new AnimationInfo { AnimationIndex = id, AnimationName = name };

            CurrentAnimationInfo = newAnim;

            animationInfos.Add(newAnim);
            OldFrame = -1;
        }

        public void SetAnimation(int id = 0)
        {
            if (RiggedModel is null) return;
            RiggedModel.SetAnimation(id);
        }

        public void SetAnimation(string name = "")
        {
            if (RiggedModel is null) return;
            RiggedModel.SetAnimation(name);
        }

        public Matrix GetBoneMatrix(int id)
        {
            if (RiggedModel == null) return Matrix.Identity;

            foreach (var bone in RiggedModel.flatListToAllNodes)
            {
                if (bone.boneID == id)
                    return bone.CombinedTransformMg * GetWorldMatrix();
            }

            return Matrix.Identity;

        }

        Dictionary<string, RiggedModel.RiggedModelNode> namesToBones = new Dictionary<string, RiggedModel.RiggedModelNode>();
        RiggedModel.RiggedModelNode[] idToBone = new RiggedModelNode[128];

        public Matrix GetBoneMatrix(string name)
        {
            if (RiggedModel == null) return Matrix.Identity;

            if (namesToBones.ContainsKey(name))
                return namesToBones[name].CombinedTransformMg * GetWorldMatrix();

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

        public Matrix GetBoneMatrix(string name, Matrix worldMatrix)
        {
            if (RiggedModel == null) return Matrix.Identity;

            if (namesToBones.ContainsKey(name))
                return namesToBones[name].CombinedTransformMg * worldMatrix;

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

        public Matrix GetBoneMatrix(int id, Matrix worldMatrix)
        {
            if (RiggedModel == null) return Matrix.Identity;

            if(id<0) return Matrix.Identity;

            var foundBone = idToBone[id];

            if(foundBone != null)
            {
                return foundBone.CombinedTransformMg * worldMatrix;
            }

            foreach (var bone in RiggedModel.flatListToAllNodes)
            {
                idToBone[bone.boneID] = bone;


                if (bone.boneID == id)
                {
                    return bone.CombinedTransformMg * GetWorldMatrix();
                }
            }

            return Matrix.Identity;

        }

        public int GetBoneId(string name)
        {
            if (RiggedModel == null) return -1;

            foreach (var bone in RiggedModel.flatListToBoneNodes)
            {
                if (bone.name.ToLower() == name.ToLower())
                    return bone.boneID;
            }

            return -1;
        }

        public bool IsAnimationPlaying()
        {

            if(RiggedModel == null) return false;

            return RiggedModel.animationRunning;
        }

        protected override Matrix GetLocalOffset()
        {
            return Matrix.CreateTranslation(RootMotionPositionOffset);
        }

        protected override Matrix GetLocalRotationOffset()
        {
            return MathHelper.GetRotationMatrix(RootMotionRotationOffset);
        }

        public override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(0.01f) * base.GetWorldMatrix();
        }

        Matrix[] finalizedBones = new Matrix[128];

        public override void RenderPreparation()
        {
            base.RenderPreparation();

            if (RiggedModel is null) return;

            if (Viewmodel && Position.Length() < 0.1f && Camera.position != Vector3.Zero)
            {
                frameStaticMeshData.IsRendered = false;
                return;
            }

            CalculateBoundingSphereWithInterval();

            RiggedModel.globalShaderMatrixs.CopyTo(finalizedBones, 0);

        }

        public override void DrawShadow(bool closeShadow = false, bool veryClose = false, bool viewmodel = false)
        {
            if (!CastShadows) return;

            if (Viewmodel && viewmodel == false) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;

            Effect effect = GameMain.Instance.render.ShadowMapEffect;

            float bias = 0.03f;

            if (closeShadow)
                bias = 0.013f;

            if (veryClose)
                bias = 0.004f;

            if (viewmodel)
                bias = 0.001f;

            bias *= NormalBiasScale;
            bias /= MathHelper.Lerp(Graphics.ShadowResolutionScale,1, 0.5f);
            bias *= Graphics.LightDistanceMultiplier;

            effect.Parameters["bias"].SetValue(bias);

            effect.Parameters["depthBias"].SetValue(BackFaceShadows ? 0.0001f : 0);

            if (viewmodel)
            {
                graphicsDevice.RasterizerState = isNegativeScale() ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
            }
            else
            {
                if (BackFaceShadows)
                {
                    graphicsDevice.RasterizerState = isNegativeScale() == false ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
                }
                else
                {
                    //graphicsDevice.RasterizerState = isNegativeScale() ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
                    graphicsDevice.RasterizerState = RasterizerState.CullNone;
                }

                //graphicsDevice.RasterizerState = isNegativeScale() ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
            }



            effect.Parameters["Bones"].SetValue(finalizedBones);

            if (closeShadow)
                Graphics.LightViewProjectionClose = frameStaticMeshData.LightViewClose * frameStaticMeshData.LightProjectionClose;
            else if (veryClose)
                Graphics.LightViewProjectionVeryClose = frameStaticMeshData.LightViewVeryClose * frameStaticMeshData.LightProjectionVeryClose;
            else if (viewmodel)
                Graphics.LightViewProjectionViewmodel = frameStaticMeshData.LightViewmodelView * frameStaticMeshData.LightViewmodelProjection;
            else
                Graphics.LightViewProjection = frameStaticMeshData.LightView * frameStaticMeshData.LightProjection;

            if (RiggedModel != null)
            {
                if (Graphics.LightDistanceMultiplier > 0.9)
                {
                    if (closeShadow)
                        if (Graphics.DirectionalLightFrustrumClose.Contains(boundingSphere) == ContainmentType.Disjoint) return;

                    if (veryClose)
                        if (Graphics.DirectionalLightFrustrumVeryClose.Contains(boundingSphere) == ContainmentType.Disjoint) return;
                }
                foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                {

                    MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                    if (meshPartData != null)
                    {
                       // if (finalizedMeshHide.Contains(meshPartData.Name))
                            //continue;
                    }

                    // Set the vertex buffer and index buffer for this mesh part
                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    graphicsDevice.Indices = meshPart.IndexBuffer;


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
                    }
                    else if (viewmodel)
                    {
                        effect.Parameters["Projection"].SetValue(frameStaticMeshData.LightViewmodelProjection);
                        effect.Parameters["View"].SetValue(frameStaticMeshData.LightViewmodelView);
                    }
                    else
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

        public override void DrawDepth(bool pointLightDraw = false, bool renderTransperent = false)
        {

            if (Transparency < 1 && renderTransperent == false) return;

            if (DitherDisolve > 0) return;

            if (pointLightDraw && Viewmodel) return;

            if (Render.IgnoreFrustrumCheck == false)
                if (frameStaticMeshData.InFrustrum == false) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;

            Effect effect = GameMain.Instance.render.OcclusionEffect;

            effect.Parameters["Bones"].SetValue(finalizedBones);

            effect.Parameters["Viewmodel"].SetValue(Viewmodel);

            effect.Parameters["Masked"].SetValue(false);
            effect.Parameters["World"].SetValue(frameStaticMeshData.World);

            bool mask = Masked;

            if (Transperent)
                mask = true;

            if (RiggedModel != null)
            {

                if (Render.CustomFrustrum != null)
                {
                    if (Render.CustomFrustrum.Contains(boundingSphere) == ContainmentType.Disjoint)
                    {
                        return;
                    }
                }

                graphicsDevice.RasterizerState = Graphics.DisableBackFaceCulling || TwoSided ? RasterizerState.CullNone : (isNegativeScale() ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise);

                graphicsDevice.DepthStencilState = DepthStencilState.Default;
                graphicsDevice.BlendState = BlendState.Opaque;

                if (!mask)
                    effect.Techniques[0].Passes[0].Apply();

                if (GameMain.Instance.render.BoundingSphere.Radius == 0 || IntersectsBoundingSphere(GameMain.Instance.render.BoundingSphere))
                    foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                    {


                        MeshPartData meshPartData = meshPart.Tag as MeshPartData;

                        if (meshPartData != null)
                        {
                            if(pointLightDraw == false)
                            if (finalizedMeshHide.Contains(meshPartData.Name))
                                continue;
                        }

                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        //effect.Techniques[0].Passes[0].Apply();


                        if (mask)
                        {

                            ApplyShaderParams(effect, meshPartData);

                            effect.Parameters["Masked"].SetValue(mask);

                            effect.Techniques[0].Passes[0].Apply();

                        }


                        meshPart.Draw(graphicsDevice);

                    }
            }
        }

        public override void DrawGeometryShadow()
        {
            if (CastGeometricShadow == false) return;

            if (Viewmodel) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = GameMain.Instance.render.GeometryShadowEffect;


            if (RiggedModel != null)
            {

                effect.Parameters["Bones"].SetValue(finalizedBones);

                var hit = Physics.LineTraceForStatic((Position - Graphics.LightDirection / 8).ToPhysics(), (Position + Graphics.LightDirection.Normalized() * 100).ToPhysics());

                if (hit.HasHit == false) return;

                Vector3 hitPoint = hit.HitPointWorld;

                if (hitPoint.Y > Position.Y)
                {
                    hitPoint = Position;
                    hitPoint.Y = hit.HitPointWorld.Y;
                }


                Vector3 normal = hit.HitNormalWorld;

                if (Vector3.Dot(hit.HitNormalWorld, Graphics.LightDirection.Normalized()) > -0.5)
                {
                    normal = Vector3.UnitY;
                }

                Plane plane = new Plane(hitPoint, normal);

                Matrix shadow = Matrix.CreateShadow(Graphics.LightDirection, plane);


                effect.Parameters["World"].SetValue(frameStaticMeshData.World * -shadow);

                effect.Techniques[0].Passes[0].Apply();

                foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                {
                    // Set the vertex buffer and index buffer for this mesh part
                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    graphicsDevice.Indices = meshPart.IndexBuffer;


                    meshPart.Draw(graphicsDevice);

                }
            }
        }

        public override void DrawUnified()
        {
            if (frameStaticMeshData.IsRendered == false) { return; }



            if (Viewmodel && Render.DrawOnlyOpaque) return;

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect

            if (RiggedModel != null)
            {
                if(Render.DrawOnlyOpaque == false)
                if (DepthTestEqual)
                {
                    if (Viewmodel == false)
                    {
                        GameMain.Instance.render.OcclusionEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjection);
                        GameMain.Instance.render.OcclusionStaticEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjection);
                    }
                    else
                    {

                        GameMain.Instance.render.OcclusionEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjectionViewmodel);
                        GameMain.Instance.render.OcclusionStaticEffect.Parameters["ViewProjection"].SetValue(Camera.finalizedView * Camera.finalizedProjectionViewmodel);
                    }

                    graphicsDevice.DepthStencilState = DepthStencilState.Default;



                    BlendState blend = new BlendState { ColorWriteChannels = ColorWriteChannels.None };

                    graphicsDevice.BlendState = blend;

                    DrawDepth(renderTransperent: true);
                }
                else
                {
                    graphicsDevice.DepthStencilState = DepthStencilState.Default;
                }

                SetupBlending();

                if (DepthTestEqual)
                {
                    DepthStencilState customDepthStencilState = new DepthStencilState
                    {
                        DepthBufferEnable = true,
                        DepthBufferWriteEnable = false,
                        DepthBufferFunction = CompareFunction.LessEqual,
                        StencilEnable = false,

                    };

                    graphicsDevice.DepthStencilState = customDepthStencilState;


                    RasterizerState rasterizerState = new RasterizerState()
                    {
                        CullMode = graphicsDevice.RasterizerState.CullMode,
                        FillMode = graphicsDevice.RasterizerState.FillMode,
                        DepthBias = Viewmodel ? -0.000004f : -0.0001f,
                        MultiSampleAntiAlias = false,
                        ScissorTestEnable = graphicsDevice.RasterizerState.ScissorTestEnable,
                        SlopeScaleDepthBias = graphicsDevice.RasterizerState.SlopeScaleDepthBias,
                        DepthClipEnable = graphicsDevice.RasterizerState.DepthClipEnable

                    };

                    graphicsDevice.RasterizerState = rasterizerState;

                }

                Effect lastEffect = null;

                foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                {



                    MeshPartData meshPartData = meshPart.Tag;

                    if (meshPartData != null)
                    {
                        if (finalizedMeshHide.Contains(meshPartData.Name))
                            continue;
                    }

                    bool transperent = Transparency < 1;

                    if(transperent == false)
                    if (meshPartData.textureName != null)
                        transperent = meshPartData.textureName.Contains("_t.");

                    Effect effect = Shader.GetAndApply(transperent ? SurfaceShaderInstance.ShaderSurfaceType.Transperent : SurfaceShaderInstance.ShaderSurfaceType.Default);

                    if (Viewmodel == false)
                    {
                        if (transperent && Render.DrawOnlyOpaque) continue;
                        if (transperent == false && Render.DrawOnlyTransparent) continue;
                    }
                    else
                        partialTransparency = true;

                    if (Transperent == false && transperent && partialTransparency == false)
                        partialTransparency = true;

                    if (lastEffect != effect)
                        ApplyPointLights(effect);

                    lastEffect = effect;
                    

                    effect.Parameters["Bones"].SetValue(finalizedBones);

                    // Set the vertex buffer and index buffer for this mesh part
                    graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    graphicsDevice.Indices = meshPart.IndexBuffer;



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

        public override bool IntersectsBoundingSphere(BoundingSphere sphere)
        {
            bool intersects = false;

            if (RiggedModel is not null)
            {
                intersects = boundingSphere.Intersects(sphere);

                if (ParrentBounds != null)
                    intersects = intersects || ParrentBounds.IntersectsBoundingSphere(sphere);
            }

            return intersects;
        }

        public override void UpdateCulling()
        {
            //isRendered = Camera.frustum.Contains(boundingSphere.Transform(base.GetWorldMatrix())) != ContainmentType.Disjoint;
            isRendered = false;
            isRenderedShadow = true;
            frameStaticMeshData.IsRendered = isRendered;
            if (Visible == false) return;

            inFrustrum = false;

            if(Level.ChangingLevel == false && GameMain.SkipFrames == 0)
            if (Vector3.Distance(Position, Camera.position) > MaxRenderDistance)
                return;



            boundingSphere.Center = Position + boundingSphereOffset;

            WorldMatrix = GetWorldMatrix();

            if (Camera.frustum.Contains(boundingSphere) != ContainmentType.Disjoint)
            {
                inFrustrum = true;
            }

            if (ParrentBounds != null)
                if (Camera.frustum.Contains(ParrentBounds.boundingSphere) != ContainmentType.Disjoint)
                    inFrustrum = true;

            if (Graphics.DirectionalLightFrustrum.Contains(boundingSphere) != ContainmentType.Disjoint)
            {
                isRenderedShadow = true;

            }

            isRendered = inFrustrum && !occluded || Viewmodel || GameMain.SkipFrames > 0;
            frameStaticMeshData.IsRendered = isRendered;
        }

        public override void Destroyed()
        {
            ClearRagdollBodies();
            if (RiggedModel == null) return;
            //namesToBones = null;
            RiggedModel.Destroy();
            destroyed = true;
            GameMain.pendingDispose.Add(this);
            //RiggedModel = null;

            Visible = false;

        }

        public void ReloadHitboxes(Entity entity)
        {
            ClearRagdollBodies();
            CreateRagdollBodies(entity);
        }


        public void UpdateHitboxes()
        {

            if (isRagdoll) return;

            Matrix world = GetWorldMatrix();


            foreach (HitboxInfo hitbox in hitboxes)
            {
                if (hitbox.RagdollRigidBodyRef == null) continue;
                lock (hitbox)
                {
                    var matrix = GetBoneMatrix(hitbox.BoneId, world);

                    try
                    {

                        var boneTrans = matrix.Decompose(out _, out var rotation, out var pos);

                        if (hitbox == null) continue;
                        if (hitbox.RagdollRigidBodyRef == null) continue;

                        hitbox.RagdollRigidBodyRef?.SetTransform(pos, rotation);

                    }
                    catch (Exception ex) { }
                }
            }

        }

        public void ClearRagdollBodies()
        {

            RiggedModel?.finalOverrides?.Clear();

            foreach(HitboxInfo hitbox in hitboxes)
            {
                Physics.Remove(hitbox.Constraint);
                Physics.Remove(hitbox.Constraint2);
                hitbox.Constraint = null;
                hitbox.Constraint2 = null;
            }

            foreach (HitboxInfo hitbox in hitboxes)
            {


                Physics.Remove(hitbox.RagdollRigidBodyRef);


                Physics.RemoveFromHitboxWorld(hitbox.RagdollRigidBodyRef);



                hitbox.RagdollRigidBodyRef = null;

            }

        }

        public void CreateRagdollBodies(Entity owner)
        {

            Owner = owner;

            ClearRagdollBodies();

            Dictionary<string, HitboxInfo> keyValuePairs = new Dictionary<string, HitboxInfo>();
            
            foreach (HitboxInfo hitbox in hitboxes)
            {

                float mass = 10;


                // Create a compound shape
                CompoundShape compoundShape = new CompoundShape();

                // Create a transform for the offset
                Matrix offsetTransform = MathHelper.GetRotationMatrix(hitbox.Rotation) * Matrix.CreateTranslation(hitbox.Position / 100);

                BoxShape boxShape = new BoxShape(hitbox.Size * 0.5f);  // Bullet expects half extents

                // Add the box shape to the compound shape with the offset
                compoundShape.AddChildShape(offsetTransform.ToPhysics(), boxShape);

                compoundShape.UserObject = new Physics.CollisionSurfaceData { surfaceType = "flesh" };

                RigidBody body = Physics.CreateFromShape(owner, Vector3.One.ToPhysics(), compoundShape, addToWorld: false);

                Physics.AddToHitboxWorld(body);

                body.CollisionShape = compoundShape;

                body.Restitution = 0;
                body.DeactivationTime = 0;

                body.Gravity = Vector3.Zero.ToPhysics();


                body.Friction = 0.6f;

                body.SetMassProps(mass, body.CollisionShape.CalculateLocalInertia(mass));

                body.SetDamping(0.0f, 0f);

                body.CcdMotionThreshold = 0.00001f;
                body.CcdSweptSphereRadius = 0.1f;

                body.CollisionFlags = CollisionFlags.NoContactResponse | CollisionFlags.CustomMaterialCallback;
                body.Flags = RigidBodyFlags.DisableWorldGravity;

                body.SetBodyType(BodyType.HitBox);
                body.SetCollisionMask(BodyType.None);


                RigidbodyData data = (RigidbodyData)body.UserObject;

                data.HitboxName = hitbox.Bone;
                data.Surface = "flesh";
                body.UserObject = data;

                hitbox.RagdollRigidBodyRef = body;
                hitbox.BoneId = GetBoneId(hitbox.Bone);

                keyValuePairs.Add(hitbox.Bone, hitbox);
            }

            foreach (HitboxInfo hitbox in hitboxes)
            {

                if (keyValuePairs.ContainsKey(hitbox.Parrent) == false) continue;

                if (GetBoneByName(hitbox.Parrent) == null) continue;

                if (GetBoneByName(hitbox.Parrent).parent == null) continue;

                RigidBody parrent = keyValuePairs[hitbox.Parrent].RagdollRigidBodyRef;


                hitbox.RagdollParrentRigidBody = parrent;
                hitbox.ParrentHitbox = keyValuePairs[hitbox.Parrent];


            }

            foreach (HitboxInfo hitbox in hitboxes)
            {

                var matrix = GetBoneMatrix(hitbox.Bone);


                var boneTrans = matrix.DecomposeMatrix();
                hitbox.RagdollRigidBodyRef.SetTransform(boneTrans.Position, boneTrans.Rotation);

                hitbox.StartBoneMatrix = hitbox.RagdollRigidBodyRef.WorldTransform;

            }



        }

        public void StartRagdollForBone(string bone)
        {
            foreach(HitboxInfo hitbox in hitboxes)
            {
                if(hitbox.Bone != bone) continue;



            }
        }

        void StartRagdollForHitbox(HitboxInfo hitbox)
        {

            CalculateRagdollRestPoseForHitbox(hitbox);
            CreateConstrainsForBone(hitbox);
            ApplyPoseIntoRagdollForHitbox(hitbox);

            if (hitbox.RagdollRigidBodyRef == null) return;

            var body = hitbox.RagdollRigidBodyRef;

            Physics.RemoveFromHitboxWorld(body);
            Physics.AddToDynamicWorld(body);

            body.ActivationState = ActivationState.WantsDeactivation;



            body.Flags = RigidBodyFlags.None;

            body.Gravity = new System.Numerics.Vector3(0, -9, 0);

            body.CollisionFlags = CollisionFlags.None;

            body.CcdMotionThreshold = 0.00001f;
            body.CcdSweptSphereRadius = 0.1f;

            body.ActivationState = ActivationState.WantsDeactivation;


            body.SetCollisionMask(BodyType.World);

            body.Activate(true);


        }

        public void StartRagdoll()
        {

            foreach (HitboxInfo hitbox in hitboxes)
            {
                StartRagdollForHitbox(hitbox);
            }

            isRagdoll = true;


        }

        public void StopRagdoll()
        {

            isRagdoll = false;

            RiggedModel?.finalOverrides?.Clear();

            CreateRagdollBodies(Owner);

            //ClearRagdollBodies();

            return;
            foreach (HitboxInfo hitbox in hitboxes)
            {
                if (hitbox.RagdollRigidBodyRef == null) continue;

                var body = hitbox.RagdollRigidBodyRef;


                body.Gravity = new System.Numerics.Vector3(0, 0, 0);

                body.CollisionFlags = CollisionFlags.NoContactResponse | CollisionFlags.CustomMaterialCallback;
                body.Flags = RigidBodyFlags.DisableWorldGravity;
                body.LinearVelocity = Vector3.Zero.ToPhysics();
                body.AngularVelocity = Vector3.Zero.ToPhysics();

                body.Gravity = Vector3.Zero.ToPhysics();


                body.Friction = 0.6f;


                body.SetDamping(0.0f, 0f);

                body.CcdMotionThreshold = 0.00001f;
                body.CcdSweptSphereRadius = 0.1f;

                body.CollisionFlags = CollisionFlags.NoContactResponse | CollisionFlags.CustomMaterialCallback;
                body.Flags = RigidBodyFlags.DisableWorldGravity;

                body.ActivationState = ActivationState.WantsDeactivation;

                body.SetBodyType(BodyType.HitBox);
                body.SetCollisionMask(BodyType.None);

                body.SetCollisionMask(BodyType.HitBox);


                body.SetCollisionMask(BodyType.HitBox);


            }


        }

        public void UpdateRagdollBodies()
        {
            return;
            foreach(HitboxInfo hitbox in hitboxes)
            {

                if (hitbox.RagdollRigidBodyRef == null) continue;

                if (hitbox.RagdollParrentRigidBody == null) continue;


                Vector3 pos1 = (Matrix.Invert(hitbox.Constraint.FrameOffsetA) * hitbox.RagdollRigidBodyRef.WorldTransform).Translation;

                //DrawDebug.Text(pos1, hitbox.ConstrainLocal1.Translation.ToString(), 0.01f);

                DrawDebug.Sphere(0.05f, pos1, Vector3.Zero, 0.01f);


            }
        }

        public bool CreateHingeConstraints = false;

        void CreateConstrainsForBone(HitboxInfo hitbox)
        {
            if (hitbox.RagdollParrentRigidBody == null) return;


            Physics.Remove(hitbox.Constraint);
            Physics.Remove(hitbox.Constraint2);

            if (hitbox.ConstrainLocal2 == Matrix.Identity)
            {
                hitbox.ConstrainLocal2 = hitbox.BoneMatrix * Matrix.Invert(hitbox.RagdollParrentRigidBody.WorldTransform);//calculate relative transformation
                                                                                                                          //hitbox.ConstrainLocal2 = Matrix.Invert(hitbox.ConstrainLocal2);
            }


            hitbox.Constraint = Physics.CreateGenericConstraint(hitbox.RagdollRigidBodyRef, hitbox.RagdollParrentRigidBody, Matrix.Identity.ToPhysics(), hitbox.ConstrainLocal2.ToPhysics());

            hitbox.Constraint.AngularLowerLimit = (hitbox.AngularLowerLimit / 180 * (float)Math.PI);
            hitbox.Constraint.AngularUpperLimit = (hitbox.AngularUpperLimit / 180 * (float)Math.PI);

            if (CreateHingeConstraints && hitbox.Parrent != "")
            {

                var boneTrans = GetBoneMatrix(hitbox.Bone).DecomposeMatrix();
                var boneTransP = GetBoneMatrix(hitbox.Parrent).DecomposeMatrix();
                //hitbox.RagdollRigidBodyRef.SetTransform(boneTrans.Position, boneTrans.Rotation);


                var boneT = boneTrans.Rotation.GetRotationMatrix();
                boneT.Translation = boneTrans.Position;

                var bonePT = boneTransP.Rotation.GetRotationMatrix();
                bonePT.Translation = boneTransP.Position;

                var frame = boneT * Matrix.Invert(hitbox.RagdollParrentRigidBody.WorldTransform);

                HingeConstraint animfollowConstraint = Physics.CreateHingeConstraint(hitbox.RagdollRigidBodyRef, hitbox.RagdollParrentRigidBody, Matrix.Identity.ToPhysics(), frame.ToPhysics());
                hitbox.Constraint2 = animfollowConstraint;

                //animfollowConstraint.SetFrames(Matrix.Identity.ToPhysics(), hitbox.ConstrainLocal2.ToPhysics());

                animfollowConstraint.SetLimit(-MathF.PI, MathF.PI);
            }
        }

        public void CreateConstrains()
        {
            foreach(HitboxInfo hitbox in hitboxes)
            {


                CreateConstrainsForBone(hitbox);

                //hitbox.Constraint.OverrideNumSolverIterations = 1;

            }
        }

        void CalculateRagdollRestPoseForHitbox(HitboxInfo hitbox)
        {
            hitbox.StartBoneMatrix = hitbox.RagdollRigidBodyRef.WorldTransform;

            if (hitbox.RigidBodyMatrix == Matrix.Identity)
            {

                var matrix = GetBoneMatrix(hitbox.Bone);



                var boneTrans = matrix.DecomposeMatrix();
                hitbox.RagdollRigidBodyRef.SetTransform(boneTrans.Position, boneTrans.Rotation);


                hitbox.BoneMatrix = boneTrans.Rotation.GetRotationMatrix();
                hitbox.BoneMatrix.Translation = boneTrans.Position;

                hitbox.RigidBodyMatrix = hitbox.RagdollRigidBodyRef.WorldTransform;

                hitbox.savedRigidBodyMatrix = hitbox.RigidBodyMatrix;

            }
            else
            {
                hitbox.RagdollRigidBodyRef.WorldTransform = hitbox.savedRigidBodyMatrix.ToPhysics();
            }
        }

        public void CalculateRagdollRestPose()
        {
            foreach (HitboxInfo hitbox in hitboxes)
            {
                if (hitbox.RagdollRigidBodyRef == null) continue;

                CalculateRagdollRestPoseForHitbox(hitbox);

            }
        }

        public float RagdollHingeForce = 0;

        public void UpdateDynamicRagdoll()
        {
            foreach(var hitbox in hitboxes)
            {

                if (hitbox.Constraint2 == null) continue;

                var animfollowConstraint = hitbox.Constraint2 as HingeConstraint;


                animfollowConstraint.SetLimit(-MathF.PI * (1f- RagdollHingeForce), MathF.PI * (1f - RagdollHingeForce));
            }
        }

        void ApplyPoseIntoRagdollForHitbox(HitboxInfo hitbox)
        {
            hitbox.RagdollRigidBodyRef.WorldTransform = hitbox.StartBoneMatrix.ToPhysics();
        }

        public void ApplyPoseIntoRagdoll()
        {

            foreach (HitboxInfo hitbox in hitboxes)
            {

                ApplyPoseIntoRagdollForHitbox(hitbox);

            }
        }


        public void ApplyRagdollToMesh()
        {

            if (isRagdoll == false) return;

            UpdateRagdollBodies();

            Vector3[] positions = new Vector3[hitboxes.Count];

            int i = -1;

            foreach (HitboxInfo hitbox in hitboxes)
            {

                i++;

                if (hitbox.RagdollRigidBodyRef == null) continue;

                // Get the world transform matrix from the ragdoll rigid body
                var worldTransform = hitbox.RagdollRigidBodyRef.WorldTransform;


                var finalMatrix = worldTransform;

                positions[i] = finalMatrix.Translation;

                var trans = ((Matrix)(finalMatrix)).DecomposeMatrix();

                DrawDebug.Text(worldTransform.Translation, trans.Scale.ToString(), 0.01f);

                SetWorldPositionOverride(hitbox.Bone, finalMatrix);

                //if (hitbox.Constraint != null)
                    //DrawDebug.Text(hitbox.RagdollRigidBodyRef.WorldTransform.Translation, hitbox.Constraint.TranslationalLimitMotor.CurrentLinearDiff.ToString(), 0.01f);


            }


        }

        public void DisplayBoneScales()
        {
            if(RiggedModel == null) return;

            foreach (var bone in RiggedModel.flatListToAllNodes)
            {

                var trans = GetBoneMatrix(bone.name).DecomposeMatrix();

                DrawDebug.Text(trans.Position, bone.name + " " + trans.Scale.ToString(), 0.01f);

                Console.WriteLine(bone.name);
                Console.WriteLine(trans.Scale);

            }

        }

        public static Dictionary<string, SkeletalMeshMeta> LoadedMeta = new Dictionary<string, SkeletalMeshMeta>();

        public void LoadMeshMetaFromFile(string path)
        {
            path = AssetRegistry.FindPathForFile(path);

            SkeletalMeshMeta meta;

            string text;


            if (LoadedMeta.ContainsKey(path))
            {
                meta = LoadedMeta[path].CloneWithDefaults();
            }
            else
            {

                if (File.Exists(path + ".skeletaldata") == false) return;

                using (var asset = AssetRegistry.GetFileStreamFromPath(path + ".skeletaldata"))
                {



                    var stream = asset.FileStream;

                    var reader = new StreamReader(stream);

                    text = reader.ReadToEnd();

                    JsonSerializerOptions options = new JsonSerializerOptions();

                    foreach (var conv in Helpers.JsonConverters.GetAll())
                        options.Converters.Add(conv);

                    meta = JsonSerializer.Deserialize<SkeletalMeshMeta>(text, options);

                    lock (LoadedMeta)
                    LoadedMeta.TryAdd(path, meta);

                    meta = meta.CloneWithDefaults();



                }

            }




            if (meta.hitboxes != null)
                hitboxes = meta.hitboxes.ToList();

            if (meta.animationInfos != null)
                animationInfos = meta.animationInfos.ToList();

        }

        public void SaveMeshMetaToFile(string path)
        {
            SkeletalMeshMeta meta = new SkeletalMeshMeta();

            meta.hitboxes = hitboxes.ToArray();
            meta.animationInfos = animationInfos.ToArray();

            JsonSerializerOptions options = new JsonSerializerOptions();

            foreach (var conv in Helpers.JsonConverters.GetAll())
                options.Converters.Add(conv);

            string text = JsonSerializer.Serialize(meta, options);

            path = AssetRegistry.FindPathForFile(path);

            File.WriteAllText(path + ".skeletaldata", text);

        }




        public override void AddNormalsToPositionNormalDictionary(ref Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)> positionToNormals)
        {

            if (RiggedModel == null) return;

            foreach (var meshPart in RiggedModel.meshes)
            {

                VertexData[] vertices = new VertexData[meshPart.NumberOfVertices];

                meshPart.VertexBuffer.GetData(vertices);

                AddNormalsToPositionNormalDictionary(vertices, ref positionToNormals);

            }
        }

        public override void GenerateSmoothNormalsFromDictionary(Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)> positionToNormals)
        {
            if (RiggedModel == null) return;

            foreach (var meshPart in RiggedModel.meshes)
            {

                VertexData[] vertices = new VertexData[meshPart.NumberOfVertices];

                meshPart.VertexBuffer.GetData(vertices);

                var data = GenerateSmoothNormalsForBuffer(vertices, positionToNormals);

                meshPart.VertexBuffer.SetData(data, 0, data.Length);
            }
        }

        public AnimationState GetAnimationState()
        {
            return new AnimationState { CurrentFrame = GetCurrentAnimationFrame(),OldFrame = OldFrame, CurrrentAnimation = GetCurrentAnimationName(), Loop = RiggedModel.loopAnimation, PlayingRootMotion = playingRootMotion, RunningAnimation = RiggedModel.animationRunning };
        }

        public void SetAnimationState(AnimationState animationState)
        {

            playingRootMotion = animationState.PlayingRootMotion;
            PlayAnimation(animationState.CurrrentAnimation, animationState.Loop,0);
            SetCurrentAnimationFrame(animationState.CurrentFrame);
            PullRootMotion();
            OldFrame = animationState.OldFrame;
            RiggedModel.animationRunning = true;

            Update(0.0001f);

            RiggedModel.animationRunning = animationState.RunningAnimation;

        }

        public struct AnimationState
        {

            [JsonInclude]
            public string CurrrentAnimation = "";

            [JsonInclude]
            public int CurrentFrame = 0;

            [JsonInclude]
            public int OldFrame = 0;

            [JsonInclude]
            public bool Loop = false;

            [JsonInclude]
            public bool PlayingRootMotion = false;

            [JsonInclude]
            public bool RunningAnimation = false;

            public AnimationState()
            {
            }
        }


    }



    public struct AnimationPose
    {
        public Dictionary<string, Matrix> Pose = new Dictionary<string, Matrix>();
        public Dictionary<string, BonePoseBlend> BoneOverrides = new Dictionary<string, BonePoseBlend>();

        public MathHelper.Transform RootMotion = new MathHelper.Transform();

        public AnimationPose() { }

        public AnimationPose(AnimationPose original)
        {
            foreach (var key in original.Pose.Keys)
            {
                Pose.Add(key, original.Pose[key]);
            }

            foreach (var key in original.BoneOverrides.Keys)
            {
                BoneOverrides.Add(key, original.BoneOverrides[key]);
            }
        }

        public void LayeredBlend(RiggedModelNode node, AnimationPose pose, float progress = 1, float meshSpaceRotation = 1)
        {

            if (progress < 0.001f) return;

            if (node == null) return;
            ApplyNodeChildrenOnPose(node, pose, progress);

            if (meshSpaceRotation > 0.01)
            {

                var newTransform = node.LocalTransformMg * node.parent.CombinedTransformMg;

                if (BoneOverrides.ContainsKey(node.name) == false)
                {
                    BoneOverrides.TryAdd(node.name, new BonePoseBlend { progress = progress * meshSpaceRotation, transform = newTransform });
                }
                else
                {
                    var oldTransform = BoneOverrides[node.name].transform.DecomposeMatrix();


                    var overr = BoneOverrides[node.name];



                    overr.transform = MathHelper.Transform.Lerp(oldTransform, newTransform.DecomposeMatrix(), progress).ToMatrix();

                    overr.progress = progress * meshSpaceRotation;

                    BoneOverrides[node.name] = overr;

                }
            }

        }

        void ApplyNodeChildrenOnPose(RiggedModelNode node, AnimationPose pose, float progress)
        {
            foreach (RiggedModelNode n in node.children)
            {

                ApplyNodeChildrenOnPose(n, pose, progress);

                if (pose.Pose.ContainsKey(n.name) == false) continue;

                if (Pose.ContainsKey(n.name) == false)
                    Pose.Add(n.name, Matrix.Identity);

                if (progress <= 0.001)
                {
                    continue;
                }
                else if (progress > 0.999)
                {
                    Pose[n.name] = pose.Pose[n.name];
                    continue;
                }

                MathHelper.Transform a = Pose[n.name].DecomposeMatrix();
                MathHelper.Transform b = pose.Pose[n.name].DecomposeMatrix();

                Pose[n.name] = MathHelper.Transform.Lerp(a, b, progress).ToMatrix();

            }
        }

        public AnimationPose Copy()
        {
            AnimationPose pose = new AnimationPose();

            pose.Pose = new Dictionary<string, Matrix>(Pose);

            pose.BoneOverrides = new Dictionary<string, BonePoseBlend>(BoneOverrides);

            return pose;

        }




    }


    public struct SkeletalMeshMeta
    {
        [JsonInclude]
        public HitboxInfo[] hitboxes;

        [JsonInclude]
        public AnimationInfo[] animationInfos;

        // Create a copy of this instance where non-JsonInclude members are left at default
        public SkeletalMeshMeta CloneWithDefaults()
        {
            return new SkeletalMeshMeta
            {
                hitboxes = hitboxes?.Select(h => h.CloneWithDefaults()).ToArray(),
                animationInfos = animationInfos?.Select(a => a.CloneWithDefaults()).ToArray()
            };
        }
    }

    public class AnimationEvent
    {
        [JsonInclude]
        public int AnimationFrame = 0;

        [JsonInclude]
        public string Name = "event";

        public override string ToString()
        {
            return $"name: {Name}    Frame: {AnimationFrame}";
        }

        // Only copy the JsonIncluded fields
        public AnimationEvent CloneWithDefaults()
        {
            return new AnimationEvent
            {
                AnimationFrame = this.AnimationFrame,
                Name = this.Name
            };
        }
    }

    public class AnimationInfo
    {
        [JsonInclude]
        public int AnimationIndex = -1;

        [JsonInclude]
        public string AnimationName = "";

        [JsonInclude]
        public AnimationEvent[] AnimationEvents = new AnimationEvent[0];

        public AnimationEvent AddEvent(int frame, string name)
        {
            var list = AnimationEvents.ToList();

            var e = new AnimationEvent { AnimationFrame = frame, Name = name };

            list.Add(e);

            AnimationEvents = list.ToArray();

            return e;
        }

        public void RemoveEvent(AnimationEvent animationEvent)
        {
            var list = AnimationEvents.ToList();
            list.Remove(animationEvent);
            AnimationEvents = list.ToArray();
        }

        public AnimationEvent GetFromIndex(int index)
        {
            if (index < AnimationEvents.Length)
            {
                return AnimationEvents[index];
            }

            return null;
        }

        // Copy only the fields marked with JsonInclude.
        public AnimationInfo CloneWithDefaults()
        {
            return new AnimationInfo
            {
                AnimationIndex = this.AnimationIndex,
                AnimationName = this.AnimationName,
                AnimationEvents = AnimationEvents?.Select(e => e.CloneWithDefaults()).ToArray()
            };
        }
    }

    public class HitboxInfo
    {
        [JsonInclude]
        public string Bone = "";

        [JsonInclude]
        public string Parrent = "";

        [JsonInclude]
        public System.Numerics.Vector3 Position;

        [JsonInclude]
        public System.Numerics.Vector3 Rotation;

        [JsonInclude]
        public System.Numerics.Vector3 Size;

        [JsonInclude]
        public Matrix ConstrainLocal1 = Matrix.Identity;

        [JsonInclude]
        public Matrix ConstrainLocal2 = Matrix.Identity;

        // These fields will be left at their default values when cloning
        public Matrix RigidBodyMatrix = Matrix.Identity;

        [JsonInclude]
        public Matrix savedRigidBodyMatrix = Matrix.Identity;

        [JsonInclude]
        public System.Numerics.Vector3 AngularLowerLimit = new System.Numerics.Vector3(-3.14f / 15, -3.14f / 15, -3.14f / 4) / (float)Math.PI * 180;

        [JsonInclude]
        public System.Numerics.Vector3 AngularUpperLimit = new System.Numerics.Vector3(3.14f / 15, 3.14f / 15, 3.14f / 4) / (float)Math.PI * 180;

        public HitboxInfo ParrentHitbox;
        public Matrix BoneMatrix = Matrix.Identity;
        public Matrix StartBoneMatrix = Matrix.Identity;
        public RigidBody RagdollRigidBodyRef;
        public RigidBody RagdollParrentRigidBody;
        public Generic6DofConstraint Constraint;
        public TypedConstraint Constraint2;
        public int BoneId = -1;

        // Clone only the properties decorated with [JsonInclude]
        public HitboxInfo CloneWithDefaults()
        {
            return new HitboxInfo
            {
                Bone = this.Bone,
                Parrent = this.Parrent,
                Position = this.Position,
                Rotation = this.Rotation,
                Size = this.Size,
                ConstrainLocal1 = this.ConstrainLocal1,
                ConstrainLocal2 = this.ConstrainLocal2,
                savedRigidBodyMatrix = this.savedRigidBodyMatrix,
                AngularLowerLimit = this.AngularLowerLimit,
                AngularUpperLimit = this.AngularUpperLimit
                // Note: other non-JsonInclude fields (e.g. RigidBodyMatrix, ParrentHitbox, etc.)
                // are not copied so they will be at their default values.
            };
        }
    }

    public struct BonePoseBlend
    {
        public Matrix transform;
        public float progress;
    }

}

