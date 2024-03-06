using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Entities.Light;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("cubeMap")]
    internal class CubeMap : Entity
    {

        RenderTargetCube map;

        GraphicsDevice graphicsDevice;

        int rendered = 0;

        StaticMesh mesh = new StaticMesh();

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            graphicsDevice = GameMain.Instance.GraphicsDevice;

            map = new RenderTargetCube(graphicsDevice, 512, false, SurfaceFormat.Color, DepthFormat.Depth24);

            mesh.LoadFromFile("models/cube.obj");
            meshes.Add(mesh);
            mesh.Visible = false;

        }

        public override void Start()
        {
            base.Start();

            Render();
            mesh.Visible = true;
            mesh.Position = Position;
            mesh.texture = map;
            mesh.Shader = AssetRegistry.GetShaderFromName("CubeMapVisualizer");

        }

        void Render()
        {
            rendered++;

            RenderFace(CubeMapFace.PositiveX);
            RenderFace(CubeMapFace.NegativeX);
            RenderFace(CubeMapFace.PositiveY);
            RenderFace(CubeMapFace.NegativeY);
            RenderFace(CubeMapFace.PositiveZ);
            RenderFace(CubeMapFace.NegativeZ);

        }

        void RenderFace(CubeMapFace face)
        {

            Camera.position = Position;

            Camera.finalizedView = GetViewForFace(face);
            Camera.view = Camera.finalizedView;
            Camera.finalizedProjection = Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(90), 1.0f, 1, 100.0f);
            Camera.projection = Camera.finalizedProjection;

            Camera.frustum.Matrix = Camera.finalizedView * Camera.finalizedProjection;

            GameMain.Instance.render.FillPrepas();

            PointLight.UpdateAll();

            Level.GetCurrent().RenderPreparation();
            var l = Level.GetCurrent().GetMeshesToRender();

            foreach(var mesh in l)
            {
                mesh.occluded = false;
                mesh.inFrustrum = true;
                mesh.isRendered = true;
            }

            GameMain.Instance.render.RenderShadowMap(l);

            GameMain.Instance.render.UpdateShaderFrameData();

            graphicsDevice.SetRenderTarget(map, face);
            graphicsDevice.Clear(Graphics.BackgroundColor);

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

            map.Dispose();

        }

    }
}
