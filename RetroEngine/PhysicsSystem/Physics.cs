﻿using BulletSharp;
using RetroEngine;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Threading;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Diagnostics;
using System.Timers;
using BulletSharp.SoftBody;
using RetroEngine.NavigationSystem;
using System.Collections;


namespace RetroEngine.PhysicsSystem
{

    [Flags]
    public enum BodyType
    {
        None = 1,
        MainBody = 2,          // 1
        HitBox = 4,            // 2
        WorldOpaque = 8,             // 4
        CharacterCapsule = 16,  // 8
        NoRayTest = 32,         // 16
        Liquid = 64,         // 16
        WorldTrasnparent = 128,

        World = WorldOpaque | WorldTrasnparent,
        GroupAll = MainBody | HitBox | World | CharacterCapsule | NoRayTest | Liquid,
        GroupHitTest = GroupAll & ~CharacterCapsule & ~Liquid,
        GroupCollisionTest = GroupAll & ~HitBox & ~Liquid,
        GroupAllPhysical = GroupHitTest & Liquid,
    }

    public struct RigidbodyData
    {
        public Entity Entity;

        public string HitboxName;

        public string Surface = "default";

        public RigidbodyData(Entity entity)
        {
            Entity = entity;
        }

    }

    public static class Physics
    {

        public static float UpdateRate = 30;

        internal static DiscreteDynamicsWorld dynamicsWorld;

        internal static DiscreteDynamicsWorld hitboxWorld;

        private static DiscreteDynamicsWorld staticWorld;
        private static List<StaticRigidBody> staticBodies = new List<StaticRigidBody>();

        private static int steps = 1;

        static List<CollisionObject> removeList = new List<CollisionObject>();

        static List<CollisionObject> collisionObjects = new List<CollisionObject>();

        static CollisionFilterCallback collisionFilterCallback = new CollisionFilterCallback();


        static SequentialImpulseConstraintSolver solver;
        static DbvtBroadphase broadphase;
        static CollisionDispatcher dispatcher;

        static Stack<MyClosestConvexResultCallback> convexResultCallbacks = new Stack<MyClosestConvexResultCallback>();
        static Stack<MyClosestRayResultCallback> ClosestRayResultCallbacks = new Stack<MyClosestRayResultCallback>();

        public class CollisionSurfaceData
        {
            public string surfaceType = "default";
        }

        static Vector3 tempVector = new Vector3();

        internal static Dictionary<CollisionObject, int> bodyTypeList = new Dictionary<CollisionObject, int>();
        internal static Dictionary<CollisionObject, int> collisionMaskList = new Dictionary<CollisionObject, int>();

        internal static void ResetBodyTypeLists()
        {
            bodyTypeList.Clear();
            collisionMaskList.Clear();
        }

        /// <summary>
        /// creating callbacks is expencive AF, so it's faster to just keep then in memory for future use
        /// </summary>
        /// <param name="step"></param>
        /// <param name="max"></param>
        public static void PopulateCallbackStacks(int step = 1, int max = 100)
        {
            lock (convexResultCallbacks)
            {
                if (convexResultCallbacks.Count < max)
                {
                    int toAdd = max - convexResultCallbacks.Count;

                    toAdd = int.Clamp(toAdd, 0, step);


                    for (int i = 0; i < toAdd; i++)
                    {
                        convexResultCallbacks.Push(new MyClosestConvexResultCallback(ref tempVector, ref tempVector));
                    }
                }
            }

            lock (ClosestRayResultCallbacks)
            {
                if (ClosestRayResultCallbacks.Count < max)
                {
                    int toAdd = max - ClosestRayResultCallbacks.Count;

                    toAdd = int.Clamp(toAdd, 0, step);


                    for (int i = 0; i < toAdd; i++)
                    {
                        ClosestRayResultCallbacks.Push(new MyClosestRayResultCallback(ref tempVector, ref tempVector));
                    }
                }
            }

        }

        public static void DebugDraw()
        {
            lock (dynamicsWorld)
            {
                dynamicsWorld.DebugDrawWorld();
            }
            lock(hitboxWorld)
            {
                hitboxWorld.DebugDrawWorld();
            }
        }

