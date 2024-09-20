using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine;
using static RetroEngine.MathHelper;
//using Microsoft.Xna.Framework.Input;

//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Diagnostics;


/*  TODO 
//  lots to do still and think about.
// Fix the amimations there is a small discrepancy between my animations and visual studios model viewer which also isn't perfect it is irking me.
// link the textures to the shader in the model 
//  a) change the texture for diffuse to a list so a model can allow for multi texturing i dunno how many or if any models actually use this.
//     ... however assimp holds texture arrays for diffuse uv textures and others so it seems to be something that exists ill have to go back and change that.
//  b) add in the normal mapping to the shader. 
//  c) add in a normal map generator algorithm, this would be nice.
//  d) add in heightmapping humm i never actually got around to writing one yet so that will need a seperate test project with a proof of concept effect.
//
// Other kinds of animations need to be added but since these are primarily world space transforms that will be quite a bit easier.
// I just need to decide how to handle it, could even just use the bone 0 transform for that which i have added as a dummy node.
//
// Deformer animations also need to be added i need to do a little more research before i add that 
// Althought the principal is simple.
// I have no idea how assimp intends these deformations to be used as mesh vertice deforms ir bone deforms for example ect...
// 
// Improve the draw method so it draws itself better improve the effect file so i can test the above stuff.
// I suppose i should read things like the specular and diffuse values ect... 
// but this is minor most people will use there own effect pixel shaders anyways.
// Maybe stick the effect into the model.
//
// There is also the notion of were these steps belong some maybe belong in the loader some maybe the model.
// there is also the question of do i want a seperate temporary model in the loader and then a seperate xnb written and read model later on.
// there is also the question of is that even smart at this point and maybe should it just be bypassed altogether and the files written in csv xml or as dats.
//
 */

/// <summary>
/// 
/// </summary>
namespace RetroEngine.Skeletal
{
    /// <summary>
    /// The rigged model  i stuffed the classes the rigged model uses in it as nested classes just to keep it all together.
    /// I don't really see anything else using these classes anyways they are really just specific to the model.
    /// Even the loader should go out of scope when its done loading a model and then even it is just going to be a conversion tool.
    /// After i make a content reader and writer for the model class there will be no need for the loader except to change over new models.
    /// However you don't really have to have it in xnb form at all you could use it as is but it does a lot of processing so... meh...
    /// </summary>
    public class RiggedModel
    {
        #region members

        public bool consoleDebug = false;

        public Effect effect;
        public int numberOfBonesInUse = 0;
        public int numberOfNodesInUse = 0;
        public int maxGlobalBones = 128; // 78       
        public Matrix[] globalShaderMatrixs; // these are the real final bone matrices they end up on the shader.
        public List<RiggedModelNode> flatListToBoneNodes = new List<RiggedModelNode>();
        public List<RiggedModelNode> flatListToAllNodes = new List<RiggedModelNode>();
        public RiggedModelMesh[] meshes;
        public RiggedModelNode rootNodeOfTree; // The actual model root node the base node of the model from here we can locate any node in the chain.
        public RiggedModelNode firstRealBoneInTree; // unused as of yet. The actual first bone in the scene the basis of the users skeletal model he created.
        //public RiggedModelNode globalPreTransformNode; // the accumulated orientations and scalars prior to the first bone acts as a scalar to the actual bone local transforms from assimp.

        // initial assimp animations
        public List<RiggedAnimation> originalAnimations = new List<RiggedAnimation>();
        public int currentAnimation = -1;
        public int currentFrame = 0;
        public bool animationRunning = false;
        public bool loopAnimation = false;
        //public float timeStart = 0f;
        public float currentAnimationFrameTime = 0;

        public float AnimationTime = 0;

        public float AnimationDuration = 1;

        internal AnimationPose animationPose = new AnimationPose();

        /// <summary>
        /// Uses static animation frames instead of interpolated frames.
        /// </summary>
        public bool UseStaticGeneratedFrames = false;

        // mainly for testing to step thru each frame.
        public float overrideAnimationFrameTime = -1;

        public bool UpdateVisual = true;

        internal bool UpdateTransforms = true;

        public Dictionary<string, Matrix> additionalLocalOffsets = new Dictionary<string, Matrix>();
        public Dictionary<string, Matrix> additionalMeshOffsets = new Dictionary<string, Matrix>();
        public Dictionary<string, Matrix> finalOverrides = new Dictionary<string, Matrix>();

        #endregion

        public Matrix World = Matrix.Identity;

        public Vector3 TotalRootMotion = new Vector3();
        public Vector3 TotalRootMotionRot = new Vector3();

        public MathHelper.Transform StartRootTransform = new MathHelper.Transform();

        #region methods


