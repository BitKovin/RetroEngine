using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RetroEngine.Map;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Audio;

namespace RetroEngine.MapParser
{
    public class MapParser
    {
        static EntityData currentEntity;
        static BrushData currentBrush;
        static MapData mapData;

        public static MapData ParseMap(string path)
        {
            mapData = new MapData();

            path = AssetRegistry.FindPathForFile(path);

            mapData.Path = path;

            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    line = line.Replace("\"", "");

                    string[] parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (line.StartsWith("// entity"))
                    {
                        currentEntity = new EntityData();
                        currentEntity.name = parts[2];
                    }
                    else if (line.StartsWith("classname"))
                    {
                        currentEntity.Classname = parts[1];
                    }
                    else if (line.StartsWith("// brush"))
                    {
                        currentBrush = new BrushData();
                        currentBrush.Name = parts[2];
                    }
                    else if (line.StartsWith("("))
                    {

                    }
                    else if (line.StartsWith("}"))
                    {
                        if (currentBrush is not null)
                        {
                            FinishBrush();
                            currentBrush = null;
                        }
                        else if (currentEntity is not null)
                        {
                            FinishEntity();
                            currentEntity = null;
                        }
                    }
                    else if (line.StartsWith("//") || line.StartsWith("{"))
                    {
                        continue;
                    }
                    else if (parts.Length > 1)
                    {
                        currentEntity.Properties.Add(parts[0], line.Replace($"{parts[0]} ", ""));
                    }

                }


            }

