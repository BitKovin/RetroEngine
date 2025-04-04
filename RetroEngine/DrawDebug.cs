﻿using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Text;
using MonoGame.Extended.Text.Extensions;
using RetroEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class DrawDebug : DebugDraw
    {
        GraphicsDevice graphicsDevice;
        BasicEffect basicEffect;

        internal static DrawDebug instance;

        static List<DrawShapeCommand> commands = new List<DrawShapeCommand>();

        public static bool Enabled = false;


        [ConsoleCommand("DrawDebug")]
        public static void DrawDebugEnable(bool value)
        {
            Enabled = value;
        }


        public static void Line(Vector3 pointA, Vector3 pointB, Vector3? color =  null, float duration = 1)
        {
            if (!Enabled) return;

            Vector3 col = Vector3.UnitX;

            if(color != null)
                col = color.Value;

            lock (commands)
            {
                commands.Add(new DrawShapeLine(pointA, pointB, col, duration));
            }
        }

        public static void Box(Vector3 min, Vector3 max, Vector3? color = null, float duration = 1)
        {
            if (!Enabled) return;

            Vector3 col = Vector3.UnitX;

            if (color != null)
                col = color.Value;

            lock (commands)
            {
                commands.Add(new DrawShapeBox(min, max, col, duration));
            }
        }

        public static void Path(List<Vector3> points, Vector3? color = null, float duration = 1)
        {
            if (!Enabled) return;

            Vector3 col = Vector3.UnitX;

            if (color != null)
                col = color.Value;

            lock (commands)
            {
                for(int i = 0; i < points.Count - 1; i++)
                {
                    commands.Add(new DrawShapeLine(points[i], points[i+1], col, duration));
                }
            }
        }


        public static void Text(Vector3 position, string text, float duration = 1)
        {

            if(Enabled == false) return;

            lock (commands)
            {
                commands.Add(new DrawText(position, text, Vector3.One, duration));
            }
        }

        public static void Sphere(float radius, Vector3 position, Vector3 color, float duration = 1)
        {
            if (!Enabled) return;
            lock (commands)
            {
                commands.Add(new DrawShapeSphere(radius, position, color, duration));
            }
        }

        static List<DrawShapeCommand> finalizesCommands = new List<DrawShapeCommand>();

        internal static void FinalizeCommands()
        {
            if (!Enabled) return;

            List<DrawShapeCommand> list;
            lock (commands)
            {
                list = new List<DrawShapeCommand>(commands);
            }

            list = list.OrderBy(c => (c is DrawText) ? 1 : 0).ToList();

            foreach (var command in list)
            {
                command.Draw(instance);
            }

            finalizesCommands = list.ToList();

            lock (commands)
            {

                foreach (var command in list)
                {
                    if (command.drawTime.Wait() == false)
                    {
                        commands.Remove(command);
                    }
                }

            }
        }

        internal static void Draw()
        {
            foreach(var command in finalizesCommands)
            {
                command.Draw(instance);
            }
        }

        public DrawDebug(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            basicEffect = new BasicEffect(graphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.LightingEnabled = false;
            basicEffect.TextureEnabled = false;

            instance = this;

        }

        public override DebugDrawModes DebugMode
        {
            get { return DebugDrawModes.DrawWireframe; }
            set { }
        }

        public override void Draw3DText(ref System.Numerics.Vector3 location, string textString)
        {
            SpriteBatch spriteBatch = GameMain.Instance.SpriteBatch;

            spriteBatch.Begin(transformMatrix: Camera.UiMatrix, blendState: BlendState.AlphaBlend);



            Vector2 pos = UiElement.WorldToScreenSpace(location, out var found);

            if (found)
            {

                spriteBatch.DrawString(GameMain.Instance.DefaultFont, textString, pos * Render.ResolutionScale, Vector2.Zero, Vector2.One / 72 * 14, Vector2.One, Color.White);
            }
            spriteBatch.End();
        }

        public override void ReportErrorWarning(string warningString)
        {

        }

        #region sphere
        private static VertexPositionColor[] sphereVertices;
        private static int[] sphereIndices;
        private static bool sphereInitialized = false;

        private void InitializeSphere(ref System.Numerics.Vector3 color)
        {

            float radius = 1;

            const int segments = 24*2;
            const int rings = 24;
            var vertices = new List<VertexPositionColor>();
            var indices = new List<int>();

            for (int i = 0; i <= rings; i++)
            {
                float theta = i * MathF.PI / rings;
                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int j = 0; j <= segments; j++)
                {
                    float phi = j * 2 * MathF.PI / segments;
                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    System.Numerics.Vector3 position = new System.Numerics.Vector3
                    (
                        radius * sinTheta * cosPhi,
                        radius * cosTheta,
                        radius * sinTheta * sinPhi
                    );

                    vertices.Add(new VertexPositionColor(
                        new Microsoft.Xna.Framework.Vector3(position.X, position.Y, position.Z),
                        new Microsoft.Xna.Framework.Color(color.X, color.Y, color.Z)
                    ));
                }
            }

            for (int i = 0; i < rings; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int first = (i * (segments + 1)) + j;
                    int second = first + segments + 1;

                    indices.Add(first);
                    indices.Add(first + 1);

                    indices.Add(first);
                    indices.Add(second);
                }
            }

            sphereVertices = vertices.ToArray();
            sphereIndices = indices.ToArray();
            sphereInitialized = true;
        }

        public override void DrawSphere(float radius, ref System.Numerics.Matrix4x4 transform, ref System.Numerics.Vector3 color)
        {
            if (!sphereInitialized)
            {
                InitializeSphere(ref color);
            }

            for (int i = 0; i < sphereVertices.Length; i++)
            {
                var vertex = sphereVertices[i];
                vertex.Color = new Microsoft.Xna.Framework.Color(color.X, color.Y, color.Z);
                sphereVertices[i] = vertex;
            }

            basicEffect.View = Camera.finalizedView;
            basicEffect.Projection = Camera.finalizedProjection;
            basicEffect.World = Matrix.CreateScale(radius) * transform;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.LineList,
                    sphereVertices,
                    0,
                    sphereVertices.Length,
                    sphereIndices,
                    0,
                    sphereIndices.Length / 2
                );
            }
            basicEffect.World = Matrix.Identity;
        }

        #endregion

        public override void DrawLine(ref System.Numerics.Vector3 from, ref System.Numerics.Vector3 to, ref System.Numerics.Vector3 color)
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

        class DrawShapeCommand
        {

            public Delay drawTime = new Delay();

            public DrawShapeCommand(float time)
            {
                drawTime.AddDelay(time);
            }

            public virtual void Draw(DebugDraw draw)
            {

            }

        }

        class DrawText : DrawShapeCommand
        {

            System.Numerics.Vector3 position;
            string text;
            Vector3 Color;

            public DrawText(Vector3 Position, string text, Vector3 Color, float time) : base(time)
            {
                position = Position.ToPhysics();
                this.text = text;
                this.Color = Color;
            }

            public override void Draw(DebugDraw draw)
            {
                base.Draw(draw);

                draw.Draw3DText(ref position, text);

            }

        }

        class DrawShapeLine : DrawShapeCommand
        {

            Vector3 pointA;
            Vector3 pointB;
            Vector3 Color;

            public DrawShapeLine(Vector3 a, Vector3 b, Vector3 color, float time) : base(time)
            {
                pointA = a;
                pointB = b;
                Color = color;
            }

            public override void Draw(DebugDraw draw)
            {
                base.Draw(draw);

                draw.DrawLine(pointA.ToPhysics(), pointB.ToPhysics(), Color.ToPhysics());

            }

        }

        class DrawShapeBox : DrawShapeCommand
        {

            System.Numerics.Vector3 pointA;
            System.Numerics.Vector3 pointB;
            System.Numerics.Vector3 Color;

            public DrawShapeBox(Vector3 min, Vector3 max, Vector3 color, float time) : base(time)
            {
                pointA = min.ToNumerics();
                pointB = max.ToNumerics();
                Color = color.ToNumerics();
            }

            public override void Draw(DebugDraw draw)
            {
                base.Draw(draw);

                draw.DrawBox(ref pointA, ref pointB, ref Color);

            }

        }


        class DrawShapeSphere : DrawShapeCommand
        {

            float radius;
            Vector3 position;
            Vector3 Color;

            public DrawShapeSphere(float radius, Vector3 position, Vector3 color, float time) : base(time)
            {
                this.radius = radius;
                this.position = position;
                Color = color;
            }

            public override void Draw(DebugDraw draw)
            {
                base.Draw(draw);

                var mat = Matrix.CreateTranslation(position).ToPhysics();

                var col = Color.ToPhysics();

                draw.DrawSphere(radius, ref mat, ref col);

            }

        }


    }
}

