using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Csg
{
    public static class CsgHelper
    {
        /// <summary>
        /// Converts a MonoGame mesh (VertexData and indices) into a Csg.NET solid.
        /// </summary>
        /// <param name="vertices">Array of VertexData from a MonoGame mesh.</param>
        /// <param name="indices">Array of indices defining the triangles in the mesh.</param>
        /// <returns>A Csg.NET Solid object.</returns>
        public static Solid ConvertMonoGameToCsg(VertexData[] vertices, int[] indices)
        {
            var polygons = new List<Polygon>();

            for (int i = 0; i < indices.Length; i += 3)
            {
                // Get indices for the triangle
                int index1 = indices[i];
                int index2 = indices[i + 1];
                int index3 = indices[i + 2];

                // Get the vertices for the triangle
                var v1 = vertices[index1];
                var v2 = vertices[index2];
                var v3 = vertices[index3];

                // Create CSG.NET vertices
                var csgVertex1 = new Vertex(
                    new Vector3D(v1.Position.X, v1.Position.Y, v1.Position.Z),
                    new Vector2D(v1.TextureCoordinate.X, v1.TextureCoordinate.Y)
                );

                var csgVertex2 = new Vertex(
                    new Vector3D(v2.Position.X, v2.Position.Y, v2.Position.Z),
                    new Vector2D(v2.TextureCoordinate.X, v2.TextureCoordinate.Y)
                );

                var csgVertex3 = new Vertex(
                    new Vector3D(v3.Position.X, v3.Position.Y, v3.Position.Z),
                    new Vector2D(v3.TextureCoordinate.X, v3.TextureCoordinate.Y)
                );

                // Create a polygon and add it to the list
                var polygon = new Polygon(new List<Vertex> { csgVertex1, csgVertex2, csgVertex3 });
                polygons.Add(polygon);
            }

            // Create and return a CSG solid
            return Solid.FromPolygons(polygons);
        }

        /// <summary>
        /// Converts a Csg.NET Solid into MonoGame mesh data (VertexData and indices).
        /// </summary>
        /// <param name="solid">The Csg.NET Solid to convert.</param>
        /// <returns>A tuple containing an array of VertexData and an array of indices.</returns>
        public static (VertexData[] Vertices, int[] Indices) ConvertCsgToMonoGameMesh(Solid solid)
        {
            var vertices = new List<VertexData>();
            var indices = new List<int>();


            foreach (var polygon in solid.Polygons)
            {
                var baseIndex = vertices.Count;

                // Calculate normal for the polygon using the first three vertices
                Vector3 normal = Vector3.Zero;
                if (polygon.Vertices.Count >= 3)
                {
                    var p0 = new Vector3((float)polygon.Vertices[0].Pos.X, (float)polygon.Vertices[0].Pos.Y, (float)polygon.Vertices[0].Pos.Z);
                    var p1 = new Vector3((float)polygon.Vertices[1].Pos.X, (float)polygon.Vertices[1].Pos.Y, (float)polygon.Vertices[1].Pos.Z);
                    var p2 = new Vector3((float)polygon.Vertices[2].Pos.X, (float)polygon.Vertices[2].Pos.Y, (float)polygon.Vertices[2].Pos.Z);

                    var edge1 = p1 - p0;
                    var edge2 = p2 - p0;
                    normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
                }

                foreach (var vertex in polygon.Vertices)
                {
                    // Add the vertex to the list
                    vertices.Add(new VertexData
                    {
                        Position = new Vector3((float)vertex.Pos.X, (float)vertex.Pos.Y, (float)vertex.Pos.Z),
                        Normal = normal, //Normal
                        TextureCoordinate = new Vector2((float)vertex.Tex.X, (float)vertex.Tex.Y), 
                        Tangent = Vector3.Zero,          // Tangent can be calculated if needed
                        BiTangent = Vector3.Zero,        // BiTangent can be calculated if needed
                        BlendIndices = Vector4.Zero,
                        BlendWeights = Vector4.Zero,
                        SmoothNormal = normal,
                        Color = Vector4.One
                    });
                }

                // Add triangle indices for the polygon
                for (int i = 2; i < polygon.Vertices.Count; i++)
                {
                    indices.Add(baseIndex);
                    indices.Add(baseIndex + i - 1);
                    indices.Add(baseIndex + i);
                }
            }

            return (vertices.ToArray(), indices.ToArray());
        }

        /// <summary>
        /// Converts a ModelMeshPart to a CSG.NET Solid.
        /// </summary>
        /// <param name="part">The ModelMeshPart to convert.</param>
        /// <returns>A CSG.NET Solid representing the geometry in the ModelMeshPart.</returns>
        public static Solid ToCsgSolid(this ModelMeshPart part)
        {
            // Retrieve vertex data
            var vertexBuffer = part.VertexBuffer;
            var vertexDeclaration = part.VertexBuffer.VertexDeclaration;
            var vertexStride = vertexDeclaration.VertexStride;

            var vertices = new VertexData[part.NumVertices];
            vertexBuffer.GetData(part.VertexOffset * vertexStride, vertices, 0, part.NumVertices, vertexStride);

            // Retrieve index data
            var indexBuffer = part.IndexBuffer;
            var indices = new int[part.PrimitiveCount * 3];
            indexBuffer.GetData(part.StartIndex * sizeof(int), indices, 0, part.PrimitiveCount * 3);

            // Convert to CSG.NET solid
            return ConvertMonoGameToCsg(vertices, indices);
        }


        public static Vector3D ToCsg(this Vector3 vector)
        {
            return new Vector3D(vector.X, vector.Y, vector.Z);
        }


    }
}
