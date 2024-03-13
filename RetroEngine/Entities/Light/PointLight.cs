using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Map;
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

        BoundingSphere lightSphere = new BoundingSphere();

        public PointLight() 
        {
            LateUpdateWhilePaused = true;
        }

        LightManager.PointLightData lightData = new LightManager.PointLightData();

        GraphicsDevice graphicsDevice;

        StaticMesh mesh = new StaticMesh();

        bool dirty = true;

        static bool finalizedFrame = false;

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            lightData.Color = data.GetPropertyVector("light_color", new Vector3(1,1,1)) * data.GetPropertyFloat("intensity",1);
            lightData.Radius = data.GetPropertyFloat("radius", 5);

            lights.Add(this);

            graphicsDevice = GameMain.Instance.GraphicsDevice;

            lightData.shadowData = new RenderTargetCube(graphicsDevice, 512, false, SurfaceFormat.Single, DepthFormat.Depth24);

            mesh.LoadFromFile("models/cube.obj");
            //meshes.Add(mesh);
            mesh.Visible = false;

        }

        public override void Start()
        {
            base.Start();

            if (Level.ChangingLevel == false) return;

            Render();

            mesh.Visible = true;
            mesh.Position = Position;
            mesh.texture = lightData.shadowData;
            mesh.Shader = AssetRegistry.GetShaderFromName("CubeMapVisualizer");

        }

        public override void Update()
        {
            base.Update();


            finalizedFrame = false;

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

            lightData.shadowData?.Dispose();

        }

        public override void LateUpdate()
        {
            lightData.Position = Position;

            
            lightSphere.Radius = lightData.Radius;

            if (IsBoundingSphereInFrustum(lightSphere))
                LightManager.AddPointLight(lightData);
        }

        protected Matrix GetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateTranslation(lightData.Position);
            return worldMatrix;
        }

        protected bool IsBoundingSphereInFrustum(BoundingSphere sphere)
        {
            return Camera.frustum.Contains(sphere.Transform(GetWorldMatrix())) != ContainmentType.Disjoint;
        }


        internal static void UpdateAll()
        {

            foreach(var light in lights)
            {
                light.dirty = true;
                light.Render();
                light.LateUpdate();
            }

        }

        internal static void DrawDirtyPointLights()
        {
            foreach (var light in finalLights)
            {
                if(light.dirty)
                light.Render();
            }
        }


        void Render()
        {


            RenderFace(CubeMapFace.PositiveX);
            RenderFace(CubeMapFace.NegativeX);
            RenderFace(CubeMapFace.PositiveY);
            RenderFace(CubeMapFace.NegativeY);
            RenderFace(CubeMapFace.PositiveZ);
            RenderFace(CubeMapFace.NegativeZ);

            dirty = false;

        }

        void RenderFace(CubeMapFace face)
        {

            var view = GetViewForFace(face);
            var projection = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(90f), 1, 0.005f, lightData.Radius*1.5f);


            var l = Level.GetCurrent().GetAllOpaqueMeshes();

            graphicsDevice.SetRenderTarget(lightData.shadowData, face);
            graphicsDevice.Clear(Color.Black);

            GameMain.Instance.render.BoundingSphere.Radius = lightData.Radius;
            GameMain.Instance.render.BoundingSphere.Center = lightData.Position;

            GameMain.Instance.render.OcclusionEffect.Parameters["View"].SetValue(view);
            GameMain.Instance.render.OcclusionEffect.Parameters["Projection"].SetValue(projection);
            GameMain.Instance.render.OcclusionEffect.Parameters["CameraPos"].SetValue(Position);
            GameMain.Instance.render.OcclusionEffect.Parameters["pointDistance"].SetValue(true);

            GameMain.Instance.render.RenderLevelGeometryDepth(l, OnlyStatic: true, onlyShadowCasters: true);

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

        

    }
}
