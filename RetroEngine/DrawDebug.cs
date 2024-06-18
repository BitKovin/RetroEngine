using BulletSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public static void Line(Vector3 pointA, Vector3 pointB, Vector3 color, float duration = 1)
        {
            if (!Enabled) return;
            lock (commands)
            {
                commands.Add(new DrawShapeLine(pointA, pointB, color, duration));
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

        internal static void Draw()
        {

            if (!Enabled) return;

            List<DrawShapeCommand> list;
            lock (commands)
            {
                list = new List<DrawShapeCommand>(commands);

                foreach(var command in list)
                {
                    if(command.drawTime.Wait()==false)
                    {
                        commands.Remove(command);
                    }
                }

                list = new List<DrawShapeCommand>(commands);
            }

            foreach(var command in list)
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

        }

        public override void ReportErrorWarning(string warningString)
        {

        }

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

