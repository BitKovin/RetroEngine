﻿namespace RetroEngine.Csg
{
    public class Vertex
    {
        /// <summary>
        /// The world position of this vertex.
        /// </summary>
        public readonly Vector3D Pos;

        /// <summary>
        /// The texture coordinate of this vertex.
        /// </summary>
		public readonly Vector2D Tex;

        int tag = 0;

        /// <summary>
        /// Initializes a new <see cref="T:Csg.Vertex"/> at a given position and with a given texture coordinate.
        /// </summary>
        /// <param name="pos">World position</param>
        /// <param name="tex">Texture coordinate</param>
        public Vertex(Vector3D pos, Vector2D tex)
        {
            Pos = pos;
			Tex = tex;
        }

        /// <summary>
        /// A unique id for this vertex.
        /// </summary>
        public int Tag
        {
            get
            {
                if (tag == 0)
                {
                    tag = Solid.GetTag();
                }
                return tag;
            }
        }

        /// <summary>
        /// Get a flipped version of this vertex.
        /// May return the same object if no changes are needed to flip.
        /// </summary>
        public Vertex Flipped()
        {
            return this;
        }

        public override string ToString() => Pos.ToString();

        /// <summary>
        /// Left multiplies the position of this vertex with the given matrix.
        /// The texture coordinate is unchanged.
        /// </summary>
        /// <returns>A new transformed vertex.</returns>
        /// <param name="matrix4x4">The transformation.</param>
        public Vertex Transform(Matrix4x4 matrix4x4)
        {
            var newpos = Pos * matrix4x4;
            return new Vertex(newpos, Tex);
        }
    }
}