        /// <summary>
        /// Instantiates the model object and the boneShaderFinalMatrix array setting them all to identity.
        /// </summary>
        public RiggedModel()
        {
            globalShaderMatrixs = new Matrix[maxGlobalBones];
            for (int i = 0; i < maxGlobalBones; i++)
            {
                globalShaderMatrixs[i] = Matrix.Identity;
            }
        }

        ~RiggedModel()
        {
            flatListToAllNodes = null;
            flatListToBoneNodes = null;
            rootNodeOfTree = null;
            firstRealBoneInTree = null;
            globalShaderMatrixs = null;
            meshes = null;
            originalAnimations = null;
        }

        public void CreateBuffers()
        {
            foreach(RiggedModelMesh mesh in meshes)
            { mesh.CreateBuffers(); }
        }

        /// <summary>
        /// As stated
        /// </summary>

        /// <summary>
        /// As stated
        /// </summary>
        void SetEffectTexture(Texture2D t)
        {
            effect.Parameters["TextureA"].SetValue(t);
        }

        /// <summary>
        /// Convienience method pass the node let it set itself.
        /// This also allows you to call this is in a iterated node tree and just bypass setting non bone nodes.
        /// </summary>
        public void SetGlobalShaderBoneNode(RiggedModelNode n)
        {
            if (n.isThisARealBone)
                globalShaderMatrixs[n.boneShaderFinalTransformIndex] = n.CombinedTransformMg;
        }

        /// <summary>
        /// Update
        /// </summary>
        public void Update(float time, bool force = false)
        {

            if (animationRunning || force)
                UpdateModelAnimations(time);

            if(time > 0)
            UpdateRootMotion();

            if (UpdateVisual == false && force == false) return;
            IterateUpdate(rootNodeOfTree);
            UpdateMeshTransforms();

            

        }


        MathHelper.Transform OldRootTransform = new MathHelper.Transform();

        void UpdateRootMotion()
        {
            if (rootNodeOfTree == null)
            {
                return;
            }

            var roots = rootNodeOfTree.children.Where(c => c.name.ToLower() == "root").ToArray();


            RiggedModelNode root;
            if (roots.Length > 0)
            {
                root = roots[0];
            }else
            {
                return;
            }

            var transform = root.LocalFinalTransformMg.DecomposeMatrix();


            Vector3 motionPos = transform.Position - OldRootTransform.Position;


            motionPos = Vector3.Transform(motionPos, MathHelper.GetRotationMatrix(RootMotionOffsetRot));

            Vector3 motionRot = transform.Rotation - OldRootTransform.Rotation;

            TotalRootMotion += motionPos/100;

            if (currentFrame != 0)
                TotalRootMotionRot += motionRot;


            OldRootTransform = transform;


        }

        public Vector3 RootMotionOffset = new Vector3();
        public Vector3 RootMotionOffsetRot = new Vector3();

        void RootMotionFinishAnimation()
        {
            if (rootNodeOfTree == null)
            {
                return;
            }

            var roots = rootNodeOfTree.children.Where(c => c.name.ToLower() == "root").ToArray();


            RiggedModelNode root;
            if (roots.Length > 0)
            {
                root = roots[0];
            }
            else
            {
                return;
            }

            var transform = root.LocalFinalTransformMg.DecomposeMatrix();

            Vector3 motionPos = transform.Position;
            Vector3 motionRot = transform.Rotation;

            //motionPos = Vector3.Transform(motionPos, MathHelper.GetRotationMatrix(RootMotionOffsetRot));

            RootMotionOffset += motionPos / 100;
            RootMotionOffsetRot += motionRot - StartRootTransform.Rotation;
            
            //Console.WriteLine(motionPos);
            //TotalRootMotion += motionPos / 100;
        }

        public void UpdatePose()
        {
            IterateUpdate(rootNodeOfTree);
            animationPose = new AnimationPose();
        }

        public void SetFrame(int frame)
        {
            if(currentAnimation >= 0 && currentAnimation < originalAnimations.Count)

            AnimationTime = (float)((double)frame * originalAnimations[currentAnimation].SecondsPerFrame);
        }

