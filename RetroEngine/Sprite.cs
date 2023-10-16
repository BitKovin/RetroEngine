using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class Sprite
    {

        public Vector2 Position;

        public Vector2 Origin;

        public Texture2D texture;

        public float Rotation;

        public Vector2 Scale = new Vector2(1,1);

        public int depth;

        public SpriteEffects effects;


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.White, Rotation, Origin, Scale, effects, depth);
        }

    }
}
