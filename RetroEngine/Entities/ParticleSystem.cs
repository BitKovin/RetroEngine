using RetroEngine.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    public class ParticleSystem : Entity
    {

        public List <ParticleEmitter> emitters = new List <ParticleEmitter>();

        public ParticleSystem() { }

        public override void Start()
        {
            base.Start();

            foreach (var emitter in emitters)
            {
                emitter.Start();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();


            foreach (var emitter in emitters)
            {
                emitter.Position = Position;
                emitter.Update();
            }
        }

    }
}
