using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class Collision
    {

        public Vector3 position;
        public Vector3 size;
        public Entity owner;

        const int Accuracy = 500;

        public static bool MakeCollionTest(Collision col1, Collision col2)
        {
            BoundingBox box1 = new BoundingBox(col1.position - (col1.size/2f), col1.position + (col1.size / 2f));
            BoundingBox box2 = new BoundingBox(col2.position - (col2.size / 2f), col2.position + (col2.size / 2f));
            return box1.Intersects(box2);
        }
    }
}
