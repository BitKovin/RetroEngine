using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        new myGame().Run();

    }

    class myGame : Game
    {

        public static GraphicsDeviceManager _graphics;
        public myGame()
        {
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this);
        }

        private static VertexBuffer vertexBuffer;
        private static IndexBuffer indexBuffer;
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Effect ef = Content.Load<Effect>("ReflectionPath");

            ef.Techniques[0].Passes[0].Apply();
            DrawFullScreenQuad(null, ef);   

        }

        private static void InitializeFullScreenQuad(GraphicsDevice graphicsDevice)
        {
            if (vertexBuffer == null)
            {
                VertexPositionTexture[] vertices =
                {
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            new VertexPositionTexture(new Vector3(-1,  1, 0), new Vector2(0, 0)),
            new VertexPositionTexture(new Vector3( 1, -1, 0), new Vector2(1, 1)),
            new VertexPositionTexture(new Vector3( 1,  1, 0), new Vector2(1, 0)),
                };

                vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
                vertexBuffer.SetData(vertices);

                int[] indices = { 0, 1, 2, 2, 1, 3 };

                indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
                indexBuffer.SetData(indices);
            }
        }
        internal static void DrawFullScreenQuad(Texture2D inputTexture, Effect effect = null)
        {

            var graphicsDevice = myGame._graphics.GraphicsDevice;

            InitializeFullScreenQuad(graphicsDevice);

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;



            if (effect != null)
            {
                effect.Parameters["Texture"]?.SetValue(inputTexture);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                }
            }
            else
            {
                BasicEffect basicEffect = new BasicEffect(graphicsDevice)
                {
                    TextureEnabled = true,
                    Texture = inputTexture,
                    VertexColorEnabled = false,
                };

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
                }
            }
        }

    }

}