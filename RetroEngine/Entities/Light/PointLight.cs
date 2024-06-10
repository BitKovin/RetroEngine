using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Map;
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

        static List<PointLight> finalLights = new List<PointLight>();

        internal BoundingSphere lightSphere = new BoundingSphere();

        public PointLight() 
        {
            LateUpdateWhilePaused = true;

            lightData.shadowData = this;

            graphicsDevice = GameMain.Instance.GraphicsDevice;

            StartOrder = -1;

        }

        LightManager.PointLightData lightData = new LightManager.PointLightData();

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

            CastShadows = data.GetPropertyBool("shadows",true);

            if (CastShadows == false)
                resolution = 2;

            lightData.Resolution = resolution;

            mesh.LoadFromFile("models/cube.obj");
            //meshes.Add(mesh);
            //mesh.Visible = false;

            

        }

        public override void Start()
        {
            base.Start();

            lights.Add(this);

            if (Level.ChangingLevel == false) return;

            Render();

            mesh.Visible = true;
            mesh.Position = Position;

            

            mesh.Shader = AssetRegistry.GetShaderFromName("CubeMapVisualizer");

        }

        public override void Update()
        {
            base.Update();

            Rotation += new Vector3(0, Time.DeltaTime * 300, 0);

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

            finalLights = new List<PointLight>(lights);

            finalizedFrame = true;


        }

        public override void Destroy()
        {
            base.Destroy();

            lights.Remove(this);

            renderTargetCube?.Dispose();

        }

        public void SetLightResolution(int res)
        {
            if(res<1)
                res = 1;

            lightData.Resolution = res;
        }

        public override void LateUpdate()
        {
            lightData.Position = Position;
            lightData.Radius = radius;

            lightData.Color = Color * Intensity;

            lightData.MinDot = MinDot;
            lightData.Direction = Rotation.GetForwardVector();



            lightVisibilityCheckMesh.Scale = new Vector3(lightData.Radius);
            lightVisibilityCheckMesh.Position = lightData.Position;

            lightSphere.Radius = lightData.Radius;

            if (enabled == false) return;

            float cameraDist = Vector3.Distance(Camera.finalizedPosition, Position);
            bool visible = (lightVisibilityCheckMesh.IsVisible() == false && cameraDist > lightData.Radius * 1.2)==false;

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
                LightManager.ClearPointLights();
                light.Render();
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
            foreach (var light in finalLights)
            {
                light.Render();
            }
        }

        void InitRenderTargetIfNeeded()
        {

            if (renderTargetCube == null)
                InitRenderTarget();

            if(renderTargetCube.Size!= lightData.Resolution)
                InitRenderTarget();

        }


        void InitRenderTarget()
        {
            renderTargetCube?.Dispose();

            renderTargetCube = new RenderTargetCube(graphicsDevice, lightData.Resolution, false, SurfaceFormat.Single, DepthFormat.Depth24);

            mesh.texture = renderTargetCube;

            dirty = true;

        }

        void Render()
        {


            if (GameMain.SkipFrames > 0)
                InitRenderTarget();

            InitRenderTargetIfNeeded();

            if (isDynamic() == false && dirty == false && Level.ChangingLevel==false)
            {
                return;
            }



            if (CastShadows == false) return;



            if (isDynamic() && Level.ChangingLevel == false)
            {

                DynamicUpdateDystance = (lightData.Radius + 10) * 6;

                float cameraDist = Vector3.Distance(Camera.finalizedPosition, Position);
                if (cameraDist >= DynamicUpdateDystance)
                    return;


                BoundingSphere boundingSphere = new BoundingSphere(lightData.Position, lightData.Radius);

                if (Camera.frustum.Contains(boundingSphere) == ContainmentType.Disjoint)
                    return;



                if (lightVisibilityCheckMesh.IsVisible() == false && cameraDist > lightData.Radius * 1.2) 
                    return;


            }

            if (CastShadows == false) return;



            RetroEngine.Render.IgnoreFrustrumCheck = true;


            lightData.Radius = radius;

            RenderFace(CubeMapFace.PositiveX);
            RenderFace(CubeMapFace.NegativeX);
            RenderFace(CubeMapFace.PositiveY);
            RenderFace(CubeMapFace.NegativeY);
            RenderFace(CubeMapFace.PositiveZ);
            RenderFace(CubeMapFace.NegativeZ);
            RetroEngine.Render.IgnoreFrustrumCheck = false;

            bool wasDirty = dirty;

            dirty = false;
        }

        void RenderFace(CubeMapFace face)
        {



            var view = GetViewForFace(face);
            var projection = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(90f), 1, 0.005f, lightData.Radius*1.5f);


            var l = Level.GetCurrent().GetAllOpaqueMeshes();

            graphicsDevice.SetRenderTarget(renderTargetCube, face);
            graphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            GameMain.Instance.render.BoundingSphere.Radius = lightData.Radius;
            GameMain.Instance.render.BoundingSphere.Center = lightData.Position;

            GameMain.Instance.render.OcclusionEffect.Parameters["ViewProjection"].SetValue(view * projection);
            GameMain.Instance.render.OcclusionEffect.Parameters["CameraPos"].SetValue(Position);
            GameMain.Instance.render.OcclusionEffect.Parameters["pointDistance"].SetValue(true);

            GameMain.Instance.render.OcclusionStaticEffect.Parameters["ViewProjection"].SetValue(view * projection);
            GameMain.Instance.render.OcclusionStaticEffect.Parameters["CameraPos"].SetValue(Position);
            GameMain.Instance.render.OcclusionStaticEffect.Parameters["pointDistance"].SetValue(true);

            GameMain.Instance.render.RenderLevelGeometryDepth(l, OnlyStatic: !isDynamic(), onlyShadowCasters: true);

            GameMain.Instance.render.BoundingSphere.Radius = 0;
            graphicsDevice.SetRenderTarget(null);
        }

        Matrix GetViewForFace(CubeMapFace face)
        {
            switch (face)
            {
                case CubeMapFace.NegativeX:
                    return Matrix.CreateLookAt(Position + Vector3.Zero, Position + new Vector3(1, 0, 0), Vector3.Up);
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

        protected override void LoadAssets()
        {
            base.LoadAssets();

            lightVisibilityCheckMesh.LoadFromFile("engine/models/pointLightVisibility.fbx");
            lightVisibilityCheckMesh.texture = GameMain.Instance.render.black;
            meshes.Add(lightVisibilityCheckMesh);

        }


    }
}