        public static void Start()
        {

            solver?.Reset();
            solver?.Dispose();
            broadphase?.Dispose();
            dispatcher?.Dispose();

            removeList.Clear();


            if (dynamicsWorld != null)
            {
                dynamicsWorld.Dispose();
                dynamicsWorld = null;
            }

            if (staticWorld != null)
            {
                staticWorld.Dispose();
                staticWorld = null;
            }

            if (hitboxWorld != null)
            {
                hitboxWorld.Dispose();
                hitboxWorld = null;
            }

            // Create a collision configuration and dispatcher
            var collisionConfig = new DefaultCollisionConfiguration();
            collisionConfig.SetConvexConvexMultipointIterations();
            collisionConfig.SetPlaneConvexMultipointIterations();

            dispatcher = new CollisionDispatcher(collisionConfig);



            // Create a broadphase and a solver
            broadphase = new DbvtBroadphase();

            

            solver = new SequentialImpulseConstraintSolver();


            // Create the dynamics world
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
            dynamicsWorld.Gravity = new Vector3(0, -9.81f, 0); // Set gravity
            dynamicsWorld.DispatchInfo.UseContinuous = true;
            dynamicsWorld.SolverInfo.NumIterations = 20;
            //dynamicsWorld.SolverInfo.SplitImpulse = 0;


            broadphase.OverlappingPairCache.SetOverlapFilterCallback(collisionFilterCallback);
            //dynamicsWorld.ForceUpdateAllAabbs = false;

            CollisionFilterCallback.ResetCache();

            staticWorld = new DiscreteDynamicsWorld(new CollisionDispatcher(new DefaultCollisionConfiguration()), new DbvtBroadphase(), new SequentialImpulseConstraintSolver(), new DefaultCollisionConfiguration());
            staticBodies.Clear();

            hitboxWorld = new DiscreteDynamicsWorld(new CollisionDispatcher(new DefaultCollisionConfiguration()), new DbvtBroadphase(), new SequentialImpulseConstraintSolver(), new DefaultCollisionConfiguration());

            dynamicsWorld.DebugDrawer = DrawDebug.instance;
            hitboxWorld.DebugDrawer = DrawDebug.instance;
        }

        public static void ResetWorld()
        {



            foreach (RigidBody body in staticBodies)
            {
                staticWorld.RemoveCollisionObject(body);
                body.CollisionShape?.Dispose();
                body.MotionState = null;
                body.Dispose();
            }

            lock (collisionObjects)
            {
                foreach (CollisionObject collisionObject in collisionObjects.ToArray())
                {

                    RigidBody body = RigidBody.Upcast(collisionObject);

                    if (body == null)
                        Remove(collisionObject);
                    else
                        Remove(body);
                }
            }
            staticBodies.Clear();
            collisionObjects.Clear();
            solver.Reset();
            broadphase.NeedCleanup = true;
            broadphase.Optimize();

            dynamicsWorld.UpdateAabbs();
            broadphase.PairCache.Dispose();



            Start();

        }

        public static Generic6DofConstraint CreateGenericConstraint(RigidBody body1, RigidBody body2, Matrix4x4? transform1 = null, Matrix4x4? transform2 = null)
        {
            var constraint = new Generic6DofConstraint(body1, body2, transform1 == null ? Matrix4x4.Identity : transform1.Value, transform2 == null ? Matrix4x4.Identity : transform2.Value, true);


            //constraint.InternalSetAppliedImpulse(0);

            // Set linear limits (translation). The first vector is the lower limit, and the second is the upper limit.
            // Use Vector3.Zero for no movement in a particular axis.
            constraint.LinearLowerLimit = Vector3.Zero; // Example limits
            constraint.LinearUpperLimit = Vector3.Zero;     // Example limits


            constraint.SetParam(ConstraintParam.StopErp,0.85f);
            constraint.SetParam(ConstraintParam.StopCfm, 0.85f);

            constraint.SetParam(ConstraintParam.Erp, 0.85f);
            constraint.SetParam(ConstraintParam.Cfm, 0.85f);

            //constraint.TranslationalLimitMotor.StopErp = Vector3.One * 100;

            //constraint.TranslationalLimitMotor.MaxMotorForce = Vector3.One* 10000000f;

            // Set angular limits (rotation). The first vector is the lower limit, and the second is the upper limit.
            // Values are in radians. Use Vector3.Zero for no rotation in a particular axis.
            constraint.AngularLowerLimit = (new Vector3(-3.14f / 15, -3.14f / 15, -3.14f / 4)); // Example limits
            constraint.AngularUpperLimit = (new Vector3(3.14f / 15, 3.14f / 15, 3.14f / 4));     // Example limits



            lock (dynamicsWorld)
                dynamicsWorld.AddConstraint(constraint, true);

            return constraint;

        }

        public static HingeConstraint CreateHingeConstraint(RigidBody body1, RigidBody body2, Matrix4x4? transform1 = null, Matrix4x4? transform2 = null)
        {


            var constraint = new HingeConstraint(body1, body2, transform1 == null ? Matrix4x4.Identity : transform1.Value, transform2 == null ? Matrix4x4.Identity : transform2.Value, true);



            lock (dynamicsWorld)
                dynamicsWorld.AddConstraint(constraint, true);

            return constraint;

        }

