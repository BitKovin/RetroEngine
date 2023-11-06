using RetroEngine.Physics;
using System;
using System.Collections.Generic;
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


    }
}
