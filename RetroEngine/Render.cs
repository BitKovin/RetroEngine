using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    public class Render
    {

        RenderTarget2D colorPath;

        RenderTarget2D outputPath;

        GraphicsDeviceManager graphics;

        public RenderTarget2D StartRenderLevel(Level level)
        {
            graphics = GameMain.inst._graphics;

            InitRenderTargetIfNeed(ref colorPath);
            InitRenderTargetIfNeed(ref outputPath);

            RenderColorPath(level);

            outputPath = colorPath;

            return outputPath;
        }

        void RenderColorPath(Level level)
        {
            graphics.GraphicsDevice.SetRenderTarget(colorPath);


            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (Entity ent in level.entities)
            {
                
                if(ent.meshes is not null)
                foreach (StaticMesh mesh in ent.meshes)
                    mesh.DrawNormals();
            }
        }

        void InitRenderTargetIfNeed(ref RenderTarget2D target)
        {
            if (target is null || target.Width != graphics.PreferredBackBufferWidth || target.Height != graphics.PreferredBackBufferHeight)
            {
                // Dispose of the old render target if it exists
                target?.Dispose();

                // Set the depth format based on your requirements
                DepthFormat depthFormat = DepthFormat.Depth24;

                // Create the new render target with the specified depth format
                target = new RenderTarget2D(
                    graphics.GraphicsDevice,
                    graphics.PreferredBackBufferWidth,
                    graphics.PreferredBackBufferHeight,
                    false, // No mipmaps
                    SurfaceFormat.Color, // Color format
                    depthFormat); // Depth format
            }
        }


    }
}