        public static Point2PointConstraint Create2PointConstraint(RigidBody body1, RigidBody body2, Vector3 attachPoint)
        {


            Matrix4x4.Invert(body1.WorldTransform, out var m1);
            Matrix4x4.Invert(body2.WorldTransform, out var m2);

            // Calculate the pivot points in the local space of each rigid body
            Vector3 pivotInA = Vector3.Transform(attachPoint, m1);
            Vector3 pivotInB = Vector3.Transform(attachPoint, m2);

            // Create the point-to-point constraint
            Point2PointConstraint p2pConstraint = new Point2PointConstraint(body1, body2, pivotInA, pivotInB);



            lock (dynamicsWorld)
                dynamicsWorld.AddConstraint(p2pConstraint, true);

            return p2pConstraint;

        }

        public static void PerformContactCheck(CollisionObject collisionObject, CollisionCallback callback)
        {

            try
            {
                if (collisionObject is null) return;

                lock (dynamicsWorld)
                {
                    dynamicsWorld.ContactTest(collisionObject, callback);
                }
            }
            catch (Exception e) { Logger.Log(e.Message); }
        }

        public static long SimulationTicks = 0;

        static Stopwatch physTime = new Stopwatch();

        public static void Simulate()
        {


            Task task = Task.Run(() =>
            {
                lock (hitboxWorld)
                    hitboxWorld.StepSimulation(Time.DeltaTime, 1, 0.01f);

                lock (staticWorld)
                {
                    foreach (StaticRigidBody staticRigidBody in staticBodies)
                    {
                        staticRigidBody.UpdateFromParrent();
                    }
                }

                

            });

            Task task2 = Task.Run(() => 
            {
                PopulateCallbackStacks(50, 200);
            });

            lock (dynamicsWorld)
            {

                float time = (float)physTime.Elapsed.TotalSeconds;
                time = Math.Min(time, 1 / 10f);
                physTime.Restart();



                if (GameMain.Instance.paused == false)
                {
                    lock (dynamicsWorld.CollisionObjectArray)
                    {
                        SimulationTicks += dynamicsWorld.StepSimulation(time * Time.GetFinalTimeScale(), 3, 1 / UpdateRate * Time.GetFinalTimeScale());
                    }
                }
            }

            task.Wait();
            task2.Wait();

        }

        public static void Remove(CollisionObject collisionObject)
        {
            if (collisionObject is null) return;

            lock (dynamicsWorld)
            {
                dynamicsWorld.RemoveCollisionObject(collisionObject);

                collisionObjects.Remove(collisionObject);
            }

        }

        public static void AddToHitboxWorld(RigidBody body)
        {
            lock (hitboxWorld)
            {
                hitboxWorld.AddRigidBody(body);
            }
        }

        public static void AddToDynamicWorld(RigidBody body)
        {

            lock (dynamicsWorld)
            {
                dynamicsWorld.AddRigidBody(body);
            }

            lock(collisionObjects)
                collisionObjects.Add(body);
        }

        public static void RemoveFromHitboxWorld(RigidBody body)
        {
            lock (hitboxWorld)
            {
                hitboxWorld.RemoveRigidBody(body);
                lock (hitboxWorld.CollisionObjectArray)
                    hitboxWorld.CollisionObjectArray.Remove(body);
            }
        }

        public static void Remove(RigidBody body)
        {
            if (body is null) return;

            lock (dynamicsWorld) lock (dynamicsWorld.CollisionObjectArray)
                {
                    dynamicsWorld.RemoveRigidBody(body);

                    //dynamicsWorld.RemoveCollisionObject(body);
                }

        }


        public static void Remove(TypedConstraint constraint)
        {

            if(constraint is null) return;

            lock (constraint)
            {
                lock (dynamicsWorld)
                {

                    if (constraint == null) return;

                    dynamicsWorld.RemoveConstraint(constraint);
                }
            }

        }

        static long oldSimTick = -1;

