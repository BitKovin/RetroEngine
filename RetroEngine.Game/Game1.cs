using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine.Entities;

namespace RetroEngine.Game
{
    public class Game : Engine.GameMain
    {
        protected override void LoadContent()
        {
            base.LoadContent();

            curentLevel.entities.Add(new Player());

            Box box = new Box();
            box.Position = new Vector3(0, -2, 0);
            curentLevel.entities.Add(box);

            box.Start();
        }
    }
}