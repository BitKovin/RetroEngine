﻿using Assimp;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RetroEngine.Localization;

namespace RetroEngine.UI
{
    public class UiText : UiElement
    {
        public SpriteFont Font;

        public Text text = new Text();

        public float FontSize = 24f;

        public Vector2 AlignProgress = new Vector2();

        public Color baseColor = Color.White;

        public UiText() : base()
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
            return Font.MeasureString(text.ToString()) / 72 * FontSize;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 pos = new Vector2(position.X + offset.X, position.Y + offset.Y);

            string text = this.text.ToString();

            //Vector2 textSize = Font.MeasureString(text) * AlignProgress;

            spriteBatch.DrawString(Font, text, pos, baseColor, rotation,relativeOrigin,Vector2.One/72*FontSize,SpriteEffects.None,0);

            base.Draw(gameTime, spriteBatch);

        }
    }
}