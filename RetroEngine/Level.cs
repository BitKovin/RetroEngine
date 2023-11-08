using RetroEngine;
using RetroEngine.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Level
    {
        public List<Entity> entities;

        public Level()
        {
            entities = new List<Entity>();
        }

        public virtual void Start()
        {
            Physics.Start();
        }


        public virtual void Update()
        {
            Physics.Update();

            List<Entity> list = new List<Entity>(); 
            list.AddRange(entities);

            foreach (Entity entity in list)
                entity.Update();
        }

        public virtual void AsyncUpdate()
        {

            List<Entity> list = new List<Entity>();
            list.AddRange(entities);

            Parallel.ForEach(list, entity =>
            {
                entity.AsyncUpdate();
            });
        }

        public virtual void LateUpdate()
        {

            List<Entity> list = new List<Entity>();
            list.AddRange(entities);

            foreach (Entity entity in list)
                entity.LateUpdate();
        }

        public virtual void RenderPreparation()
        {
            
            Parallel.ForEach(entities, entity =>
            {
                foreach(StaticMesh mesh in entity.meshes)
                    mesh.RenderPreparation();
                
            });
        }

        public virtual List<StaticMesh> GetMeshesToRender()
        {
            List <StaticMesh> list = new List<StaticMesh >();

            foreach (Entity ent in entities)
            {
                if (ent.meshes is not null)
                    foreach (StaticMesh mesh in ent.meshes)
                        if(mesh.Transperent)
                            list.Add(mesh);
            }

            list = list.OrderByDescending(mesh => mesh.CalculatedCameraDistance).ToList();

            foreach (Entity ent in entities)
            {
                if (ent.meshes is not null)
                    foreach (StaticMesh mesh in ent.meshes)
                        if (mesh.Transperent == false)
                            list.Insert(0,mesh);
            }

            return list;
        }

    }
}
