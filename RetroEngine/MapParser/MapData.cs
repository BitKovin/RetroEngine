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

                    CompoundShape shape = new CompoundShape();
                    foreach (BrushData brush in ent.Brushes)
                    {
                        foreach (var face in BrushFaceMesh.GetFacesFromPath(Path.Replace(".map", ".obj"), "entity" + ent.name + "_" + "brush" + brush.Name))
                        {

                            face.useAvgVertexPosition = true;

                            entity.meshes.Add(face);


                            shape.AddChildShape(System.Numerics.Matrix4x4.Identity, Physics.CreateCollisionShapeFromModel(face.model));


                        }
                    }

                    RigidBody rigidBody = Physics.CreateFromShape(entity, Vector3.One.ToPhysics(), shape, collisionFlags: BulletSharp.CollisionFlags.StaticObject);

                    entity.body = rigidBody;

                    level.entities.Add(entity);
                }
                else
                {
                    Entity entity = LevelObjectFactory.CreateByTechnicalName(ent.Classname) as Entity;
                    if (entity is null) continue;

                    if (ent.Properties.ContainsKey("origin"))
                    {
                        Vector3 position = ent.GetPropertyVector("origin");
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

        public Vector3 GetPropertyVector(string name)
        {
            string[] parts = Properties[name].Split(" ");

            return new Vector3(float.Parse(parts[0])*-1, float.Parse(parts[2]), float.Parse(parts[1])) / MapData.UnitSize;
        }

    }

    public class BrushData
    {
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        public List<Vector3> Points { get; set; } = new List<Vector3>();

        public List<Vector3[]> faces { get; set; } = new List<Vector3[]>();

        public string Texture { get; set; }

        public float TextureScale = 1;
        public List<float> TextureCoordinates { get; set; } = new List<float>();
        public string Name { get; set; }
    }
}
