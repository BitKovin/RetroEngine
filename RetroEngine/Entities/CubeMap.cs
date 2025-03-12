using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Entities.Light;
using RetroEngine.Map;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("cubeMap")]
    internal class CubeMap : Entity
    {

        static internal List<CubeMap> cubeMaps = new List<CubeMap>();

        static internal List<CubeMap> cubeMapsFinalized = new List<CubeMap>();

        static BoundingSphere cameraPoint = new BoundingSphere(Vector3.Zero, 0.1f);

        internal RenderTargetCube map;

        GraphicsDevice graphicsDevice;

        int resolution = 256;

        StaticMesh mesh = new StaticMesh();

        BoundingBox boundingBox;

        public Vector3 boundingBoxMin = Vector3.Zero;
        public Vector3 boundingBoxMax = Vector3.Zero;

        public bool Infinite = false;

        Vector3 initialPosition = Vector3.Zero;

        string targetName = "";

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            graphicsDevice = GameMain.Instance.GraphicsDevice;

            resolution = (int)data.GetPropertyFloat("resolution",256);

            map = new RenderTargetCube(graphicsDevice, resolution, false, SurfaceFormat.ColorSRgb, DepthFormat.Depth24);

            targetName = data.GetPropertyString("target", "cubemapTarget_default");

            if(meshes.Count == 0)
            {
                boundingBoxMin = Vector3.Zero;
                boundingBoxMax = Vector3.Zero;
                Infinite = true;
            }

            List<Vector3> vertices = new List<Vector3>();

            foreach (var m in meshes)
            {
                vertices.AddRange(m.GetMeshVertices());
            }

            if (vertices.Count > 0)
            {
                boundingBox = BoundingBox.CreateFromPoints(vertices.ToArray());

                boundingBoxMin = boundingBox.Min;
                boundingBoxMax = boundingBox.Max;
            }

            meshes.Clear();

            mesh.LoadFromFile("models/cube.obj");
            //meshes.Add(mesh);
            mesh.Visible = false;

            boundingBox = new BoundingBox(boundingBoxMin, boundingBoxMax);

            StartOrder = 10;

        }

        public override void Start()
        {
            base.Start();

            AlwaysFinalizeFrame = true;

            UpdateWhilePaused = true;
            LateUpdateWhilePaused = true;

            Console.WriteLine("rendering cubemap!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //Thread.Sleep(2000);

            graphicsDevice.Clear(Color.Black);

            initialPosition = Position;

            Entity target = Level.GetCurrent().FindEntityByName(targetName);

            if (target != null)
            {
                Position = target.Position;
                initialPosition = target.Position;
            }
            else if (Infinite == false && initialPosition!= Vector3.Zero)
            {
                Position = (boundingBoxMin + boundingBoxMax) / 2f;

                Position = Navigation.ProjectToGround(Position) + Vector3.UnitY * 1.4f;
            }

            Render();
            mesh.Visible = true;
            mesh.Position = Position;
            mesh.texture = map;
            mesh.Shader = new Graphic.SurfaceShaderInstance("CubeMapVisualizer");


            foreach (RigidBody body in bodies)
            {

                body.CollisionFlags = BulletSharp.CollisionFlags.NoContactResponse;
                body.SetBodyType(PhysicsSystem.BodyType.None);
                body.SetCollisionMask(PhysicsSystem.BodyType.None);
                Physics.Remove(body);
            }

            LateUpdateWhilePaused = true;

            bodies.Clear();
        }

        public override void Update()
        {
            base.Update();

            cubeMaps.Clear();


            finalizedMaps = false;

        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            cameraPoint.Center = Camera.position;

            if (boundingBox.Intersects(cameraPoint) || Infinite)
            {
                cubeMaps.Add(this);
            }

        }

        static bool finalizedMaps = false;

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();

            cubeMapsFinalized = new List<CubeMap>(cubeMaps);

            finalizedMaps = true;
        }

        static CubeMap lastCubemap = null;

        public static CubeMap GetClosestToCamera()
        {
            if (cubeMapsFinalized.Count == 0)
                return null;

            cubeMapsFinalized = cubeMapsFinalized.OrderBy(m => Vector3.Distance(m.initialPosition, Camera.position) + (m.Infinite? 2000 : 0)).ToList();

            var selected = cubeMapsFinalized[0];

            if (selected == null || selected.Destroyed)
            {
                selected = lastCubemap;
            }

            if (selected == null || selected.Destroyed)
            {
                selected = null;
            }

            if (selected != null)
            {
                lastCubemap = selected;
            }
            return selected;

        }

        void Render()
        {

            if(Destroyed)
                return;

            PointLight.UpdateAll();

            Vector3 startPos = Camera.position;
            Matrix view = Camera.view;
            Matrix projection = Camera.projection;

            PointLight.LateUpdateAll();

            RenderFace(CubeMapFace.PositiveX);
            RenderFace(CubeMapFace.PositiveX);
            RenderFace(CubeMapFace.NegativeX);
            RenderFace(CubeMapFace.PositiveY);
            RenderFace(CubeMapFace.NegativeY);
            RenderFace(CubeMapFace.PositiveZ);
            RenderFace(CubeMapFace.NegativeZ);


            Camera.position = startPos;
            Camera.view = view;
            Camera.projection = projection;

            Level.GetCurrent().RenderPreparation();

        }

        void RenderFace(CubeMapFace face)
        {


            Camera.position = Position;

            Camera.finalizedView = GetViewForFace(face);
            Camera.view = Camera.finalizedView;
            Camera.finalizedProjection = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(90), 1.0f, 1, 10000.0f);
            Camera.projection = Camera.finalizedProjection;

            Camera.frustum.Matrix = Camera.finalizedView * Camera.finalizedProjection;

            GameMain.Instance.render.FillPrepas();

            graphicsDevice.SetRenderTarget(GameMain.Instance.render.ReflectionOutput);
            graphicsDevice.Clear(Color.Black);
            
            PointLight.LateUpdateAll();
            Level.GetCurrent().RenderPreparation();
            var l = Level.GetCurrent().GetMeshesToRender();

            foreach(var mesh in l)
            {
                mesh.occluded = false;
                mesh.inFrustrum = true;
                mesh.isRendered = true;
            }

            GameMain.Instance.render.RenderShadowMap(l);
            GameMain.Instance.render.RenderShadowMapClose(l);
            GameMain.Instance.render.RenderShadowMapVeryClose(l);

            GameMain.Instance.render.UpdateShaderFrameData();

            graphicsDevice.SetRenderTarget(map, face);
            graphicsDevice.Clear(Color.Black);

            l = Level.GetCurrent().GetMeshesToRender();

            GameMain.Instance.render.RenderLevelGeometryForward(l, OnlyStatic: true);

            graphicsDevice.SetRenderTarget(null);
        }

        Matrix GetViewForFace(CubeMapFace face)
        {
            switch (face)
            {
                case CubeMapFace.NegativeX:
                    return Matrix.CreateLookAt(Position + Vector3.Zero, Position + new Vector3(1,0,0), Vector3.Up);
                case CubeMapFace.PositiveX:
                    return Matrix.CreateLookAt(Position + Vector3.Zero, Position + new Vector3(-1, 0, 0), Vector3.Up);
                case CubeMapFace.PositiveY:
                    return Matrix.CreateLookAt(Position + Vector3.Zero, Position + new Vector3(0, 1, 0), new Vector3(0, 0, -1));
                case CubeMapFace.NegativeY:
                    return Matrix.CreateLookAt(Position + Vector3.Zero, Position + new Vector3(0, -1, 0), new Vector3(0, 0, 1));
                case CubeMapFace.PositiveZ:
                    return Matrix.CreateLookAt(Position + Vector3.Zero, Position + new Vector3(0, 0, 1), Vector3.Up);
                case CubeMapFace.NegativeZ:
                    return Matrix.CreateLookAt(Position + Vector3.Zero, Position + new Vector3(0, 0, -1), Vector3.Up);
            }

            return Matrix.Identity;
        }

        public override void Destroy()
        {
            base.Destroy();

            map?.Dispose();

        }

    }
}