        /// <summary>
        /// Gets the animation frame corresponding to the elapsed time for all the nodes and loads them into the model node transforms.
        /// </summary>
        private void UpdateModelAnimations(float gameTime)
        {
            if (originalAnimations.Count > 0 && currentAnimation < originalAnimations.Count)
            {

                AnimationTime = AnimationTime + gameTime;

                
                float animationTotalDuration;
                if (loopAnimation)
                    animationTotalDuration = (float)originalAnimations[currentAnimation].DurationInSeconds;
                else
                    animationTotalDuration = (float)originalAnimations[currentAnimation].DurationInSeconds;

                AnimationDuration = animationTotalDuration;

                while (AnimationTime < 0)
                    AnimationTime = AnimationTime + animationTotalDuration;

                currentAnimationFrameTime = (float)AnimationTime;

                // just for seeing a single frame lets us override the current frame.
                if (overrideAnimationFrameTime >= 0f)
                {
                    currentAnimationFrameTime = overrideAnimationFrameTime;
                    if (overrideAnimationFrameTime > animationTotalDuration)
                        overrideAnimationFrameTime = 0f;
                }

                

                // if we are using static frames.
                currentFrame = (int)(currentAnimationFrameTime / originalAnimations[currentAnimation].SecondsPerFrame);
                int numbOfFrames = originalAnimations[currentAnimation].TotalFrames;

                bool frameReset = false;

                // usually we aren't using static frames and we might be looping.
                if (currentAnimationFrameTime > animationTotalDuration)
                {
                    if (loopAnimation)
                    {
                        currentAnimationFrameTime = currentAnimationFrameTime - animationTotalDuration;
                        AnimationTime = 0;
                    }
                    else // animation completed
                    {
                        currentAnimationFrameTime = animationTotalDuration- 0.0001f;
                        animationRunning = false;
                        //currentFrame = originalAnimations[currentAnimation].TotalFrames - 1;
                        //currentFrame = 0;
                        //timeStart = 0;
                        //AnimationTime = 0;
                        //animationRunning = false;
                        //currentAnimationFrameTime = 0.0001f;
                    }
                    frameReset = true;
                    RootMotionFinishAnimation();

                }

                int nodeCount = originalAnimations[currentAnimation].animatedNodes.Count;
                for (int nodeLooped = 0; nodeLooped < nodeCount; nodeLooped++)
                {
                    var animNodeframe = originalAnimations[currentAnimation].animatedNodes[nodeLooped];
                    var node = animNodeframe.nodeRef;

                    if(node.RootMotionBone)
                    {
                        StartRootTransform = animNodeframe.frameOrientations[0].DecomposeMatrix();
                    }
                }

                if (UpdateVisual == false) return;
                // use the precalculated frame time lookups.
                if (UseStaticGeneratedFrames)
                {
                    // set the local node transforms from the frame.
                    if (currentFrame < numbOfFrames)
                    {
                        for (int nodeLooped = 0; nodeLooped < nodeCount; nodeLooped++)
                        {
                            var animNodeframe = originalAnimations[currentAnimation].animatedNodes[nodeLooped];
                            var node = animNodeframe.nodeRef;
                            node.LocalTransformMg = animNodeframe.frameOrientations[currentFrame];

                            if (node.RootMotionBone && frameReset)
                                OldRootTransform = node.LocalFinalTransformMg.DecomposeMatrix();

                        }
                    }
                }

                // use the calculated interpolated frames directly
                if (UseStaticGeneratedFrames == false)
                {
                    for (int nodeLooped = 0; nodeLooped < nodeCount; nodeLooped++)
                    {
                        var animNodeframe = originalAnimations[currentAnimation].animatedNodes[nodeLooped];
                        var node = animNodeframe.nodeRef;
                        // use dynamic interpolated frames
                        node.LocalTransformMg = originalAnimations[currentAnimation].Interpolate(currentAnimationFrameTime, animNodeframe, loopAnimation);
                        if (node.RootMotionBone && frameReset)
                            OldRootTransform = node.LocalFinalTransformMg.DecomposeMatrix();
                    }

                }
            }
        }

