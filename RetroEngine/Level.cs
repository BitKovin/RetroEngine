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

        }

        public virtual void Update()
        {
            foreach (Entity entity in entities)
                entity.Update();
        }

        public virtual void AsyncUpdate()
        {
            Parallel.ForEach(entities, entity =>
            {
                entity.AsyncUpdate();
            });
        }

        public virtual void LateUpdate()
        {
            foreach (Entity entity in entities)
                entity.LateUpdate();
        }


    }
}
