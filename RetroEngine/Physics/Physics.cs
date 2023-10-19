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

                    Vector3 rotationEulerAngles;

                    rotationEulerAngles.X = (float)Math.Atan2(rotationMatrix.M32, rotationMatrix.M33); // Pitch
                    rotationEulerAngles.Y = (float)Math.Atan2(-rotationMatrix.M31, Math.Sqrt(rotationMatrix.M32 * rotationMatrix.M32 + rotationMatrix.M33 * rotationMatrix.M33)); // Yaw
                    rotationEulerAngles.Z = (float)Math.Atan2(rotationMatrix.M21, rotationMatrix.M11); // Roll

                    ent.Rotation = new Microsoft.Xna.Framework.Vector3((float)rotationEulerAngles.X, (float)rotationEulerAngles.Y, (float)rotationEulerAngles.Z);

                }
            }

        }

        public static RigidBody CreateSphere(Entity entity, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody sphereRigidBody;

            // Create a sphere shape
            var sphereShape = new SphereShape(1.0f);
            var motionState = new DefaultMotionState(Matrix.Translation(0, 10, 0));

            // Create a rigid body for the sphere
            var sphereRigidBodyInfo = new RigidBodyConstructionInfo(1.0f, motionState, sphereShape);
            sphereRigidBody = new RigidBody(sphereRigidBodyInfo);
            sphereRigidBody.CollisionFlags = collisionFlags;

            sphereRigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(sphereRigidBody);

            return sphereRigidBody;
        }

        public static RigidBody CreateBox(Entity entity, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody boxRigidBody;

            // Create a sphere shape
            var sphereShape = new BoxShape(new Vector3(1,1,1));
            var motionState = new DefaultMotionState(Matrix.Translation(0, 10, 0));

            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(1.0f, motionState, sphereShape);
            boxRigidBody = new RigidBody(boxRigidBodyInfo);
            boxRigidBody.CollisionFlags = collisionFlags;

            boxRigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(boxRigidBody);

            return boxRigidBody;
        }

    }
}