        /// <summary>
        /// Updates the node transforms
        /// </summary>
        private void IterateUpdate(RiggedModelNode node)
        {

            if(finalOverrides.ContainsKey(node.name))
            {
                Matrix trans = Matrix.CreateScale(0.01f) * finalOverrides[node.name] * Matrix.Invert(World);

                node.CombinedTransformMg = trans;

                if (node.isThisARealBone && UpdateTransforms)
                {
                    globalShaderMatrixs[node.boneShaderFinalTransformIndex] = node.OffsetMatrixMg * node.CombinedTransformMg;
                }

                // Call children
                for (int i = 0; i < node.children.Count; i++)
                {
                    IterateUpdate(node.children[i]);
                }

                return;

            }

            // Cache frequently accessed values
            Matrix additionalMesh;
            Matrix additionalLocal;

            lock (additionalLocalOffsets)
                additionalLocal = additionalLocalOffsets.TryGetValue(node.name, out var localOffset) ? localOffset : Matrix.Identity;

            lock (additionalMeshOffsets)
                additionalMesh = additionalMeshOffsets.TryGetValue(node.name, out var meshOffset) ? meshOffset : Matrix.Identity;

            if (node.parent != null)
            {
                Matrix localTransform;

                if (additionalMesh != Matrix.Identity)
                {
                    // Decompose parent transform once and reuse
                    var parentTransform = node.parent.CombinedTransformMg.DecomposeMatrix();
                    Vector3 parentPosition = parentTransform.Position;
                    parentTransform.Position = Vector3.Zero;

                    Matrix parentRotation = parentTransform.ToMatrix();
                    Matrix parentLocation = Matrix.CreateTranslation(parentPosition);

                    localTransform = node.LocalTransformMg * additionalLocal * parentRotation * additionalMesh * parentLocation;
                }
                else
                {
                    localTransform = node.LocalTransformMg * additionalLocal * node.parent.CombinedTransformMg;
                }

                if (animationPose.BoneOverrides.TryGetValue(node.name, out var boneOverride))
                {
                    // Decompose transforms once and reuse
                    var currentTransform = localTransform.DecomposeMatrix();
                    var startTransform = currentTransform;
                    var overrideTransform = boneOverride.transform.DecomposeMatrix();

                    currentTransform.Rotation = overrideTransform.Rotation;
                    currentTransform.RotationQuaternion = overrideTransform.RotationQuaternion;

                    currentTransform = MathHelper.Transform.Lerp(startTransform, currentTransform, boneOverride.progress);
                    localTransform = currentTransform.ToMatrix();
                }

                node.CombinedTransformMg = localTransform;
                node.LocalFinalTransformMg = localTransform * Matrix.Invert(node.parent.CombinedTransformMg);
            }
            else
            {

                node.CombinedTransformMg = additionalMesh * node.LocalTransformMg * additionalLocal;
                node.LocalFinalTransformMg = node.CombinedTransformMg;
            }

            if (node.isThisARealBone && UpdateTransforms)
            {
                globalShaderMatrixs[node.boneShaderFinalTransformIndex] = node.OffsetMatrixMg * node.CombinedTransformMg;
            }

            // Call children
            for (int i = 0; i < node.children.Count; i++)
            {
                IterateUpdate(node.children[i]);
            }
        }

        public void ResetBoneTransformsToRestPose(RiggedModelNode node)
        {

            return;
            // Compute the combined transform for the node
            if (node.parent != null)
            {

                node.LocalTransformMg = Matrix.Identity * -node.OffsetMatrixMg * Matrix.Invert(node.parent.CombinedTransformMg);

                node.CombinedTransformMg = node.LocalTransformMg * node.parent.CombinedTransformMg;
                node.LocalFinalTransformMg = node.LocalTransformMg * Matrix.Invert(node.parent.CombinedTransformMg);
            }
            else
            {
                node.CombinedTransformMg = node.LocalTransformMg;
                node.LocalFinalTransformMg = node.CombinedTransformMg;
            }

            // Update the global shader matrix if needed
            if (node.isThisARealBone)
            {
                globalShaderMatrixs[node.boneShaderFinalTransformIndex] = node.OffsetMatrixMg * node.CombinedTransformMg;
            }

            // Call recursively for children
            for (int i = 0; i < node.children.Count; i++)
            {
                ResetBoneTransformsToRestPose(node.children[i]);
            }
        }

        // ok ... in draw we should now be able to call on this in relation to the world transform.
        private void UpdateMeshTransforms()
        {

            if (CurrentPlayingAnimationIndex <= 0) return;

            // try to handle when we just have mesh transforms
            for (int i = 0; i < meshes.Length; i++)
            {
                if(originalAnimations.Count>0)
                    if(originalAnimations.Count> CurrentPlayingAnimationIndex)
                // This feels errr is hacky.
                //meshes[i].nodeRefContainingAnimatedTransform.CombinedTransformMg = meshes[i].nodeRefContainingAnimatedTransform.LocalTransformMg * meshes[i].nodeRefContainingAnimatedTransform.InvOffsetMatrixMg;
                if (originalAnimations[CurrentPlayingAnimationIndex].animatedNodes.Count > 1)
                {
                    meshes[i].nodeRefContainingAnimatedTransform.CombinedTransformMg = Matrix.Identity;
                }

            }
        }


        /// <summary>
        /// Sets the global final bone matrices to the shader and draws it.
        /// </summary>

        public void Destroy()
        {

            

            //flatListToAllNodes = null;
            //flatListToBoneNodes = null;
            //rootNodeOfTree = null;
            //firstRealBoneInTree = null;
            //globalShaderMatrixs = null;
            //meshes = null;
            //originalAnimations = null;

            return;
            foreach (RiggedModelNode n in flatListToAllNodes)
            {
                if (n == null) continue;

                n.children = null;
                n.parent = null;

            }


        }

        #endregion

        #region Region animation stuff

        public int CurrentPlayingAnimationIndex
        {
            get { return currentAnimation; }
            set
            {
                var n = value;
                if (n >= originalAnimations.Count)
                    n = 0;
                currentAnimation = n;
            }
        }