        public static void Update()
        {

            lock (dynamicsWorld)
            {
                var collisionObjects = dynamicsWorld.CollisionObjectArray.ToArray();

                Parallel.For(0, collisionObjects.Length, i =>
                {
                    CollisionObject colObj = null;
                    try
                    {
                        colObj = collisionObjects[i];
                    }
                    catch (Exception ex) { Logger.Log(ex); }



                    // Check if the collision object is a rigid body
                    if (colObj is RigidBody rigidBody)
                    {
                        if (colObj.UserObject is null)
                        {
                            lock (dynamicsWorld)
                            {
                                dynamicsWorld.RemoveRigidBody((RigidBody)colObj);
                            }
                            return;
                        }


                        BodyType bodyType = colObj.GetBodyType();

                        switch (bodyType)
                        {
                            case BodyType.World:
                            case BodyType.MainBody:
                                break;

                            case BodyType.HitBox:
                                return;
                                break;
                        }
                        if (colObj.IsActive == false) return;
                        Entity ent = ((RigidbodyData)colObj.UserObject).Entity;

                        Vector3 pos = colObj.WorldTransform.Translation;

                        // Assume this value is set based on your fixed physics update rate
                        float fixedDeltaTime = Math.Max(1 / UpdateRate, Time.DeltaTime);

                        if (oldSimTick != SimulationTicks)
                        {
                            ent.PhysicalVelocity = rigidBody.LinearVelocity;
                        }

                        if (ent.DisablePhysicsInterpolation == false)
                        {
                            // Store the previous and current positions of the entity
                            Vector3 previousPosition = ent.Position.ToPhysics(); // This should be updated each physics tick
                            Vector3 currentPosition = pos; // This is updated during the physics update

                            if (previousPosition != currentPosition)
                            {
                                // Interpolate the position based on the elapsed time in the current frame
                                float interpolationFactor = Time.DeltaTime / fixedDeltaTime;
                                ent.Position = Vector3.Lerp(previousPosition, currentPosition, Math.Min(interpolationFactor,1));
                            }
                        }

                        

                        else
                        {
                            ent.Position = pos;
                        }

                        Matrix4x4 rotationMatrix = rigidBody.WorldTransform.GetBasis();
                        Quaternion rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);


                        

                        Vector3 rotationEulerAngles = ((Microsoft.Xna.Framework.Quaternion)rotation).ToYawPitchRoll().ToPhysics();



                        ent.Rotation = new Microsoft.Xna.Framework.Vector3((float)rotationEulerAngles.X, (float)rotationEulerAngles.Y, (float)rotationEulerAngles.Z);
                    }
                });
            }

            oldSimTick = SimulationTicks;

        }

        /*
         public static void Update()
{
    lock (dynamicsWorld)
    {
        for (int i = 0; i < dynamicsWorld.CollisionObjectArray.Count; i++)
        {
            CollisionObject colObj = null;
            try
            {
                colObj = dynamicsWorld.CollisionObjectArray[i];

            } catch(Exception ex) { Console.WriteLine(ex); }
            // Check if the collision object is a rigid body
            if (colObj is RigidBody rigidBody)
            {
                if (colObj.UserObject is null)
                {
                    dynamicsWorld.RemoveRigidBody((RigidBody)colObj);
                    continue;
                }
                if (colObj.UserIndex2 == -1)
                    colObj.SetBodyType(BodyType.MainBody);

                if (colObj.UserIndex == -1)
                    colObj.SetCollisionMask(BodyType.GroupCollisionTest);


                BodyType bodyType = colObj.GetBodyType();



                switch (bodyType)
                {
                    case BodyType.World:
                    case BodyType.MainBody:


                        break;

                    case BodyType.HitBox:
                        continue;
                        break;


                }
                if (colObj.IsActive == false) continue;
                Entity ent = (Entity)colObj.UserObject;

                Vector3 pos = colObj.WorldTransform.Translation;

                // Assume this value is set based on your fixed physics update rate
                float fixedDeltaTime = Math.Max(1 / 50f,Time.DeltaTime);

                if (ent.DisablePhysicsInterpolation == false)
                {

                    // Store the previous and current positions of the entity
                    Vector3 previousPosition = ent.Position.ToPhysics(); // This should be updated each physics tick
                    Vector3 currentPosition = pos; // This is updated during the physics update

                    if (previousPosition == currentPosition)
                    {
                    }
                    else
                    {

                        // Interpolate the position based on the elapsed time in the current frame
                        float interpolationFactor = Time.DeltaTime / fixedDeltaTime;
                        ent.Position = Vector3.Lerp(previousPosition, currentPosition, interpolationFactor);
                    }
                }else
                {
                    ent.Position = pos;
                }

                Matrix4x4 rotationMatrix = rigidBody.WorldTransform.GetBasis();
                Quaternion rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);

                Vector3 rotationEulerAngles = ToEulerAngles(rotation);

                ent.Rotation = new Microsoft.Xna.Framework.Vector3((float)rotationEulerAngles.X, (float)rotationEulerAngles.Y, (float)rotationEulerAngles.Z);

            }
        }
    }
}
         */ // old not parallel version



