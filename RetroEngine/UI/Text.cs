using Assimp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace RetroEngine.UI
{
    public class Text : UiElement
    {
        public SpriteFont Font;
        public string text = "";

        public float FontSize = 24f;

        public Vector2 AlignProgress = new Vector2();

        public Color baseColor = Color.White;

        public Text() : base()
        {
            Font = GameMain.Instance.DefaultFont;
        }

        public override void Update()
        {

            size = GetSize();

            base.Update();
        }

        public override Vector2 GetSize()
        {
            return Font.MeasureString(text) / 72 * FontSize;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 pos = new Vector2(position.X + offset.X, position.Y + offset.Y);

            Vector2 textSize = Font.MeasureString(text) * AlignProgress;

            spriteBatch.DrawString(Font, text, pos - textSize / 72 * FontSize, baseColor, rotation,relativeOrigin,Vector2.One/72*FontSize,SpriteEffects.None,0);

            base.Draw(gameTime, spriteBatch);

        }
    }
}