        /// <summary>
        /// This takes the original assimp animations and calculates a complete steady orientation matrix per frame for the fps of the animation duration.
        /// </summary>
        public void CreateStaticAnimationLookUpFrames(int fps, bool addLoopingTime)
        {
            foreach (var anim in originalAnimations)
                anim.SetAnimationFpsCreateFrames(fps, this, addLoopingTime);
        }

        public void StartCurrentAnimation()
        {
            AnimationTime = 0;
            animationRunning = true;
        }


        public void BeginAnimation(int animationIndex)
        {
            AnimationTime = 0;
            currentAnimation = animationIndex;
            animationRunning = true;
        }

        public void SetAnimation(int animationIndex)
        {
            AnimationTime = 0;
            currentAnimation = animationIndex;
            animationRunning = false;
        }

        public void SetAnimation(string name)
        {
            AnimationTime = 0;

            int index = 0;

            for (int i = 0; i < originalAnimations.Count; i++)
            {
                var anim = originalAnimations[i];
                if (anim.animationName.ToLower().EndsWith(name))
                {
                    index = i;
                    break;
                }
            }
            currentAnimation = index;
            CurrentPlayingAnimationIndex = index;
            animationRunning = false;
        }

        public void StopAnimation()
        {
            animationRunning = false;
        }

        public int id = 0;

        static int copyId = 0;

        public RiggedModel MakeCopy()
        {
            RiggedModel copy = new RiggedModel();

            copy.id = copyId;

            copyId++;

            copy.numberOfBonesInUse = numberOfBonesInUse;
            copy.numberOfNodesInUse = numberOfNodesInUse;
            copy.maxGlobalBones = maxGlobalBones;
            copy.globalShaderMatrixs = new Matrix[128];
            copy.meshes = meshes;
            copy.currentAnimation = currentAnimation;
            copy.currentFrame = currentFrame;
            copy.animationRunning = animationRunning;
            copy.loopAnimation = loopAnimation;
            copy.UseStaticGeneratedFrames = UseStaticGeneratedFrames;
            copy.overrideAnimationFrameTime = overrideAnimationFrameTime;

            Dictionary<RiggedModelNode, RiggedModelNode> cloneNodesMap;

            cloneNodesMap = new Dictionary<RiggedModelNode, RiggedModelNode>();


            foreach (var node in flatListToAllNodes)
            {
                cloneNodesMap.Add(node, new RiggedModelNode());
            }

            foreach (var node in flatListToBoneNodes)
            {
                cloneNodesMap.TryAdd(node, new RiggedModelNode());
            }

            foreach(var key in cloneNodesMap.Keys)
            {
                cloneNodesMap[key] = key.MakeCopy(cloneNodesMap, cloneNodesMap[key]);
            }

            copy.flatListToAllNodes = new List<RiggedModelNode>();

            foreach (var node in flatListToAllNodes)
            {
                copy.flatListToAllNodes.Add(cloneNodesMap[node]);
            }

            copy.flatListToBoneNodes = new List<RiggedModelNode>();
            foreach (var node in flatListToBoneNodes)
            {
                copy.flatListToBoneNodes.Add(cloneNodesMap[node]);
            }

            copy.rootNodeOfTree = cloneNodesMap[rootNodeOfTree];
            copy.firstRealBoneInTree = cloneNodesMap[firstRealBoneInTree];

            copy.originalAnimations = new List<RiggedAnimation>();

            foreach (var animation in originalAnimations)
            {
                copy.originalAnimations.Add(animation.MakeCopy(cloneNodesMap));
            }

            return copy;
        }

        #endregion


        /// <summary>
        /// Models are composed of meshes each with there own textures and sets of vertices associated to them.
        /// </summary>
        public class RiggedModelMesh
        {
            public RiggedModelNode nodeRefContainingAnimatedTransform;
            public string textureName;
            public string textureNormalMapName;
            public string textureHeightMapName;
            public Texture2D texture;
            public Texture2D textureNormalMap;
            public Texture2D textureHeightMap;
            public VertexData[] vertices;
            public int[] indices;
            public string nameOfMesh = "";
            public int NumberOfIndices { get { return indices.Length; } }
            public int NumberOfVertices { get { return vertices.Length; } }
            public int MaterialIndex { get; set; }
            public Matrix LinkedNodesOffsetMg { get; set; }
            public Matrix MeshInitialTransformFromNodeMg { get; set; }
            public Matrix MeshCombinedFinalTransformMg { get; set; }
            /// <summary>
            /// Defines the minimum vertices extent in each direction x y z in system coordinates.
            /// </summary>
            public Vector3 Min { get; set; }
            /// <summary>
            /// Defines the mximum vertices extent in each direction x y z in system coordinates.
            /// </summary>
            public Vector3 Max { get; set; }
            /// <summary>
            /// Defines the center mass point or average of all the vertices.
            /// </summary>
            public Vector3 Centroid { get; set; }