        public static Vector3 ToEulerAngles(Quaternion quaternion)
        {
            Vector3 angles = new Vector3();
            double sinr_cosp = 2.0 * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            double cosr_cosp = 1.0 - 2.0 * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            double sinp = 2.0 * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp); // Use 90 degrees if out of range
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            double siny_cosp = 2.0 * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            double cosy_cosp = 1.0 - 2.0 * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles / (float)Math.PI * 180;
        }

        public static Quaternion ToQuaternion(Vector3 eulerAngles)
        {
            eulerAngles *= (float)Math.PI / 180.0f; // Convert degrees to radians

            float cosX = (float)Math.Cos(eulerAngles.X / 2.0f);
            float sinX = (float)Math.Sin(eulerAngles.X / 2.0f);
            float cosY = (float)Math.Cos(eulerAngles.Y / 2.0f);
            float sinY = (float)Math.Sin(eulerAngles.Y / 2.0f);
            float cosZ = (float)Math.Cos(eulerAngles.Z / 2.0f);
            float sinZ = (float)Math.Sin(eulerAngles.Z / 2.0f);

            float x = sinX * cosY * cosZ + cosX * sinY * sinZ;
            float y = cosX * sinY * cosZ - sinX * cosY * sinZ;
            float z = cosX * cosY * sinZ - sinX * sinY * cosZ;
            float w = cosX * cosY * cosZ + sinX * sinY * sinZ;

            return new Quaternion(x, y, z, w);
        }

        public static RigidBody CreateSphere(Entity entity, float mass = 10, float radius = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var sphereShape = new SphereShape(radius);
            sphereShape.UserObject = new CollisionSurfaceData();
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            Vector3 inertia = Vector3.Zero;
            sphereShape.CalculateLocalInertia(mass, out inertia);

            // Create a rigid body for the sphere
            var sphereRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, sphereShape, inertia);
            RigidBody = new RigidBody(sphereRigidBodyInfo);

            RigidBody.SetBodyType(BodyType.MainBody);
            RigidBody.SetCollisionMask(BodyType.GroupCollisionTest);

            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = new RigidbodyData(entity);

            lock(dynamicsWorld)
                dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 0.5f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.5f;

            collisionObjects.Add(RigidBody);

            return RigidBody;
        }

        public static RigidBody CreateBox(Entity entity, Vector3 size, float mass = 10, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;


            // Create a sphere shape
            var sphereShape = new BoxShape(size / 2);
            sphereShape.UserObject = new CollisionSurfaceData();
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            //sphereShape.Margin = 0;

            Vector3 inertia = Vector3.Zero;
            sphereShape.CalculateLocalInertia(mass, out inertia);

            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, sphereShape, inertia);
            RigidBody = new RigidBody(boxRigidBodyInfo);

            RigidBody.SetBodyType(BodyType.MainBody);
            RigidBody.SetCollisionMask(BodyType.GroupCollisionTest);

            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = new RigidbodyData(entity);

            lock(dynamicsWorld)
            dynamicsWorld.AddRigidBody(RigidBody);


            RigidBody.Friction = 1f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.1f;

            collisionObjects.Add(RigidBody);

            return RigidBody;
        }


        public static RigidBody CreateFromShape(Entity entity, Vector3 size, CollisionShape shape, float mass = 10, CollisionFlags collisionFlags = CollisionFlags.None, BodyType bodyType = BodyType.MainBody, bool addToWorld = true)
        {
            RigidBody RigidBody;


            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            Vector3 inertia = Vector3.Zero;
            shape.CalculateLocalInertia(mass, out inertia);
            shape.LocalScaling = size;

            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, shape, inertia);
            RigidBody = new RigidBody(boxRigidBodyInfo);

            RigidBody.SetBodyType(BodyType.MainBody);
            RigidBody.SetCollisionMask(BodyType.GroupCollisionTest);

            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = new RigidbodyData(entity);

            if (addToWorld)
            {

                lock (dynamicsWorld)
                    dynamicsWorld.AddRigidBody(RigidBody);

                collisionObjects.Add(RigidBody);

            }

            if (collisionFlags == CollisionFlags.StaticObject && entity.Static)
            {

                var staticBody = new RetroEngine.StaticRigidBody(boxRigidBodyInfo);
                staticBody.CollisionFlags = collisionFlags;

                staticBody.SetParrent(RigidBody);

                staticBodies.Add(staticBody);
                lock (staticWorld)
                {
                    staticWorld.AddRigidBody(staticBody);
                }
            }

            RigidBody.SetBodyType(bodyType);
            RigidBody.SetCollisionMask(BodyType.GroupCollisionTest);

            RigidBody.Friction = 1f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.1f;

            RigidBody.Restitution = 0;



            return RigidBody;
        }


        public static RigidBody CreateCharacterCapsule(Entity entity, float Height, float radius, float mass = 80, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var Shape = new CapsuleShape(radius, Height - radius*2);
            Shape.Margin = 0;

            Shape.UserObject = new CollisionSurfaceData();
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));


            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, Shape);
            RigidBody = new RigidBody(boxRigidBodyInfo);

            RigidBody.SetBodyType(BodyType.MainBody);
            RigidBody.SetCollisionMask(BodyType.GroupCollisionTest);

            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = new RigidbodyData(entity);

            RigidBody.SetCollisionMask(BodyType.GroupCollisionTest);
            RigidBody.SetBodyType(BodyType.CharacterCapsule);

            lock (dynamicsWorld)
            dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 0f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0f;
            RigidBody.CollisionShape = Shape;

            RigidBody.ActivationState = ActivationState.DisableDeactivation;


            collisionObjects.Add(RigidBody);

            return RigidBody;
        }

        public static RigidBody CreateCapsule(Entity entity, float Height, float radius, float mass = 10, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var Shape = new CapsuleShape(radius, Height - radius * 2);
            Shape.Margin = 0;

            Shape.UserObject = new CollisionSurfaceData();
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));


            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, Shape);
            RigidBody = new RigidBody(boxRigidBodyInfo);

            RigidBody.SetBodyType(BodyType.MainBody);
            RigidBody.SetCollisionMask(BodyType.GroupCollisionTest);

            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = new RigidbodyData(entity);

            RigidBody.SetCollisionMask(BodyType.GroupCollisionTest);
            RigidBody.SetBodyType(BodyType.MainBody);


            lock (dynamicsWorld)
                dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Restitution = 0f;
            RigidBody.CollisionShape = Shape;

            collisionObjects.Add(RigidBody);

            return RigidBody;
        }

        static MyClosestRayResultCallback NewClosestRayResultCallback(ref Vector3 rayFromWorld, ref Vector3 rayToWorld)
        {


            lock (ClosestRayResultCallbacks)
            {
                if(ClosestRayResultCallbacks.Count > 0)
                {
                    MyClosestRayResultCallback callback = ClosestRayResultCallbacks.Pop();
                    callback.RayFromWorld = rayFromWorld;
                    callback.RayToWorld = rayToWorld;
                    return callback;

                }
            }

            return new MyClosestRayResultCallback(ref rayFromWorld,ref rayToWorld);

        }

        static MyClosestConvexResultCallback NewClosestConvexResultCallback(ref Vector3 rayFromWorld, ref Vector3 rayToWorld)
        {
            lock (convexResultCallbacks)
            {
                if (convexResultCallbacks.Count > 0)
                {
                    MyClosestConvexResultCallback callback = convexResultCallbacks.Pop();
                    callback.ConvexFromWorld = rayFromWorld;
                    callback.ConvexFromWorld = rayToWorld;
                    callback.Start = rayFromWorld;
                    callback.End = rayToWorld;
                    callback.DisableCustomChecks = false;
                    return callback;

                }
            }

            return new MyClosestConvexResultCallback(ref rayFromWorld, ref rayToWorld);
        }

        public static MyClosestRayResultCallback LineTrace(Microsoft.Xna.Framework.Vector3 rayStart, Microsoft.Xna.Framework.Vector3 rayEnd, List<CollisionObject> ignoreList = null, BodyType bodyType = BodyType.GroupAll)
        {

            Vector3 start = rayStart.ToPhysics();
            Vector3 end = rayEnd.ToPhysics();

            MyClosestRayResultCallback rayCallback = NewClosestRayResultCallback(ref start, ref end);
            rayCallback.BodyTypeMask = bodyType;
            if (ignoreList is not null)
                rayCallback.ignoreList = ignoreList;

            lock (dynamicsWorld)
            {
                CollisionWorld world = dynamicsWorld;

                // Perform the ray cast
                world.RayTest(start, end, rayCallback);


                if (bodyType.HasFlag(BodyType.HitBox))
                {

                    MyClosestRayResultCallback rayCallback2 = NewClosestRayResultCallback(ref start, ref end);
                    rayCallback2.BodyTypeMask = bodyType;
                    if (ignoreList is not null)
                        rayCallback2.ignoreList = ignoreList;

                    lock (hitboxWorld)
                        hitboxWorld.RayTest(start, end, rayCallback2);

                    float distHitbox = Vector3.Distance(start, rayCallback2.HitPointWorld);
                    float distWorld = Vector3.Distance(start, rayCallback.HitPointWorld);

                    if (rayCallback2.HasHit)
                        if (distHitbox < distWorld)
                        {
                            return rayCallback2;
                        }

                }
                

                return rayCallback;
            }
        }

        public static MyAllRayResultCallback MultiLineTrace(Microsoft.Xna.Framework.Vector3 rayStart, Microsoft.Xna.Framework.Vector3 rayEnd, List<CollisionObject> ignoreList = null, BodyType bodyType = BodyType.GroupAll)
        {
            lock (dynamicsWorld)
            {

                Vector3 start = rayStart.ToPhysics();
                Vector3 end = rayEnd.ToPhysics();

                CollisionWorld world = dynamicsWorld;

                MyAllRayResultCallback rayCallback = new MyAllRayResultCallback(start, end);
                rayCallback.BodyTypeMask = bodyType;
                if (ignoreList is not null)
                    rayCallback.ignoreList = ignoreList;


                // Perform the ray cast
                world.RayTest(start, end, rayCallback);

                if (bodyType.HasFlag(BodyType.HitBox))
                {

                    MyAllRayResultCallback rayCallback2 = new MyAllRayResultCallback(start, end);
                    rayCallback2.BodyTypeMask = bodyType;
                    if (ignoreList is not null)
                        rayCallback2.ignoreList = ignoreList;


                    // Perform the ray cast
                    lock(hitboxWorld)
                        hitboxWorld.RayTest(start, end, rayCallback2);

                    rayCallback.Hits.AddRange(rayCallback2.Hits);


                }

                rayCallback.Hits = rayCallback.Hits.OrderBy(h => h.ClosestHitFraction).ToList();

                return rayCallback;
            }
        }

        public static MyClosestConvexResultCallback SphereTraceForStatic(Vector3 rayStart, Vector3 rayEnd, float radius = 0.5f)
        {


            CollisionWorld world = staticWorld;

            // Create a sphere shape with the specified radius
            SphereShape sphereShape = new SphereShape(radius);

            Matrix4x4 start = Matrix4x4.CreateTranslation(rayStart);
            Matrix4x4 end = Matrix4x4.CreateTranslation(rayEnd);


            MyClosestConvexResultCallback convResultCallback = NewClosestConvexResultCallback(ref rayStart, ref rayEnd);

            convResultCallback.DisableCustomChecks = true;

            // Set the callback to respond only to static objects
            convResultCallback.FlagToRespond = CollisionFlags.StaticObject;
            lock (staticWorld)
            {
                world.ConvexSweepTest(sphereShape, start, end, convResultCallback);
            }

            return convResultCallback;
        }

        public static MyClosestConvexResultCallback SphereTrace(Microsoft.Xna.Framework.Vector3 rayStart, Microsoft.Xna.Framework.Vector3 rayEnd, float radius = 0.5f, List<RigidBody> ignoreList = null, BodyType bodyType = BodyType.GroupAll)
        {

            if (Thread.CurrentThread != GameMain.RenderThread && Thread.CurrentThread != GameMain.GameThread)
            {

            }
            lock (dynamicsWorld)
            {
                CollisionWorld world = dynamicsWorld;

                // Create a sphere shape with the specified radius
                SphereShape sphereShape = new SphereShape(radius);

                Vector3 rStart = rayStart.ToPhysics();
                Vector3 rEnd = rayEnd.ToPhysics();

                Matrix4x4 start = Matrix4x4.CreateTranslation(rayStart.ToPhysics());
                Matrix4x4 end = Matrix4x4.CreateTranslation(rayEnd.ToPhysics());


                MyClosestConvexResultCallback convResultCallback = NewClosestConvexResultCallback(ref rStart, ref rEnd);
                convResultCallback.BodyTypeMask = bodyType;

                if (ignoreList is not null)
                    convResultCallback.ignoreList = ignoreList;


                // Perform the sphere sweep
                world.ConvexSweepTest(sphereShape, start, end, convResultCallback);

                if (bodyType.HasFlag(BodyType.HitBox))
                {

                    MyClosestConvexResultCallback convResultCallback2 = NewClosestConvexResultCallback(ref rStart, ref rEnd);
                    convResultCallback2.BodyTypeMask = bodyType;

                    if (ignoreList is not null)
                        convResultCallback2.ignoreList = ignoreList;


                    // Perform the sphere sweep
                    lock(hitboxWorld)
                    hitboxWorld.ConvexSweepTest(sphereShape, start, end, convResultCallback2);

                    float distHitbox = Vector3.Distance(rayStart.ToPhysics(), convResultCallback2.HitPointWorld);
                    float distWorld = Vector3.Distance(rayStart.ToPhysics(), convResultCallback.HitPointWorld);

                    if (convResultCallback2.HasHit)
                        if (distHitbox < distWorld)
                        {
                            return convResultCallback2;
                        }

                }

                return convResultCallback;
            }

           
        }

        public static MyClosestRayResultCallback LineTraceForStatic(Vector3 rayStart, Vector3 rayEnd)
        {
            CollisionWorld world = staticWorld;

            MyClosestRayResultCallback rayCallback = NewClosestRayResultCallback(ref rayStart, ref rayEnd);

            rayCallback.DisableCustomChecks = true;

            rayCallback.FlagToRespond = CollisionFlags.StaticObject;

            lock (staticWorld)
            {
                // Perform the ray cast
                world.RayTest(rayStart, rayEnd, rayCallback);
            }

            return rayCallback;

        }


        public static CollisionShape CreateCollisionShapeFromModel(Model model, float scale = 1.0f, CollisionSurfaceData shapeData = null, bool complex = false)
        {

            if (shapeData == null)
                shapeData = new CollisionSurfaceData();

            // Create a compound shape to hold multiple child collision shapes
            CompoundShape compoundShape = new CompoundShape();
            compoundShape.UserObject = shapeData;

            // Loop through the model's meshes
            foreach (ModelMesh mesh in model.Meshes)
            {
                for (int i = 0; i < mesh.MeshParts.Count; i++)
                {
                    // Create a convex hull shape for each mesh
                    CollisionShape Shape;
                    if (complex)
                        Shape = CreateBvhTriangleMeshShape(mesh, scale, i);
                    else
                        Shape = CreateConvexHullShape(mesh, scale, i);

                    Shape.UserObject = shapeData;




                    // Calculate the mesh's transformation matrix (position and rotation)
                    Matrix4x4 transform = Matrix4x4.Identity;

                    // Add the convex shape to the compound shape with the calculated transformation
                    compoundShape.AddChildShape(transform, Shape);
                }
            }

            return compoundShape;
        }

        /*
        public static CollisionShape CreateCollisionShapeFromStaticMesh(StaticMesh staticMesh, float scale = 1.0f, CollisionShapeData shapeData = null, bool complex = false)
        {

            if (shapeData == null)
                shapeData = new CollisionShapeData();

            // Create a compound shape to hold multiple child collision shapes
            CompoundShape compoundShape = new CompoundShape();
            compoundShape.UserObject = shapeData;


            // Create a convex hull shape for each mesh
            IndexedMesh mesh = new IndexedMesh();


            var meshes = staticMesh.GetMeshData();
            Recast.MergeMeshes(meshes, out var verts, out var faces);

            mesh.Allocate(faces.Length, verts.Length);
            mesh.SetData(faces, verts);

            TriangleMesh triangleMesh = new TriangleMesh();

            triangleMesh.AddIndexedMesh(mesh);


            // 6. Create the collision shape
            BvhTriangleMeshShape Shape = new BvhTriangleMeshShape(triangleMesh, true); // Use quantization for better performance

            

            Shape.UserObject = shapeData;




            // Calculate the mesh's transformation matrix (position and rotation)
            Matrix4x4 transform = Matrix4x4.Identity;

            // Add the convex shape to the compound shape with the calculated transformation
            compoundShape.AddChildShape(transform, Shape);



            return compoundShape;
        }
        */
        public static ConvexHullShape CreateConvexHullShape(ModelMesh mesh, float scale, int part = 0)
        {
            // Get the vertices from the model's mesh part
            VertexData[] vertices = new VertexData[mesh.MeshParts[part].VertexBuffer.VertexCount];
            mesh.MeshParts[part].VertexBuffer.GetData(vertices);

            // Extract the positions and scale them if necessary
            Vector3[] positions = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                positions[i] = vertices[i].Position.ToPhysics() * scale;
            }

            // Create a convex hull shape from the positions
            ConvexHullShape shape = new ConvexHullShape(positions);

            return shape;
        }

        public static BvhTriangleMeshShape CreateBvhTriangleMeshShape(ModelMesh mesh, float scale, int part = 0)
        {
            // 1. Gather vertices in a contiguous array
            VertexData[] vertices = new VertexData[mesh.MeshParts[part].VertexBuffer.VertexCount];
            mesh.MeshParts[part].VertexBuffer.GetData(vertices);


            // 3. Access indices for correct triangle construction
            // Assuming indices are 32-bit integers
            int[] indices = new int[mesh.MeshParts[part].IndexBuffer.IndexCount];
            mesh.MeshParts[part].IndexBuffer.GetData(indices);

            // 4. Create the triangle mesh
            TriangleMesh triangleMesh = new TriangleMesh();


            for (int i = 0; i < indices.Length; i += 3)
            {
                var vertex0 = vertices[indices[i]].Position.ToPhysics();
                var vertex1 = vertices[indices[i + 1]].Position.ToPhysics();
                var vertex2 = vertices[indices[i + 2]].Position.ToPhysics();
                triangleMesh.AddTriangle(vertex0, vertex1, vertex2, false);  // Assume triangles are not welded
            }


            // 6. Create the collision shape
            BvhTriangleMeshShape meshShape = new BvhTriangleMeshShape(triangleMesh, true); // Use quantization for better performance

            return meshShape;
        }

        public static BvhTriangleMeshShape CreateBvhTriangleMeshShape(Csg.Solid solid, CollisionSurfaceData collisionShapeData)
        {
            var data = Csg.CsgHelper.ConvertCsgToMesh(solid);

            // 1. Gather vertices in a contiguous array
            VertexData[] vertices = data.Vertices;


            // 3. Access indices for correct triangle construction
            // Assuming indices are 32-bit integers
            int[] indices = data.Indices;

            // 4. Create the triangle mesh
            TriangleMesh triangleMesh = new TriangleMesh();


            for (int i = 0; i < indices.Length; i += 3)
            {
                var vertex0 = vertices[indices[i]].Position.ToPhysics();
                var vertex1 = vertices[indices[i + 1]].Position.ToPhysics();
                var vertex2 = vertices[indices[i + 2]].Position.ToPhysics();
                triangleMesh.AddTriangle(vertex0, vertex1, vertex2, false);  // Assume triangles are not welded
            }


            // 6. Create the collision shape
            BvhTriangleMeshShape meshShape = new BvhTriangleMeshShape(triangleMesh, true); // Use quantization for better performance

            meshShape.UserObject = collisionShapeData;

            return meshShape;
        }

    }
}
