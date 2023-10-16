using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class Collision2D
    {

        public Vector2 position;
        public Point size;
        public Entity owner;

        const int Accuracy = 500;

        public static bool MakeCollionTest(Collision2D col1, Collision2D col2)
        {
            Point pos1 = new Point((int)(col1.position.X * Accuracy), (int)(col1.position.Y * Accuracy));
            Point pos2 = new Point((int)(col2.position.X * Accuracy), (int)(col2.position.Y * Accuracy));
            Rectangle Col1 = new Rectangle(new Point(pos1.X, pos1.Y), new Point(col1.size.X * Accuracy, col1.size.Y * Accuracy));
            Rectangle Col2 = new Rectangle(new Point(pos2.X, pos2.Y), new Point(col2.size.X * Accuracy, col2.size.Y * Accuracy));

            return Col1.Intersects(Col2);

        }
    }
}