            public VertexBuffer VertexBuffer;
            public IndexBuffer IndexBuffer;

            public int VertexOffset = 0;
            public int StartIndex = 0;
            public int PrimitiveCount;


            public MeshPartData Tag;

            public void CreateBuffers()
            {

                VertexBuffer = new VertexBuffer(GameMain.Instance.GraphicsDevice, typeof(VertexData), vertices.Length, BufferUsage.None);
                VertexBuffer.SetData(vertices);
                IndexBuffer = new IndexBuffer(GameMain.Instance.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count(), BufferUsage.None);
                IndexBuffer.SetData(indices.ToArray());
                PrimitiveCount = indices.Length/3;
                //vertices = null;
            }

            public void Draw(GraphicsDevice gd)
            {
                //gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexData.VertexDeclaration);

                gd.DrawIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            VertexOffset,
                            StartIndex,
                            PrimitiveCount);

            }

        }

        /// <summary>
        /// A node of the rigged model is really a transform joint some are bones some aren't. These form a heirarchial linked tree structure.
        /// </summary>
        public class RiggedModelNode
        {
            public string name = "";
            public int boneShaderFinalTransformIndex = -1;
            public RiggedModelNode parent;
            public List<RiggedModelNode> children = new List<RiggedModelNode>();

            public List<int> meshIndices;

            public bool RootMotionBone = false;

            // probably don't need most of these they are from the debug phase.
            public bool isTheRootNode = false;
            public bool isTheGlobalPreTransformNode = false; // marks the node prior to the first bone...   (which is a accumulated pre transform multiplier to other bones)?.
            public bool isTheFirstBone = false; // marked as root bone.
            public bool isThisARealBone = false; // a actual bone with a bone offset.
            public bool isANodeAlongTheBoneRoute = false; // similar to is isThisNodeTransformNecessary but can include the nodes after bones.
            public bool isThisNodeTransformNecessary = false; // has a requisite transformation in this node that a bone will need later.
            public bool isThisAMeshNode = false; // is this actually a mesh node.
            public bool isThisTheFirstMeshNode = false;
            //public RiggedModelMesh meshRef; // no point in this as there can be many refs per node we link in the opposite direction.

            /// <summary>
            /// The inverse offset takes one from model space to bone space to say it will have a position were the bone is in the world.
            /// It is of the world space transform type from model space.
            /// </summary>
            public Matrix InvOffsetMatrixMg { get { return Matrix.Invert(OffsetMatrixMg); } set { OffsetMatrixMg = Matrix.Invert(value); } }
            /// <summary>
            /// Typically a chain of local transforms from bone to bone allow one bone to build off the next. 
            /// This is the inverse bind pose position and orientation of a bone or the local inverted bind pose e.g. inverse bone position at a node.
            /// The multiplication of this value by a full transformation chain at that specific node reveals the difference of its current model space orientations to its bind pose orientations.
            /// This is a tranformation from world space towards model space.
            /// </summary>
            public Matrix OffsetMatrixMg { get; set; }
            /// <summary>
            /// The simplest one to understand this is a transformation of a specific bone in relation to the previous bone.
            /// This is a world transformation that has local properties.
            /// </summary>
            public Matrix LocalTransformMg { get; set; }

            public Matrix LocalFinalTransformMg { get; set; }
            /// <summary>
            /// The multiplication of transforms down the tree accumulate this value tracks those accumulations.
            /// While the local transforms affect the particular orientation of a specific bone.
            /// While blender or other apps my allow some scaling or other adjustments from special matrices can be combined with this.
            /// This is a world space transformation. Basically the final world space transform that can be uploaded to the shader after all nodes are processed.
            /// </summary>
            public Matrix CombinedTransformMg { get; set; }

            public override string ToString()
            {
                return name;
            }

            public RiggedModelNode MakeCopy(Dictionary<RiggedModelNode, RiggedModelNode> keyValuePairs, RiggedModelNode copy)
            {

                copy.name = name;
                copy.boneShaderFinalTransformIndex=boneShaderFinalTransformIndex;

                if(parent is not null)
                copy.parent = keyValuePairs[parent];
                
                copy.isTheRootNode = isTheRootNode;
                copy.isTheGlobalPreTransformNode=isTheGlobalPreTransformNode;
                copy.isTheFirstBone = isTheFirstBone;
                copy.isThisARealBone = isThisARealBone;
                copy.isANodeAlongTheBoneRoute = isANodeAlongTheBoneRoute;
                copy.isThisNodeTransformNecessary=isThisNodeTransformNecessary;
                copy.isThisAMeshNode=isThisAMeshNode;
                copy.isThisTheFirstMeshNode= isThisTheFirstMeshNode;

                copy.RootMotionBone = RootMotionBone;

                copy.InvOffsetMatrixMg = InvOffsetMatrixMg;
                copy.OffsetMatrixMg = OffsetMatrixMg;
                copy.LocalTransformMg = LocalTransformMg;
                copy.CombinedTransformMg = CombinedTransformMg;

                copy.children = new List<RiggedModelNode>();

                foreach(var child in children)
                {
                    copy.children.Add(keyValuePairs[child]);
                }

                return copy;

            }
        }

