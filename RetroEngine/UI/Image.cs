using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.UI
{
    public class Image : UiElement
    {

        Texture2D tex;

        public Color baseColor = Color.White;

        public Image() : base()
        {
            tex = AssetRegistry.LoadTextureFromFile("engine/textures/white.png", false, false);
        }


        public void SetTexture(string path)
        {
            tex = AssetRegistry.LoadTextureFromFile(path);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Rectangle mainRectangle = new Rectangle();
            mainRectangle.Location = new Point((int)position.X + (int)offset.X, (int)position.Y + (int)offset.Y);
            mainRectangle.Size = new Point((int)size.X, (int)size.Y);


            spriteBatch.Draw(tex, mainRectangle, baseColor);

            base.Draw(gameTime, spriteBatch);

        }

    }
}
