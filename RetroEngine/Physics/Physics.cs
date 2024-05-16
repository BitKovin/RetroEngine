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

        private static DiscreteDynamicsWorld staticWorld;
        private static List<StaticRigidBody> staticBodies = new List<StaticRigidBody>();

        private static int steps = 1;

        static List<CollisionObject> removeList = new List<CollisionObject>();

        static List<CollisionObject> collisionObjects = new List<CollisionObject>();



        static SequentialImpulseConstraintSolver solver;
        static DbvtBroadphase broadphase;
        static CollisionDispatcher dispatcher;

        public class CollisionShapeData
        {
            public string surfaceType = "default";
        }

        public static void DebugDraw()
        {
            dynamicsWorld.DebugDrawWorld();
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

            // Create a collision configuration and dispatcher
            var collisionConfig = new DefaultCollisionConfiguration();
            collisionConfig.SetConvexConvexMultipointIterations(1, 1);
            collisionConfig.SetPlaneConvexMultipointIterations(1, 1);
            dispatcher = new CollisionDispatcher(collisionConfig);

            // Create a broadphase and a solver
            broadphase = new DbvtBroadphase();

            solver = new SequentialImpulseConstraintSolver();


            // Create the dynamics world
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
            dynamicsWorld.Gravity = new Vector3(0, -9.81f, 0); // Set gravity
            dynamicsWorld.DispatchInfo.UseContinuous = true;


            staticWorld = new DiscreteDynamicsWorld(new CollisionDispatcher(new DefaultCollisionConfiguration()), new DbvtBroadphase(), new SequentialImpulseConstraintSolver(), new DefaultCollisionConfiguration());
            staticBodies.Clear();

            dynamicsWorld.DebugDrawer = new DebugDrawer(GameMain.Instance.GraphicsDevice);
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

            foreach (CollisionObject collisionObject in collisionObjects)
            {

                RigidBody body = RigidBody.Upcast(collisionObject);

                if (body == null)
                    Remove(collisionObject);
                else
                    Remove(body);
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

        public static void Simulate()
        {
            lock (staticWorld)
            {
                foreach (StaticRigidBody staticRigidBody in staticBodies)
                {
                    staticRigidBody.UpdateFromParrent();
                }
            }
            lock (dynamicsWorld)
            {
                if (GameMain.Instance.paused == false)
                    dynamicsWorld.StepSimulation(Time.DeltaTime * Time.TimeScale, steps, Math.Max(1 / 30f, Time.DeltaTime));
            }
        }

        public static void Remove(CollisionObject collisionObject)
        {
            if (collisionObject is null) return;

            lock (dynamicsWorld)
            {
                dynamicsWorld.RemoveCollisionObject(collisionObject);
            }
            collisionObjects.Remove(collisionObject);
            collisionObject.UserObject = null;
            collisionObject.CollisionShape.Dispose();
            collisionObject.Dispose();

        }

        public static void Remove(RigidBody body)
        {
            if (body is null) return;

            lock (dynamicsWorld)
            {
                dynamicsWorld.RemoveRigidBody(body);
                dynamicsWorld.CollisionObjectArray.Remove(body);
            }
            collisionObjects.Remove(body);
            body.ClearForces();


            body.UserObject = null;
            body.MotionState.Dispose();
            body.CollisionShape.Dispose();

            body.Dispose();

        }

        public static void Update()
        {

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

                    if (colObj.IsActive == false) continue;

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
            sphereShape.UserObject = new CollisionShapeData();
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

            collisionObjects.Add(RigidBody);

            return RigidBody;
        }

        public static RigidBody CreateBox(Entity entity, Vector3 size, float mass = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;


            // Create a sphere shape
            var sphereShape = new BoxShape(size / 2);
            sphereShape.UserObject = new CollisionShapeData();
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            sphereShape.Margin = 0;

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

            collisionObjects.Add(RigidBody);

            return RigidBody;
        }


        public static RigidBody CreateFromShape(Entity entity, Vector3 size, CollisionShape shape, float mass = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;


            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));

            Vector3 inertia = Vector3.Zero;
            shape.CalculateLocalInertia(mass, out inertia);
            shape.LocalScaling = size;

            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, shape, inertia);
            RigidBody = new RigidBody(boxRigidBodyInfo);
            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(RigidBody);
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



            RigidBody.Friction = 1f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0.1f;

            collisionObjects.Add(RigidBody);

            return RigidBody;
        }


        public static RigidBody CreateCharacterCapsule(Entity entity, float HalfHeight, float radius, float mass = 1, CollisionFlags collisionFlags = CollisionFlags.None)
        {
            RigidBody RigidBody;

            // Create a sphere shape
            var Shape = new CapsuleShape(radius, HalfHeight);
            Shape.UserObject = new CollisionShapeData();
            var motionState = new DefaultMotionState(Matrix4x4.CreateTranslation(0, 0, 0));


            // Create a rigid body for the sphere
            var boxRigidBodyInfo = new RigidBodyConstructionInfo(collisionFlags == CollisionFlags.StaticObject ? 0 : mass, motionState, Shape);
            RigidBody = new RigidBody(boxRigidBodyInfo);
            RigidBody.CollisionFlags = collisionFlags;

            RigidBody.UserObject = entity;

            dynamicsWorld.AddRigidBody(RigidBody);

            RigidBody.Friction = 0f;
            RigidBody.SetDamping(0.1f, 0.1f);
            RigidBody.Restitution = 0f;


            collisionObjects.Add(RigidBody);

            return RigidBody;
        }

        public static ClosestRayResultCallback LineTrace(Vector3 rayStart, Vector3 rayEnd, List<CollisionObject> ignoreList = null)
        {
            CollisionWorld world = dynamicsWorld;

            MyClosestRayResultCallback rayCallback = new MyClosestRayResultCallback(ref rayStart, ref rayEnd);
            if (ignoreList is not null)
                rayCallback.ignoreList = ignoreList;

            lock (dynamicsWorld)
            {
                // Perform the ray cast
                world.RayTest(rayStart, rayEnd, rayCallback);
            }
            return rayCallback;


        }

        public static MyClosestConvexResultCallback SphereTraceForStatic(Vector3 rayStart, Vector3 rayEnd, float radius = 0.5f)
        {

            CollisionWorld world = staticWorld;

            // Create a sphere shape with the specified radius
            SphereShape sphereShape = new SphereShape(radius);

            Matrix4x4 start = Matrix4x4.CreateTranslation(rayStart);
            Matrix4x4 end = Matrix4x4.CreateTranslation(rayEnd);


            MyClosestConvexResultCallback convResultCallback = new MyClosestConvexResultCallback(ref rayStart, ref rayEnd);

            // Set the callback to respond only to static objects
            convResultCallback.FlagToRespond = CollisionFlags.StaticObject;
            lock (staticWorld)
            {
                world.ConvexSweepTest(sphereShape, start, end, convResultCallback);
            }

            return convResultCallback;
        }

        public static MyClosestConvexResultCallback SphereTrace(Vector3 rayStart, Vector3 rayEnd, List<CollisionObject> ignoreList = null, float radius = 0.5f)
        {
            CollisionWorld world = dynamicsWorld;

            // Create a sphere shape with the specified radius
            SphereShape sphereShape = new SphereShape(radius);

            Matrix4x4 start = Matrix4x4.CreateTranslation(rayStart);
            Matrix4x4 end = Matrix4x4.CreateTranslation(rayEnd);


            MyClosestConvexResultCallback convResultCallback = new MyClosestConvexResultCallback(ref rayStart, ref rayEnd);

            if (ignoreList is not null)
                convResultCallback.ignoreList = ignoreList;

            lock (staticWorld)
            {
                // Perform the sphere sweep
                world.ConvexSweepTest(sphereShape, start, end, convResultCallback);
            }

            return convResultCallback;
        }

        public static MyClosestRayResultCallback LineTraceForStatic(Vector3 rayStart, Vector3 rayEnd)
        {
            CollisionWorld world = staticWorld;

            MyClosestRayResultCallback rayCallback = new MyClosestRayResultCallback(ref rayStart, ref rayEnd);

            rayCallback.FlagToRespond = CollisionFlags.StaticObject;

            lock (staticWorld)
            {
                // Perform the ray cast
                world.RayTest(rayStart, rayEnd, rayCallback);
            }
            return rayCallback;


        }


        public static CollisionShape CreateCollisionShapeFromModel(Model model, float scale = 1.0f, CollisionShapeData shapeData = null, bool complex = false)
        {

            if (shapeData == null)
                shapeData = new CollisionShapeData();

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
            mesh.MeshParts[0].VertexBuffer.GetData(vertices);


            // 3. Access indices for correct triangle construction
            // Assuming indices are 32-bit integers
            int[] indices = new int[mesh.MeshParts[part].IndexBuffer.IndexCount];
            mesh.MeshParts[0].IndexBuffer.GetData(indices);

            // 4. Create the triangle mesh
            TriangleMesh triangleMesh = new TriangleMesh();
            for (int i = 0; i < indices.Length; i += 3)
            {
                var vertex0 = vertices[indices[i]].Position.ToPhysics();
                var vertex1 = vertices[indices[i + 1]].Position.ToPhysics();
                var vertex2 = vertices[indices[i + 2]].Position.ToPhysics();
                triangleMesh.AddTriangle(vertex0, vertex1, vertex2, true);  // Assume triangles are not welded
            }


            // 6. Create the collision shape
            BvhTriangleMeshShape meshShape = new BvhTriangleMeshShape(triangleMesh, true); // Use quantization for better performance

            return meshShape;
        }

        public class DebugDrawer : DebugDraw
        {
            GraphicsDevice graphicsDevice;
            BasicEffect basicEffect;

            public DebugDrawer(GraphicsDevice graphicsDevice)
            {
                this.graphicsDevice = graphicsDevice;
                basicEffect = new BasicEffect(graphicsDevice);
                basicEffect.VertexColorEnabled = true;
                basicEffect.LightingEnabled = false;
                basicEffect.TextureEnabled = false;

            }

            public override DebugDrawModes DebugMode
            {
                get { return DebugDrawModes.DrawWireframe; }
                set { }
            }

            public override void Draw3DText(ref Vector3 location, string textString)
            {

            }

            public override void ReportErrorWarning(string warningString)
            {

            }

            public override void DrawLine(ref Vector3 from, ref Vector3 to, ref Vector3 color)
            {
                VertexPositionColor[] vertices = new VertexPositionColor[2];
                vertices[0] = new VertexPositionColor(from, new Microsoft.Xna.Framework.Color(color.X, color.Y, color.Z));
                vertices[1] = new VertexPositionColor(to, new Microsoft.Xna.Framework.Color(color.X, color.Y, color.Z));

                basicEffect.View = Camera.finalizedView;
                basicEffect.Projection = Camera.finalizedProjection;

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
                }
            }
        }

    }
}
