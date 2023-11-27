using Assimp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.UI
{
    public class Text : UiElement
    {
        public SpriteFont Font;
        public string text = "";

        public Color baseColor = Color.White;

        public Text() : base()
        {
            Font = GameMain.inst.DefaultFont;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 pos = new Vector2(position.X + origin.X, position.Y + origin.Y);

            spriteBatch.DrawString(Font, text, pos, baseColor, rotation,relativeOrigin,size,SpriteEffects.None,0);

            base.Draw(gameTime, spriteBatch);

        }
    }
}
