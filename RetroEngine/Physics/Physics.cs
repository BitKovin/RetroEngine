using BulletSharp;
using BulletSharp.Math;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Physics
{
    public class Physics
    {

        private static DiscreteDynamicsWorld dynamicsWorld;

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
            dynamicsWorld.Gravity = new BulletSharp.Math.Vector3(0, -9.81f, 0); // Set gravity
        }

        public static void Update()
        {
            dynamicsWorld.StepSimulation(Engine.Time.deltaTime);

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

                    Vector3 pos = colObj.WorldTransform.Origin;

                    ent.Position = new Microsoft.Xna.Framework.Vector3((float)pos.X, (float)pos.Y, (float)pos.Z);

                    Matrix rotationMatrix = rigidBody.WorldTransform.Basis;
                    Quaternion rotation = Quaternion.RotationMatrix(rotationMatrix);

                    Vector3 rotationEulerAngles = ToEulerAngles(rotation);

                    ent.Rotation = new Microsoft.Xna.Framework.Vector3((float)rotationEulerAngles.X, (float)rotationEulerAngles.Y, (float)rotationEulerAngles.Z);

                }
            }

        }

        static Vector3 ToEulerAngles(Quaternion quaternion)
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


        public static RigidBody CreateSphere(Entity entity, float mass = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var sphereShape = new SphereShape(1.0f);
            var motionState = new DefaultMotionState(Matrix.Translation(0, 10, 0));

            Vector3 inertia = Vector3.Zero;
            sphereShape.CalculateLocalInertia(mass, out inertia);

            // Create a rigid body for the sphere
            var sphereRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, sphereShape);
            RigidBody = new RigidBody(sphereRigidBodyInfo);
            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 0.5f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.5f;

            return RigidBody;
        }

        public static RigidBody CreateBox(Entity entity,Vector3 size,float mass = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var sphereShape = new BoxShape(size);
            var motionState = new DefaultMotionState(Matrix.Translation(0, 0, 0));

            Vector3 inertia = Vector3.Zero;
            sphereShape.CalculateLocalInertia(mass, out inertia);

            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags==CollisionFlags.StaticObject? 0:mass, motionState, sphereShape, inertia);
            RigidBody = new RigidBody(boxRigidBodyInfo);
            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 1f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.1f;

            return RigidBody;
        }

    }
}
