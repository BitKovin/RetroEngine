using BulletSharp;
using RetroEngine;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace RetroEngine
{
    public class Physics
    {

        private static DiscreteDynamicsWorld dynamicsWorld;

        private static int steps = 2;

        public static void Start()
        {
            // Create a collision configuration and dispatcher
            var collisionConfig = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(collisionConfig);
            // Create a broadphase and a solver
            var broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();

            // Create the dynamics world
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
            dynamicsWorld.Gravity = new Vector3(0, -9.81f, 0); // Set gravity
            dynamicsWorld.DispatchInfo.UseContinuous = true;

        }

        public static void PerformContactCheck(CollisionObject collisionObject, CollisionCallback callback) 
        {
        
            dynamicsWorld.ContactTest(collisionObject, callback);

        }

        public static void Simulate()
        {
            if (GameMain.inst.paused == false)
                dynamicsWorld.StepSimulation(RetroEngine.Time.deltaTime, steps, Math.Max(1f / 60f, Time.deltaTime));
        }

        public static void Remove(CollisionObject collisionObject)
        {
            dynamicsWorld.RemoveCollisionObject(collisionObject);
            collisionObject.Dispose();
        }

        public static void Update()
        {
            Simulate();

            for (int i = 0; i < dynamicsWorld.NumCollisionObjects; i++)
            {
                CollisionObject colObj = dynamicsWorld.CollisionObjectArray[i];

                // Check if the collision object is a rigid body
                if (colObj is RigidBody rigidBody)
                {
                    if (colObj.UserObject is null)
                    {
                        dynamicsWorld.RemoveRigidBody((RigidBody)colObj);
                        continue;
                    }

                    Entity ent = (Entity)colObj.UserObject;

                    Vector3 pos = colObj.WorldTransform.Translation;

                    ent.Position = new Microsoft.Xna.Framework.Vector3((float)pos.X, (float)pos.Y, (float)pos.Z);

                    Matrix4x4 rotationMatrix = rigidBody.WorldTransform.GetBasis();
                    Quaternion rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);

                    Vector3 rotationEulerAngles = ToEulerAngles(rotation);

                    ent.Rotation = new Microsoft.Xna.Framework.Vector3((float)rotationEulerAngles.X, (float)rotationEulerAngles.Y, (float)rotationEulerAngles.Z);

                }
            }

        }

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

        public static RigidBody CreateSphere(Entity entity, float mass = 1, float radius = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var sphereShape = new SphereShape(radius);
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            Vector3 inertia = Vector3.Zero;
            sphereShape.CalculateLocalInertia(mass, out inertia);

            // Create a rigid body for the sphere
            var sphereRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, sphereShape, inertia);
            RigidBody = new RigidBody(sphereRigidBodyInfo);
            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 0.5f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.5f;

            return RigidBody;
        }

        public static RigidBody CreateBox(Entity entity, Vector3 size, float mass = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var sphereShape = new BoxShape(size/2);
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            Vector3 inertia = Vector3.Zero;
            sphereShape.CalculateLocalInertia(mass, out inertia);

            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, sphereShape, inertia);
            RigidBody = new RigidBody(boxRigidBodyInfo);
            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 1f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.1f;

            return RigidBody;
        }


        public static RigidBody CreateFromShape(Entity entity, Vector3 size,CollisionShape shape, float mass = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;


            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            Vector3 inertia = Vector3.Zero;
            shape.CalculateLocalInertia(mass, out inertia);

            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, shape, inertia);
            RigidBody = new RigidBody(boxRigidBodyInfo);
            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 1f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.1f;

            return RigidBody;
        }


        public static RigidBody CreateCharacterCapsule(Entity entity, float HalfHeight,float radius, float mass = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var Shape = new CapsuleShape(radius,HalfHeight);
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            Shape.Margin = 0f;

            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, Shape);
            RigidBody = new RigidBody(boxRigidBodyInfo);
            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 0f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0f;
            

            Matrix4x4 fixedRotation = Matrix4x4.Identity;

            return RigidBody;
        }

        public static ClosestRayResultCallback LineTrace(Vector3 rayStart, Vector3 rayEnd)
        {
            CollisionWorld world = dynamicsWorld as CollisionWorld;

            ClosestRayResultCallback rayCallback = new ClosestRayResultCallback(ref rayStart, ref rayEnd);

            // Perform the ray cast
            world.RayTest(rayStart, rayEnd, rayCallback);

            // Check if the ray hit something
            if (rayCallback.HasHit)
            {
                // Access information about the hit object
                RigidBody hitRigidBody = RigidBody.Upcast(rayCallback.CollisionObject);
                Vector3 hitPoint = rayCallback.HitPointWorld;
                Vector3 hitNormal = rayCallback.HitNormalWorld;

                // Now, you can use 'hitRigidBody', 'hitPoint', and 'hitNormal' for further processing
            }

            return rayCallback;


        }

        public static CollisionShape CreateCollisionShapeFromModel(Model model, float scale = 1.0f)
        {
            // Create a compound shape to hold multiple child collision shapes
            CompoundShape compoundShape = new CompoundShape();

            // Loop through the model's meshes
            foreach (ModelMesh mesh in model.Meshes)
            {
                // Create a convex hull shape for each mesh
                ConvexHullShape convexShape = CreateConvexHullShape(mesh, scale);

                Matrix4x4 scaling = Matrix4x4.CreateScale(scale);



                // Calculate the mesh's transformation matrix (position and rotation)
                Matrix4x4 transform = Matrix4x4.Identity;

                // Add the convex shape to the compound shape with the calculated transformation
                compoundShape.AddChildShape(transform, convexShape);
            }

            return compoundShape;
        }

        public static ConvexHullShape CreateConvexHullShape(ModelMesh mesh, float scale)
        {
            // Get the vertices from the model's mesh part
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[mesh.MeshParts[0].VertexBuffer.VertexCount];
            mesh.MeshParts[0].VertexBuffer.GetData(vertices);

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

    }
}
