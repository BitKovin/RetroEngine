using BulletSharp;
using RetroEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Map
{
    public class MapData
    {
        public string Game { get; set; }
        public string Path { get; set; }
        public string Format { get; set; }
        public List<EntityData> Entities { get; set; } = new List<EntityData>();

        public bool MergeBrushes = true;

        public static float UnitSize = 32;

        public Level GetLevel()
        {
            Level level = new Level();

            level.Start();


            foreach(EntityData ent in Entities)
            {
                if (ent.Brushes.Count > 0)
                {
                    Entity entity = LevelObjectFactory.CreateByTechnicalName(ent.Classname) as Entity;

                    if (entity is null)
                        entity = new Entity();

                    entity.name = ent.Classname;

                    List<BrushFaceMesh> faces = new List<BrushFaceMesh>();

                    foreach (BrushData brush in ent.Brushes)
                    {
                        foreach (var face in BrushFaceMesh.GetFacesFromPath(Path.Replace(".map", ".obj"), "entity" + ent.name + "_" + "brush" + brush.Name))
                        {
                            var shape = Physics.CreateCollisionShapeFromModel(face.model, shapeData: new Physics.CollisionShapeData { surfaceType = face.textureName });
                            RigidBody rigidBody = Physics.CreateFromShape(entity, Vector3.One.ToPhysics(), shape, collisionFlags: BulletSharp.CollisionFlags.StaticObject);
                            entity.bodies.Add(rigidBody);
                        }

                        foreach (var face in BrushFaceMesh.GetMergedFacesFromPath(Path.Replace(".map", ".obj"), "entity" + ent.name + "_" + "brush" + brush.Name))
                        {
                            faces.Add(face);
                            face.PreloadTextures();
                        }

                    }

                    if (MergeBrushes)
                    {
                        entity.meshes.AddRange(BrushFaceMesh.MergeFaceMeshes(faces));

                        foreach (BrushFaceMesh brushFaceMesh in faces)
                            StaticMesh.UnloadModel(brushFaceMesh.model);

                    }
                    else
                        entity.meshes.AddRange(faces);


                    level.entities.Add(entity);
                    entity.FromData(ent);
                }
                else
                {
                    Entity entity = LevelObjectFactory.CreateByTechnicalName(ent.Classname) as Entity;
                    if (entity is null) continue;

                    if (ent.Properties.ContainsKey("origin"))
                    {
                        Vector3 position = ent.GetPropertyVectorPosition("origin");
                        entity.Position = position;
                    }

                    entity.FromData(ent);

                    level.entities.Add(entity);
                    //entity.Start();
                }

            }

            //level.Start();

            return level;
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
                string[] parts = Properties[name].Replace(".",",").Split(" ");

                return new Vector3(float.Parse(parts[0]) * -1, float.Parse(parts[2]), float.Parse(parts[1])) / MapData.UnitSize;
            }catch(Exception)
            {
                return Vector3.Zero;
            }
        }

        public Vector3 GetPropertyVectorRotation(string name)
        {
            try
            {
                string[] parts = Properties[name].Replace(".", ",").Split(" ");

                return new Vector3(float.Parse(parts[0]) , float.Parse(parts[1]), float.Parse(parts[2]));
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
                string[] parts = Properties[name].Replace(".", ",").Split(" ");

                return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
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

                return float.Parse(parts[0].Replace(".",","));
            }catch (Exception)
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

    }

    public class BrushData
    {
        public string Texture { get; set; }

        public float TextureScale = 1;
        public List<float> TextureCoordinates { get; set; } = new List<float>();
        public string Name { get; set; }
    }
}