            return mapData;
        }

        static void FinishEntity()
        {
            if (currentEntity != null)
            {
                mapData.Entities.Add(currentEntity);
            }

        }

        static void FinishBrush()
        {
            if (currentEntity != null)
                if (currentBrush != null)
                {
                    /*
                    currentBrush.Vertices = new List<Vector3>();

                    
                    var v = GetFaceVertices(currentBrush.Points);



                    foreach (var p in v)
                    {
                        currentBrush.faces.Add(p);
                    }
                    */
                    currentEntity.Brushes.Add(currentBrush);
                }

        }

        public static List<Vector3[]> GetFaceVertices(List<Vector3> points)
        {
            List<Vector3[]> faces = new List<Vector3[]>();

            List<Vector3> faceVertices = new List<Vector3>();

            if (points.Count % 3 != 0)
            {
                throw new ArgumentException("Invalid number of points. The points list should be a multiple of 3.");
            }

            List<Vector3> offsets = new List<Vector3>();
            List<Vector3> directions = new List<Vector3>();



            for (int i = 0; i < points.Count; i += 3)
            {

                //directions.Add(points[i + 1] - points[i]);

                Vector3 normal = CalculatePlaneNormal(points[i], points[i + 1], points[i + 2]);

                offsets.Add(points[i]);
                directions.Add(normal);

                offsets.Add(points[i]);
                directions.Add(normal * -1);

                //offsets.Add(points[i]);
                //directions.Add(points[i + 2] - points[i]);
            }
            Vector3 intersection = Vector3.Zero;
            for (int i = 0; i < offsets.Count; i++)
            {

                faceVertices.Add(offsets[i]);

                for (int j = 0; j < offsets.Count; j++)
                    for (int k = 0; k < offsets.Count; k++)
                    {
                        Vector3 intersectionOld = intersection;

                        //var intersections = GetPlaneContacts(offsets[i], directions[i], offsets[j], directions[j]);

                        //foreach (var intersecion in intersections)
                        //faceVertices.Add(intersecion);


                        if (GetPlaneContacts(offsets[i], directions[i], offsets[j], directions[j], offsets[k], directions[k], out intersection))
                        {
                            if (!faceVertices.Contains(intersection))
                                faceVertices.Add(intersection);
                        }
                    }


                if (i % 2 == 1)
                {
                    faces.Add(faceVertices.ToArray());
                    faceVertices.Clear();
                }

            }

            return faces;
        }

        public static Vector3 CalculatePlaneNormal(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            // Calculate two vectors lying on the plane
            Vector3 vector1 = point2 - point1;
            Vector3 vector2 = point3 - point1;

            // Calculate the cross product of the two vectors to find the plane's normal
            Vector3 normal = Vector3.Cross(vector1, vector2);
            normal.Normalize();

            return normal;
        }

        public static List<Vector3> CreateTriangulatedCubeVertices(float size)
        {
            // Define the half-size of the cube
            float halfSize = size / 2.0f;

            // Define the vertices for a cube
            Vector3[] vertices = new Vector3[]
            {
        // Front face
        new Vector3(-halfSize, halfSize, halfSize),
        new Vector3(halfSize, halfSize, halfSize),
        new Vector3(halfSize, -halfSize, halfSize),
        new Vector3(-halfSize, -halfSize, halfSize),

        // Back face
        new Vector3(-halfSize, halfSize, -halfSize),
        new Vector3(halfSize, halfSize, -halfSize),
        new Vector3(halfSize, -halfSize, -halfSize),
        new Vector3(-halfSize, -halfSize, -halfSize),
            };

            // Create the vertex list for the triangulated cube
            List<Vector3> cubeVertices = new List<Vector3>();

            // Define the indices for the triangles
            int[] indices = new int[]
            {
        0, 1, 2, 2, 3, 0, // Front face
        4, 5, 6, 6, 7, 4, // Back face
        0, 4, 7, 7, 3, 0, // Left face
        1, 5, 6, 6, 2, 1, // Right face
        0, 1, 5, 5, 4, 0, // Top face
        2, 3, 7, 7, 6, 2, // Bottom face
            };

            foreach (int index in indices)
            {
                cubeVertices.Add(vertices[index]);
            }

            return cubeVertices;
        }

        public static bool GetPlaneContacts(
    Vector3 Offset1, Vector3 Normal1,
    Vector3 Offset2, Vector3 Normal2,
    Vector3 Offset3, Vector3 Normal3,
    out Vector3 intersectionPoint)
        {
            // Calculate the cross products of pairs of normal vectors
            Vector3 cross1_2 = Vector3.Cross(Normal1, Normal2);
            Vector3 cross2_3 = Vector3.Cross(Normal2, Normal3);
            Vector3 cross3_1 = Vector3.Cross(Normal3, Normal1);

            // Calculate the determinant of the 3x3 matrix formed by the normal vectors
            float determinant = Vector3.Dot(Normal1, cross2_3);

            // Check if the determinant is close to zero (planes are parallel)
            if (Math.Abs(determinant) < 1e-6f)
            {
                intersectionPoint = Vector3.Zero;
                return false; // No unique intersection point
            }

            // Calculate the intersection point by solving a system of linear equations
            Vector3 distance = Offset2 - Offset1;
            float t1 = Vector3.Dot(distance, cross2_3) / determinant;
            intersectionPoint = Offset1 + t1 * Normal1;

            return true;
        }

        public static bool GetIntersection(Vector3 v1Offset, Vector3 v1Direction, Vector3 v2Offset, Vector3 v2Direction, out Vector3 intersection)
        {
            Vector3 cross = Vector3.Cross(v1Direction, v2Direction);

            if (Vector3.Distance(v1Offset, v2Offset) < 1) { intersection = Vector3.Zero; return false; }

            // Check if the lines are parallel (cross product magnitude is close to zero).
            if (cross.Length() < float.Epsilon)
            {
                intersection = Vector3.Zero; // No intersection, lines are parallel.
                return false;
            }

            Vector3 diff = v2Offset - v1Offset;
            float t1 = Vector3.Dot(diff, Vector3.Cross(v2Direction, cross)) / cross.LengthSquared();
            intersection = v1Offset + v1Direction * t1;
            return true;
        }

    }
}