        /// <summary>
        /// Animations for the animation structure i have all the nodes in the rigged animation and the nodes have lists of frames of animations.
        /// </summary>
        public class RiggedAnimation
        {
            public string targetNodeConsoleName = "_none_"; //"L_Hand";

            public string animationName = "";
            public double DurationInTicks = 0;
            public double DurationInSeconds = 0;
            public double DurationInSecondsLooping = 0;
            public double TicksPerSecond = 0;
            public double SecondsPerFrame = 0;
            public double TicksPerFramePerSecond = 0;
            public int TotalFrames = 0;

            private int fps = 0;

            //public int MeshAnimationNodeCount;
            public bool HasMeshAnimations = false;
            public bool HasNodeAnimations = false;

            public List<RiggedAnimationNodes> animatedNodes;

            public RiggedAnimation MakeCopy(Dictionary<RiggedModelNode, RiggedModelNode> keyValuePairs)
            {
                RiggedAnimation copy = new RiggedAnimation();

                copy.targetNodeConsoleName = targetNodeConsoleName;
                copy.animationName = animationName;
                copy.DurationInTicks= DurationInTicks;
                copy.DurationInSeconds = DurationInSeconds;
                copy.DurationInSecondsLooping = DurationInSecondsLooping;
                copy.TicksPerSecond = TicksPerSecond;
                copy.SecondsPerFrame = SecondsPerFrame;
                copy.TicksPerFramePerSecond = TicksPerFramePerSecond;
                copy.TotalFrames = TotalFrames;
                copy.fps = fps;
                copy.HasMeshAnimations = HasMeshAnimations;
                copy.HasNodeAnimations = HasNodeAnimations;

                copy.animatedNodes= new List<RiggedAnimationNodes> ();

                foreach(var node in animatedNodes)
                {
                    copy.animatedNodes.Add(node.MakeCopy(keyValuePairs));
                }

                return copy;
            }

            public override string ToString()
            {
                return animationName;
            }
            public void SetAnimationFpsCreateFrames(int animationFramesPerSecond, RiggedModel model, bool loopAnimation)
            {
                //Console.WriteLine("________________________________________________________");
                //Console.WriteLine("Animation name: " + animationName + "  DurationInSeconds: " + DurationInSeconds + "  DurationInSecondsLooping: " + DurationInSecondsLooping);
                fps = animationFramesPerSecond;
                TotalFrames = (int)(DurationInSeconds * animationFramesPerSecond);
                TicksPerFramePerSecond = TicksPerSecond / animationFramesPerSecond;
                SecondsPerFrame = 1d / animationFramesPerSecond;
                CalculateNewInterpolatedAnimationFrames(model, loopAnimation);
            }

            private void CalculateNewInterpolatedAnimationFrames(RiggedModel model, bool loopAnimation)
            {
                // Loop nodes.
                for (int i = 0; i < animatedNodes.Count; i++)
                {
                    // Make sure we have enough frame orientations alloted for the number of frames.
                    animatedNodes[i].frameOrientations = new Matrix[TotalFrames];
                    animatedNodes[i].frameOrientationTimes = new double[TotalFrames];


                    // Loop destination frames.
                    for (int j = 0; j < TotalFrames; j++)
                    {
                        // Find and set the interpolated value from the s r t elements based on time.
                        var frameTime = j * SecondsPerFrame; // + .0001d;
                        animatedNodes[i].frameOrientations[j] = Interpolate(frameTime, animatedNodes[i], loopAnimation);
                        animatedNodes[i].frameOrientationTimes[j] = frameTime;
                    }
                }
            }


