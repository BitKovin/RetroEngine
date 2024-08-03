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


namespace RetroEngine
{
    public class SkeletalMesh : StaticMesh
    {
        protected RiggedModel RiggedModel;

        protected RiggedModelLoader modelReader = new RiggedModelLoader(GameMain.content, null);

        public BoundingSphere boundingSphere = new BoundingSphere();

        protected static Dictionary<string, RiggedModel> LoadedRigModels = new Dictionary<string, RiggedModel>();


        protected Dictionary<string, Matrix> additionalLocalOffsets = new Dictionary<string, Matrix>();
        protected Dictionary<string, Matrix> additionalMeshOffsets = new Dictionary<string, Matrix>();

        protected AnimationPose animationPose = new AnimationPose();

        public SkeletalMesh ParrentBounds;

        public bool UpdatePose = true;

        public List<HitboxInfo> hitboxes = new List<HitboxInfo>();

        public bool AlwaysUpdateVisual = false;

        float newAnimInterpolationProgress = 0;
        float newAnimInterpolationSpeed = 0;

        AnimationPose oldAnimPose;

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

                LoadedRigModels.Add(path, RiggedModel);
            }

            RiggedModel = LoadedRigModels[path].MakeCopy();


            RiggedModel.Update(0);

            CalculateBoundingSphere();

            GetBoneMatrix("");

            additionalLocalOffsets = RiggedModel.additionalLocalOffsets;

            additionalMeshOffsets = RiggedModel.additionalMeshOffsets;

            LoadMeshMetaFromFile(path);



