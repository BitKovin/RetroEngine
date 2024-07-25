using BulletSharp;
using RetroEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assimp.Metadata;
using SharpFont.PostScript;
using RetroEngine.Entities.Light;
using MonoGame.Framework.Content.Pipeline.Builder;
using RetroEngine.PhysicsSystem;
using System.Globalization;
using SharpFont;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RetroEngine.Map
{
    public class MapData
    {
        public string Game { get; set; }
        public string Path { get; set; }
        public string Format { get; set; }
        public List<EntityData> Entities { get; set; } = new List<EntityData>();

        public static bool MergeBrushes = false;

        public static float UnitSize = 32;


        static List<BrushFaceMesh> loadedFaces = new List<BrushFaceMesh>();

        public Level GetLevel()
        {
            Level level = new Level();

            level.Start();

            GameMain.Instance.curentLevel = level;

            float i = 0;

            List<Vector3> pointLights = new List<Vector3>();

            foreach (EntityData ent in Entities)
            {

                PointLight entity = LevelObjectFactory.CreateByTechnicalName(ent.Classname) as PointLight;

                if (entity == null) continue;

                if (ent.Properties.ContainsKey("origin"))
                {
                    Vector3 position = ent.GetPropertyVectorPosition("origin");
                    entity.Position = position;
                }

                pointLights.AddUnique(entity.Position);

            }

            level.entityID = 0;

            foreach (EntityData ent in Entities)
            {

                Entity entity = LevelObjectFactory.CreateByTechnicalName(ent.Classname);

                float progress = 0.2f + (i / (float)Entities.Count) * 0.5f;

                LoadingScreen.Update(progress);

                if (ent.Brushes.Count > 0)
                {
                    if (entity is null)
                        entity = new Entity();



                    List<BrushFaceMesh> faces = new List<BrushFaceMesh>();

                    Dictionary<Vector3, List<BrushFaceMesh>> pointLightFaces = new Dictionary<Vector3, List<BrushFaceMesh>>();

                    foreach(Vector3 pos in pointLights)
                    {
                        pointLightFaces.TryAdd(pos, new List<BrushFaceMesh>());
                    }

                    pointLightFaces.TryAdd(Vector3.Zero, new List<BrushFaceMesh>());

                    foreach (BrushData brush in ent.Brushes)
                    {

                        var loadedFaces = BrushFaceMesh.GetMergedFacesFromPath(Path.Replace(".map", ".obj"), "entity" + ent.name + "_" + "brush" + brush.Name);

                        foreach (var face in loadedFaces)
                        {
                            var shape = Physics.CreateCollisionShapeFromModel(face.model, shapeData: new Physics.CollisionShapeData { surfaceType = face.textureName }, complex: entity.ConvexBrush == false);
                            RigidBody rigidBody = Physics.CreateFromShape(entity, Vector3.One.ToPhysics(), shape, collisionFlags: BulletSharp.CollisionFlags.StaticObject, bodyType: PhysicsSystem.BodyType.World);
                            rigidBody.SetCollisionMask(BodyType.GroupAll);
                            entity.bodies.Add(rigidBody);
                        }

                        foreach (var face in loadedFaces)
                        {
                            faces.Add(face);
                            face.PreloadTextures();
                        }

                    }

                    var poses = pointLightFaces.Keys.ToArray();

                    foreach(var face in faces)
                    {

                        if (poses.Length == 0)
                        {
                            pointLightFaces[Vector3.Zero].Add(face);
                            continue;
                        }



                        Vector3 facePos = face.avgVertexPosition;

                        Vector3 closestPos;

                        poses = poses.OrderBy(p => Vector3.Distance(facePos, p)).ToArray();

                        
                        closestPos = poses[0];

                        pointLightFaces[closestPos].Add(face);


                    }

                    if (MergeBrushes || entity.mergeBrushes)
                    {
                        foreach (var key in pointLightFaces.Keys)
                        {

                            var faceList = pointLightFaces[key];

                            var mergedResult = BrushFaceMesh.MergeFaceMeshes(faceList);

                            entity.meshes.AddRange(mergedResult);

                            loadedFaces.AddRange(mergedResult);

                        }


                        foreach (BrushFaceMesh brushFaceMesh in faces)
                            StaticMesh.UnloadModel(brushFaceMesh.model);


                    }
                    else
                    {
                        entity.meshes.AddRange(faces);
                    }

                    Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)> positionToNormals = new Dictionary<Vector3, (Vector3 accumulatedNormal, List<Vector3> existingNormals)>();

                    foreach (var face in entity.meshes)
                        face.AddNormalsToPositionNormalDictionary(ref positionToNormals);

                    foreach (var face in entity.meshes)
                        face.GenerateSmoothNormalsFromDictionary(positionToNormals);

                    level.AddEntity(entity);
                    entity.FromData(ent);
                }
                else
                {
                    if (entity is null)
                    {
                        entity = new Entity();
                    }

                    if (ent.Properties.ContainsKey("origin"))
                    {
                        Vector3 position = ent.GetPropertyVectorPosition("origin");
                        entity.Position = position;
                    }

                    

                    entity.FromData(ent);

                    level.AddEntity(entity);
                    //entity.Start();
                }

                i++;

            }

            //level.Start();

            return level;
        }

        public static void ClearMeshData()
        {
            foreach(var face in loadedFaces)
            {
                if (face == null) continue;
                if(face.model ==  null) continue;

                StaticMesh.UnloadModel(face.model);

            }
        }

        public EntityData GetEntityDataFromClass(string className)
        {
            foreach (EntityData ent in Entities)
            {
                if(ent.Classname == className)
                    return ent;
            }

            return null;
        }

    }

   

    public class EntityData
    {
        public string Classname { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        public List<BrushData> Brushes { get; set; } = new List<BrushData>();

        public string name { get; set; }

        public Vector3 GetPropertyVectorPosition(string name)
        {
            try
            {
                string[] parts = Properties[name].Split(" ");

                return new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture)*-1) / MapData.UnitSize;
            }catch(Exception)
            {
                return Vector3.Zero;
            }
        }

        public Vector3 GetPropertyVectorRotation(string name)
        {
            try
            {
                string[] parts = Properties[name].Split(" ");

                return new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture) , float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));
            }
            catch (Exception)
            {
                return Vector3.Zero;
            }
        }

        public Vector3 GetPropertyVector(string name, Vector3 def)
        {
            try
            {
                string[] parts = Properties[name].Split(" ");

                return new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture), float.Parse(parts[2], CultureInfo.InvariantCulture));
            }catch(Exception)
            {
                return def;
            }
        }

        public float GetPropertyFloat(string name, float defaultValue = 0)
        {
            try
            {

                string[] parts = Properties[name].Split(" ");

                return float.Parse(parts[0], CultureInfo.InvariantCulture);
            }catch (Exception)
            {
                return defaultValue;
            }
        }

        public bool GetPropertyBool(string name, bool defaultValue = false)
        {
            try
            {

                string[] parts = Properties[name].Split(" ");

                return bool.Parse(parts[0].Replace("0", "false").Replace("1", "true").ToLower());
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public string GetPropertyString(string name, string defaultValue = "")
        {
            if(Properties.ContainsKey(name))
                return Properties[name];
            return defaultValue;
        }

        public static Vector3 ConvertRotation(Vector3 importRot)
        {

            importRot = (importRot + new Vector3(0, 180, 0)).NormalizeAngles().NonZeroAngles() / 180 * (float)Math.PI;

            Matrix rotM = Matrix.CreateRotationX(-importRot.Z) *
                            Matrix.CreateRotationZ(importRot.X) *
                            Matrix.CreateRotationY(importRot.Y);

            Vector3 rotation = rotM.DecomposeMatrix().Rotation;

            return rotation;
        }

    }

    public class BrushData
    {
        public string Texture { get; set; }

        public float TextureScale = 1;
        public List<float> TextureCoordinates { get; set; } = new List<float>();
        public string Name { get; set; }
    }
}