            /// <summary>
            /// ToDo when we are looping back i think i need to artificially increase the duration in order to get a slightly smoother animation from back to front.
            /// </summary>
            public unsafe Matrix Interpolate(double animTime, RiggedAnimationNodes nodeAnim, bool loopAnimation)
            {
                double durationSecs = loopAnimation ? DurationInSecondsLooping : DurationInSeconds;

                // Adjust animTime to be within the animation duration
                animTime %= durationSecs;
                if (animTime < 0) animTime += durationSecs;

                // Cache counts
                int qrotCount = nodeAnim.qrotTime.Count;
                int posCount = nodeAnim.positionTime.Count;
                int scaleCount = nodeAnim.scaleTime.Count;

                // Interpolation indices and times
                int qIndex1 = 0, qIndex2 = 0, pIndex1 = 0, pIndex2 = 0, sIndex1 = 0, sIndex2 = 0;
                double tq1 = 0, tq2 = 0, tp1 = 0, tp2 = 0, ts1 = 0, ts2 = 0;

                // Find keyframes for quaternion, position, and scale in one loop
                var qrotTimes = nodeAnim.qrotTime;
                var posTimes = nodeAnim.positionTime;
                var scaleTimes = nodeAnim.scaleTime;
                
                    for (int i = 0; i < qrotCount || i < posCount || i < scaleCount; i++)
                    {
                        if (i < qrotCount && animTime >= qrotTimes[i])
                        {
                            qIndex1 = i;
                            qIndex2 = (i + 1) % qrotCount;
                            tq1 = qrotTimes[qIndex1];
                            tq2 = qrotTimes[qIndex2] > tq1 ? qrotTimes[qIndex2] : qrotTimes[qIndex2] + durationSecs;
                        }

                        if (i < posCount && animTime >= posTimes[i])
                        {
                            pIndex1 = i;
                            pIndex2 = (i + 1) % posCount;
                            tp1 = posTimes[pIndex1];
                            tp2 = posTimes[pIndex2] > tp1 ? posTimes[pIndex2] : posTimes[pIndex2] + durationSecs;
                        }

                        if (i < scaleCount && animTime >= scaleTimes[i])
                        {
                            sIndex1 = i;
                            sIndex2 = (i + 1) % scaleCount;
                            ts1 = scaleTimes[sIndex1];
                            ts2 = scaleTimes[sIndex2] > ts1 ? scaleTimes[sIndex2] : scaleTimes[sIndex2] + durationSecs;
                        }
                    }
                

                Quaternion q1 = nodeAnim.qrot[qIndex1];
                Quaternion q2 = nodeAnim.qrot[qIndex2];
                Vector3 p1 = nodeAnim.position[pIndex1];
                Vector3 p2 = nodeAnim.position[pIndex2];
                Vector3 s1 = nodeAnim.scale[sIndex1];
                Vector3 s2 = nodeAnim.scale[sIndex2];

                // Interpolate quaternion
                float tqi = (float)((animTime - tq1) / (tq2 - tq1));
                Quaternion q = Quaternion.Slerp(q1, q2, tqi);

                // Interpolate position
                float tpi = (float)((animTime - tp1) / (tp2 - tp1));
                Vector3 p = Vector3.Lerp(p1, p2, tpi);

                // Interpolate scale
                float tsi = (float)((animTime - ts1) / (ts2 - ts1));
                Vector3 s = Vector3.Lerp(s1, s2, tsi);

                // Combine transforms directly
                Matrix transform = Matrix.CreateScale(s) * Matrix.CreateFromQuaternion(q) * Matrix.CreateTranslation(p);

                return transform;
            }

            public double GetInterpolationTimeRatio(double s, double e, double val)
            {
                if (val < s || val > e)
                    throw new Exception("RiggedModel.cs RiggedAnimation GetInterpolationTimeRatio the value " + val + " passed to the method must be within the start and end time. ");
                return (val - s) / (e - s);
            }

            
        }

        /// <summary>
        /// Each node contains lists for Animation frame orientations. 
        /// The initial srt transforms are copied from assimp and a static interpolated orientation frame time set is built.
        /// This is done for the simple reason of efficiency and scalable computational look up speed. 
        /// The trade off is a larger memory footprint per model that however can be mitigated.
        /// </summary>
        public class RiggedAnimationNodes
        {
            public RiggedModelNode nodeRef;
            public string nodeName = "";
            // in model tick time
            public List<double> positionTime = new List<double>();
            public List<double> scaleTime = new List<double>();
            public List<double> qrotTime = new List<double>();
            public List<Vector3> position = new List<Vector3>();
            public List<Vector3> scale = new List<Vector3>();
            public List<Quaternion> qrot = new List<Quaternion>();

            // the actual calculated interpolation orientation matrice based on time.
            public double[] frameOrientationTimes;
            public Matrix[] frameOrientations;

            public RiggedAnimationNodes MakeCopy(Dictionary<RiggedModelNode, RiggedModelNode> keyValuePairs)
            {
                RiggedAnimationNodes copy = new RiggedAnimationNodes();

                copy.nodeRef = keyValuePairs[nodeRef];
                copy.nodeName = nodeName;
                copy.positionTime = positionTime;
                copy.scaleTime = scaleTime;
                copy.qrotTime = qrotTime;
                copy.position = position;
                copy.scale = scale;
                copy.qrot = qrot;
                copy.frameOrientationTimes = frameOrientationTimes;
                copy.frameOrientations = frameOrientations;

                return copy;
            }

        }

    }
    

}