            RiggedModel.overrideAnimationFrameTime = -1;
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
                        points.Add(b.CombinedTransformMg.Translation / 100);
                    }
                }
            }

            boundingSphere = BoundingSphere.CreateFromPoints(points);
            boundingSphere.Radius *= 1.1f;
            boundingSphere.Radius += 0.3f;

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


        public AnimationPose GetPose()
        {

            if (RiggedModel == null) return new AnimationPose();

            Dictionary<string, Matrix> boneNamesToTransforms = new Dictionary<string, Matrix>();

            RiggedModel.UpdatePose();

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

            foreach (string key in p.Keys)
            {
                if (namesToBones.ContainsKey(key) == false) continue;

                var node = namesToBones[key];

                if (node.isThisARealBone)
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

            //RiggedModel.animationPose.BoneOverrides = new Dictionary<string, BonePoseBlend>();

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
            if (RiggedModel != null)
                foreach (var bone in RiggedModel.flatListToBoneNodes)
                {
                    if (bone.name == name)
                        return bone;
                }

            return null;
        }

        public virtual void Update(float deltaTime)
        {
            if (RiggedModel is null) return;

            RiggedModel.animationPose.BoneOverrides = new Dictionary<string, BonePoseBlend>();

            RiggedModel.additionalMeshOffsets = additionalMeshOffsets;
            RiggedModel.additionalLocalOffsets = additionalLocalOffsets;

            RiggedModel.UpdateVisual = (isRendered && UpdatePose) || AlwaysUpdateVisual;
            RiggedModel.Update(deltaTime);

            newAnimInterpolationProgress += deltaTime * newAnimInterpolationSpeed;

            if(RiggedModel.UpdateVisual && newAnimInterpolationProgress>0 && newAnimInterpolationProgress<1)
            {
                PastePoseLocal(GetPoseLocal());
            }

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

            RiggedModel.SetAnimation(id);
            RiggedModel.BeginAnimation(RiggedModel.CurrentPlayingAnimationIndex);
            RiggedModel.loopAnimation = looped;

            if (firstAnim)
            {
                RiggedModel.Update(0);
                newAnimInterpolationProgress = 1;
            }
        }

        public void PlayAnimation(string name, bool looped = true, float interpolationTime = 0.2f)
        {

            if (RiggedModel is null) return;

            if (interpolationTime > 0.001)
                oldAnimPose = GetPoseLocal();

            newAnimInterpolationSpeed = 1/interpolationTime;
            newAnimInterpolationProgress = 0;

            bool firstAnim = RiggedModel.currentAnimation == -1;

            RiggedModel.SetAnimation(name);
            RiggedModel.BeginAnimation(RiggedModel.CurrentPlayingAnimationIndex);
            RiggedModel.loopAnimation = looped;

            if(firstAnim)
                RiggedModel.Update(0);
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
                if (bone.boneShaderFinalTransformIndex == id)
                    return bone.CombinedTransformMg * GetWorldMatrix();
            }

            return Matrix.Identity;

        }

        Dictionary<string, RiggedModel.RiggedModelNode> namesToBones = new Dictionary<string, RiggedModel.RiggedModelNode>();

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

        public int GetBoneId(string name)
        {
            if (RiggedModel == null) return -1;

            foreach (var bone in RiggedModel.flatListToBoneNodes)
            {
                if (bone.name.ToLower() == name.ToLower())
                    return bone.boneShaderFinalTransformIndex;
            }

            return -1;
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
                bias = 0.0055f;

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
                        if (Graphics.DirectionalLightFrustrumClose.Contains(boundingSphere.Transform(GetWorldMatrix())) == ContainmentType.Disjoint) return;

                    if (veryClose)
                        if (Graphics.DirectionalLightFrustrumVeryClose.Contains(boundingSphere.Transform(GetWorldMatrix())) == ContainmentType.Disjoint) return;
                }
                foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                {
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
                    if (Render.CustomFrustrum.Contains(boundingSphere.Transform(base.GetWorldMatrix())) == ContainmentType.Disjoint)
                    {
                        return;
                    }
                }

                graphicsDevice.RasterizerState = Graphics.DisableBackFaceCulling || TwoSided ? RasterizerState.CullNone : (isNegativeScale() ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise);

                if (!mask)
                    effect.Techniques[0].Passes[0].Apply();

                if (GameMain.Instance.render.BoundingSphere.Radius == 0 || IntersectsBoundingSphere(GameMain.Instance.render.BoundingSphere))
                    foreach (RiggedModel.RiggedModelMesh meshPart in RiggedModel.meshes)
                    {
                        // Set the vertex buffer and index buffer for this mesh part
                        graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                        graphicsDevice.Indices = meshPart.IndexBuffer;

                        //effect.Techniques[0].Passes[0].Apply();


                        if (mask)
                        {

                            MeshPartData meshPartData = meshPart.Tag as MeshPartData;
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

            GraphicsDevice graphicsDevice = GameMain.Instance._graphics.GraphicsDevice;
            // Load the custom effect
            Effect effect = Shader;


            effect.Parameters["Bones"].SetValue(finalizedBones);
            if (RiggedModel != null)
            {

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

        public override bool IntersectsBoundingSphere(BoundingSphere sphere)
        {
            bool intersects = false;

            if (RiggedModel is not null)
            {
                intersects = boundingSphere.Transform(base.GetWorldMatrix()).Intersects(sphere);

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

            WorldMatrix = GetWorldMatrix();

            if (Camera.frustum.Contains(boundingSphere.Transform(base.GetWorldMatrix())) != ContainmentType.Disjoint)
            {
                inFrustrum = true;
            }

            if (ParrentBounds != null)
                if (Camera.frustum.Contains(ParrentBounds.boundingSphere.Transform(base.GetWorldMatrix())) != ContainmentType.Disjoint)
                    inFrustrum = true;

            if (Graphics.DirectionalLightFrustrum.Contains(boundingSphere.Transform(base.GetWorldMatrix())) != ContainmentType.Disjoint)
            {
                isRenderedShadow = true;

            }

            isRendered = inFrustrum && !occluded || Viewmodel || GameMain.SkipFrames > 0;
            frameStaticMeshData.IsRendered = isRendered;
        }

        public override void Destroyed()
        {
            ClearHitboxBodies();
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
            ClearHitboxBodies();
            CreateHitboxBodies(entity);
        }

        public void ClearHitboxBodies()
        {
            foreach (HitboxInfo hitbox in hitboxes)
            {
                Physics.Remove(hitbox.RigidBodyRef);

                hitbox.RigidBodyRef = null;

            }

        }

        public void CreateHitboxBodies(Entity owner)
        {
            foreach (HitboxInfo hitbox in hitboxes)
            {
                RigidBody body = Physics.CreateBox(owner, hitbox.Size, 0);

                body.UserIndex2 = (int)BodyType.HitBox;

                hitbox.RigidBodyRef = body;


            }
        }

        public void UpdateHitboxes()
        {

            foreach (HitboxInfo hitbox in hitboxes)
            {
                if (hitbox.RigidBodyRef == null) continue;

                var matrix = Matrix.CreateRotationZ(hitbox.Rotation.Z / 180 * (float)Math.PI) *
                            Matrix.CreateRotationX(hitbox.Rotation.X / 180 * (float)Math.PI) *
                            Matrix.CreateRotationY(hitbox.Rotation.Y / 180 * (float)Math.PI) *
                    Matrix.CreateTranslation(hitbox.Position) * GetBoneMatrix(hitbox.Bone);

                var boneTrans = matrix.DecomposeMatrix();
                hitbox.RigidBodyRef.SetPosition(boneTrans.Position);
                hitbox.RigidBodyRef.SetRotation(boneTrans.Rotation);

            }

        }


        public void LoadMeshMetaFromFile(string path)
        {
            path = AssetRegistry.FindPathForFile(path);

            if (File.Exists(path + ".skeletaldata") == false) return;

            var stream = AssetRegistry.GetFileStreamFromPath(path + ".skeletaldata");

            var reader = new StreamReader(stream);

            string text = reader.ReadToEnd();

            JsonSerializerOptions options = new JsonSerializerOptions();

            foreach (var conv in Helpers.JsonConverters.GetAll())
                options.Converters.Add(conv);

            SkeletalMeshMeta meta = JsonSerializer.Deserialize<SkeletalMeshMeta>(text, options);

            hitboxes = meta.hitboxes.ToList();


        }

        public void SaveMeshMetaToFile(string path)
        {
            SkeletalMeshMeta meta = new SkeletalMeshMeta();

            meta.hitboxes = hitboxes.ToArray();

            JsonSerializerOptions options = new JsonSerializerOptions();

            foreach (var conv in Helpers.JsonConverters.GetAll())
                options.Converters.Add(conv);

            string text = JsonSerializer.Serialize(meta, options);

            path = AssetRegistry.FindPathForFile(path);

            File.WriteAllText(path + ".skeletaldata", text);

        }

        public struct SkeletalMeshMeta
        {

            [JsonInclude]
            public HitboxInfo[] hitboxes;

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


    }



    public struct AnimationPose
    {
        public Dictionary<string, Matrix> Pose = new Dictionary<string, Matrix>();
        public Dictionary<string, BonePoseBlend> BoneOverrides = new Dictionary<string, BonePoseBlend>();

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


    }

    public class HitboxInfo
    {
        [JsonInclude]
        public string Bone = "";

        [JsonInclude]
        public System.Numerics.Vector3 Position;
        [JsonInclude]
        public System.Numerics.Vector3 Rotation;

        [JsonInclude]
        public System.Numerics.Vector3 Size;

        public RigidBody RigidBodyRef;

    }

    public struct BonePoseBlend
    {
        public Matrix transform;
        public float progress;
    }

}

