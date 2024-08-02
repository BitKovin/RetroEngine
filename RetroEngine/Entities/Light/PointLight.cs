using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Map;
using RetroEngine.PhysicsSystem;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Light
{

    [LevelObject("light_point")]
    public class PointLight : Entity
    {

        static List<PointLight> lights = new List<PointLight>();

        //static List<PointLight> finalLights = new List<PointLight>();

        internal BoundingSphere lightSphere = new BoundingSphere();

        List<CubeMapFace> facesToUpdate = new List<CubeMapFace>();

        public float CollisionTestRadius = 0;

        public bool SkipUp = false;
        public bool SkipDown = false;

        public PointLight() 
        {
            LateUpdateWhilePaused = true;

            lightData.shadowData = this;

            graphicsDevice = GameMain.Instance.GraphicsDevice;

            StartOrder = -1;

        }

        protected LightManager.PointLightData lightData = new LightManager.PointLightData();

        GraphicsDevice graphicsDevice;

        StaticMesh mesh = new StaticMesh();

        bool dirty = true;

        static bool finalizedFrame = false;

        public int resolution = 512;

        public float radius = 10;
        public float MinDot = -1f;

        public bool Dynamic = true;
        public bool CastShadows = false;

        public Vector3 Color = Vector3.One;
        public float Intensity = 2f;

        public float Priority = 1;

        float DynamicUpdateDystance;

        public bool enabled = true;

        internal RenderTargetCube renderTargetCube;

        LightVisibilityCheckMesh lightVisibilityCheckMesh = new LightVisibilityCheckMesh();

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Intensity = data.GetPropertyFloat("intensity", 1);

            Color = data.GetPropertyVector("light_color", Color);
            radius = data.GetPropertyFloat("radius", 5);

            lightData.Radius = radius;

            Dynamic = data.GetPropertyBool("dynamic");

            
            resolution = 256;

            resolution = (int)data.GetPropertyFloat("resolution", 256);

            Vector3 importRot = data.GetPropertyVector("angles", Vector3.Zero);


            Rotation = EntityData.ConvertRotation(importRot);

            SetAngle(data.GetPropertyFloat("light_angle", 180));

            //DrawDebug.Line(Position, Position + Rotation.GetForwardVector() * 4, Vector3.One, 40);

            CastShadows = data.GetPropertyBool("shadows",true);

            CollisionTestRadius = data.GetPropertyFloat("collisionTestRadius", 0);

            SkipUp = data.GetPropertyBool("dynamicSkipUp", false);
            SkipDown = data.GetPropertyBool("dynamicSkipDown", false);

            if (CastShadows == false)
                resolution = 5;

            lightData.Resolution = resolution;

            mesh.LoadFromFile("models/cube.obj");
            //meshes.Add(mesh);
            //mesh.Visible = false;

            lightData.shadowData = this;
            graphicsDevice = GameMain.Instance.GraphicsDevice;


        }

        public void SetAngle(float angleInDegrees)
        {
            // Convert the angle from degrees to radians
            float angleInRadians = MathHelper.ToRadians(angleInDegrees);

            // Compute the cosine of the angle
            float minDot = (float)Math.Cos(angleInRadians);

            MinDot = minDot;
        }

        public override void Start()
        {
            base.Start();

            lights.Add(this);

            if (Level.ChangingLevel == false) return;

            TestFaceSide();

            if (Level.ChangingLevel== true)
            {

                LateUpdate();

                dirty = true;

                Render(lightData);

                dirty = true;
            }
               



            mesh.Visible = true;
            mesh.Position = Position;

            

            mesh.Shader = AssetRegistry.GetShaderFromName("CubeMapVisualizer");

        }

        void TestFaceSide()
        {

            facesToUpdate = new List<CubeMapFace>();

            TestSide(CubeMapFace.PositiveX);
            TestSide(CubeMapFace.NegativeX);
            TestSide(CubeMapFace.PositiveY);
            TestSide(CubeMapFace.NegativeY);
            TestSide(CubeMapFace.PositiveZ);
            TestSide(CubeMapFace.NegativeZ);

        }

        Vector3 GetCubemapDirection(CubeMapFace face)
        {
            Vector3 dir = Vector3.Zero;

            switch (face)
            {
                case CubeMapFace.PositiveX:
                    dir = Vector3.UnitX; break;

                case CubeMapFace.NegativeX:
                    dir = -Vector3.UnitX; break;

                case CubeMapFace.PositiveY:
                    dir = Vector3.UnitY; break;

                case CubeMapFace.NegativeY:
                    dir = -Vector3.UnitY; break;

                case CubeMapFace.PositiveZ:
                    dir = Vector3.UnitZ; break;

                case CubeMapFace.NegativeZ:
                    dir = -Vector3.UnitZ; break;

            }

            return dir;
        }

        void TestSide(CubeMapFace face)
        {


            Vector3 dir = GetCubemapDirection(face);

            Vector3 testPos = Position + dir * CollisionTestRadius;

            if (face == CubeMapFace.PositiveY && SkipUp)
                return;

            if (face == CubeMapFace.NegativeY && SkipDown)
                return;

            if (Physics.LineTraceForStatic(Position.ToPhysics(), testPos.ToPhysics()).HasHit)
            {
                //DrawDebug.Sphere(CollisionTestRadius, testPos, Vector3.Zero, 10);
                return;
            }

            

            facesToUpdate.Add(face);

        }

        public override void AsyncUpdate()
        {
            base.Update();


            //Rotation += new Vector3(0, Time.DeltaTime * 300, 0);

            float dist = Vector3.Distance(Camera.position, Position);

            if (dist < (lightData.Radius + 2) * 2)
            {
                SetLightResolution(resolution);
            }
            if(dist > (lightData.Radius + 2) * 2)
            {
                SetLightResolution((int)((float)resolution/1.5f));
            }
            if (dist > (lightData.Radius+3) * 3)
            {
                SetLightResolution((int)((float)resolution / 2));
            }
            if (dist > (lightData.Radius + 4) * 5)
            {
                SetLightResolution(resolution / 3);
            }

            finalizedFrame = false;

        }

        bool isDynamic()
        {
            return Dynamic;
        }

        public override void FinalizeFrame()
        {
            base.FinalizeFrame();


            if (finalizedFrame == true) return;

            //finalLights = new List<PointLight>(lights);

            finalizedFrame = true;


        }

        public override void Destroy()
        {
            base.Destroy();

            lights.Remove(this);

            lock(GameMain.pendingDispose)
            {
                GameMain.pendingDispose.Add(renderTargetCube);
            }
            //renderTargetCube?.Dispose();


        }

        public void SetLightResolution(int res)
        {
            if(res<1)
                res = 1;

            lightData.Resolution = res;
        }

        public override void LateUpdate()
        {



            lightVisibilityCheckMesh.Scale = new Vector3(lightData.Radius);
            lightVisibilityCheckMesh.Position = lightData.Position;

            lightSphere.Radius = lightData.Radius;

            if (enabled == false) return;

            float cameraDist = Vector3.Distance(Camera.finalizedPosition, lightData.Position);
            bool visible = (lightVisibilityCheckMesh.IsVisible() == false && cameraDist > lightData.Radius * 1.2)==false;


            lightData.Position = Position;
            lightData.Radius = radius;

            lightData.Color = Color * Intensity;

            lightData.MinDot = MinDot;
            lightData.Direction = Rotation.GetForwardVector();

            if ((IsBoundingSphereInFrustum(lightSphere) && visible) || Level.ChangingLevel)
            LightManager.AddPointLight(lightData);
        }

        protected Matrix GetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateTranslation(lightData.Position);
            return worldMatrix;
        }

        internal bool IsBoundingSphereInFrustum(BoundingSphere sphere)
        {
            return Camera.frustum.Contains(sphere.Transform(GetWorldMatrix())) != ContainmentType.Disjoint;
        }


        internal static void UpdateAll()
        {
            LightManager.ClearPointLights();
            foreach (var light in lights)
            {
                light.dirty = true;
                light.LateUpdate();
                //LightManager.ClearPointLights();
                light.Render(light.lightData);
                light.LateUpdate();
            }

        }

        internal static void LateUpdateAll()
        {
            foreach (var light in lights)
            {
                light.LateUpdate();
            }
        }

        internal static void DrawDirtyPointLights()
        {
            foreach (var light in LightManager.FinalPointLights)
            {
                if (Level.ChangingLevel)
                {
                    light.shadowData.Render(light.shadowData.lightData);
                }
                else
                {
                    light.shadowData.Render(light);
                }
            }
        }

        void InitRenderTargetIfNeeded()
        {

            if (Destroyed) return;

            if (renderTargetCube == null)
                InitRenderTarget();

            if(renderTargetCube.Size!= lightData.Resolution)
                InitRenderTarget();

        }


        void InitRenderTarget()
        {
            lock (GameMain.pendingDispose)
            {
                GameMain.pendingDispose.Add(renderTargetCube);
            }

            renderTargetCube = new RenderTargetCube(graphicsDevice, lightData.Resolution, false, resolution>400 ? SurfaceFormat.Single : SurfaceFormat.HalfSingle, DepthFormat.Depth24);

            mesh.texture = renderTargetCube;

            dirty = true;

        }

        void Render(LightManager.PointLightData pointLightData)
        {

            if (Destroyed)
                return;

            InitRenderTargetIfNeeded();

            if (isDynamic() == false && dirty == false && Level.ChangingLevel==false)
            {
                return;
            }



            if (CastShadows == false) return;



            if (isDynamic() && Level.ChangingLevel == false)
            {

                if (enabled == false)
                    return;

                DynamicUpdateDystance = (pointLightData.Radius + 10) * 6;

                float cameraDist = Vector3.Distance(Camera.finalizedPosition, pointLightData.Position);
                if (cameraDist >= DynamicUpdateDystance)
                    return;


                BoundingSphere boundingSphere = new BoundingSphere(pointLightData.Position, pointLightData.Radius);

                if (Camera.frustum.Contains(boundingSphere) == ContainmentType.Disjoint)
                    return;



                if (lightVisibilityCheckMesh.IsVisible() == false && cameraDist > pointLightData.Radius * 1.5) 
                    return;


            }

            if (CastShadows == false) return;


            RetroEngine.Render.IgnoreFrustrumCheck = true;

            //DrawDebug.Sphere(lightData.Radius, lightData.Position, Vector3.One, 0.01f);

            //lightData.Radius = radius;



            RenderFace(CubeMapFace.PositiveX, pointLightData);
            RenderFace(CubeMapFace.NegativeX, pointLightData);
            RenderFace(CubeMapFace.PositiveY, pointLightData);
            RenderFace(CubeMapFace.NegativeY, pointLightData);
            RenderFace(CubeMapFace.PositiveZ, pointLightData);
            RenderFace(CubeMapFace.NegativeZ, pointLightData);
            RetroEngine.Render.IgnoreFrustrumCheck = false;

            bool wasDirty = dirty;

            dirty = false;
        }

        void RenderFace(CubeMapFace face, LightManager.PointLightData lightData)
        {

            if ((GameMain.SkipFrames < 1 || Level.ChangingLevel == false) && facesToUpdate.Contains(face) == false)
                return;

            //DrawDebug.Line(Position, Position + GetCubemapDirection(face));

            var view = GetViewForFace(face, lightData);
            var projection = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(90f), 1, 0.005f, lightData.Radius*1.5f);


            var l = Level.GetCurrent().GetAllOpaqueMeshes();

            graphicsDevice.SetRenderTarget(renderTargetCube, face);
            graphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);


            GameMain.Instance.render.BoundingSphere.Radius = lightData.Radius;
            GameMain.Instance.render.BoundingSphere.Center = lightData.Position;

            GameMain.Instance.render.OcclusionEffect.Parameters["ViewProjection"].SetValue(view * projection);
            GameMain.Instance.render.OcclusionEffect.Parameters["CameraPos"].SetValue(lightData.Position);
            GameMain.Instance.render.OcclusionEffect.Parameters["pointDistance"].SetValue(true);

            GameMain.Instance.render.OcclusionStaticEffect.Parameters["ViewProjection"].SetValue(view * projection);
            GameMain.Instance.render.OcclusionStaticEffect.Parameters["CameraPos"].SetValue(lightData.Position);
            GameMain.Instance.render.OcclusionStaticEffect.Parameters["pointDistance"].SetValue(true);

            GameMain.Instance.render.RenderLevelGeometryDepth(l, OnlyStatic: !isDynamic(), onlyShadowCasters: true, pointLight: true);

            GameMain.Instance.render.BoundingSphere.Radius = 0;
            graphicsDevice.SetRenderTarget(null);
        }

        Matrix GetViewForFace(CubeMapFace face, LightManager.PointLightData lightData)
        {
            switch (face)
            {
                case CubeMapFace.NegativeX:
                    return Matrix.CreateLookAt(lightData.Position + Vector3.Zero, lightData.Position + new Vector3(1, 0, 0), Vector3.Up);
                case CubeMapFace.PositiveX:
                    return Matrix.CreateLookAt(lightData.Position + Vector3.Zero, lightData.Position + new Vector3(-1, 0, 0), Vector3.Up);
                case CubeMapFace.PositiveY:
                    return Matrix.CreateLookAt(lightData.Position + Vector3.Zero, lightData.Position + new Vector3(0, 1, 0), new Vector3(0, 0, -1));
                case CubeMapFace.NegativeY:
                    return Matrix.CreateLookAt(lightData.Position + Vector3.Zero, lightData.Position + new Vector3(0, -1, 0), new Vector3(0, 0, 1));
                case CubeMapFace.PositiveZ:
                    return Matrix.CreateLookAt(lightData.Position + Vector3.Zero, lightData.Position + new Vector3(0, 0, 1), Vector3.Up);
                case CubeMapFace.NegativeZ:
                    return Matrix.CreateLookAt(lightData.Position + Vector3.Zero, lightData.Position + new Vector3(0, 0, -1), Vector3.Up);
            }

            return Matrix.Identity;
        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            lightVisibilityCheckMesh.LoadFromFile("engine/models/pointLightVisibility.fbx");
            lightVisibilityCheckMesh.texture = GameMain.Instance.render.black;
            meshes.Add(lightVisibilityCheckMesh);

        }


    }
}
