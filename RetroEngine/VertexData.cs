using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexData : IVertexType
    {
        public Vector3 Position;

        public Vector3 Normal;

        public Vector2 TextureCoordinate;

        public Vector3 Tangent;

        public Vector2 bone1 = new Vector2(0);
        public Vector2 bone2 = new Vector2(0);
        public Vector2 bone3 = new Vector2(0);
        public Vector2 bone4 = new Vector2(0);

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexData(Vector3 position, Vector3 normal, Vector2 textureCoordinate, Vector3 tangent)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Tangent = tangent;
        }

        public override int GetHashCode()
        {
            return (((Position.GetHashCode() * 397) ^ Normal.GetHashCode()) * 397) ^ TextureCoordinate.GetHashCode() ^ bone1.GetHashCode() ^ bone2.GetHashCode() ^ bone3.GetHashCode() ^ bone4.GetHashCode();
        }

        public override string ToString()
        {
            string[] obj = new string[7] { "{{Position:", null, null, null, null, null, null };
            Vector3 position = Position;
            obj[1] = position.ToString();
            obj[2] = " Normal:";
            position = Normal;
            obj[3] = position.ToString();
            obj[4] = " TextureCoordinate:";
            Vector2 textureCoordinate = TextureCoordinate;
            obj[5] = textureCoordinate.ToString();
            obj[6] = "}}";
            return string.Concat(obj);
        }

        public static bool operator ==(VertexData left, VertexData right)
        {
            if (left.Position == right.Position && left.Normal == right.Normal)
            {
                return left.TextureCoordinate == right.TextureCoordinate;
            }

            return false;
        }

        public static bool operator !=(VertexData left, VertexData right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return this == (VertexData)obj;
        }

        static VertexData()
        {
            VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
                new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), 
                new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0), 
                new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0), 
                new VertexElement(44, VertexElementFormat.Vector2, VertexElementUsage.Position, 1), 
                new VertexElement(52, VertexElementFormat.Vector2, VertexElementUsage.Position, 2), 
                new VertexElement(60, VertexElementFormat.Vector2, VertexElementUsage.Position, 3), 
                new VertexElement(68, VertexElementFormat.Vector2, VertexElementUsage.Position, 4));
        }

        public struct VertexElementByteOffset
        {
            public static int currentByteSize = 0;
            //[STAThread]
            public static int PositionStartOffset() { currentByteSize = 0; var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
            public static int Offset(int n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
            public static int Offset(float n) { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
            public static int Offset(Vector2 n) { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
            public static int Offset(Color n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
            public static int Offset(Vector3 n) { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
            public static int Offset(Vector4 n) { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }

            public static int OffsetInt() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
            public static int OffsetFloat() { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
            public static int OffsetColor() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
            public static int OffsetVector2() { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
            public static int OffsetVector3() { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
            public static int OffsetVector4() { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }
        }

    }
}
