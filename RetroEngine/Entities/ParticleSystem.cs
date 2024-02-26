using Microsoft.Xna.Framework;
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
                emitter.Position = Position;
                emitter.Start();
                meshes.Add(emitter);
            }
        }

        public static void Preload(string systemName)
        {
            var sys = ParticleSystemFactory.CreateByTechnicalName(systemName);

            if (sys == null) return;

            sys.LoadAssetsIfNeeded();

            sys.Start();

            sys.Destroy();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

            List<ParticleEmitter> list = new List <ParticleEmitter>(emitters);

            foreach (var emitter in list)
            {
                if(emitter.Destroyed)
                    emitters.Remove(emitter);

                emitter.Position = Position;
                emitter.Update();
            }

            if (emitters.Count == 0)
                Destroy();

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            foreach (var emitter in emitters)
            {
                emitter.LoadAssets();
            }
        }

        public static ParticleSystem Create(string name)
        {
            ParticleSystem system = ParticleSystemFactory.CreateByTechnicalName(name);
            Level.GetCurrent().AddEntity(system);
            return system;
        }

    }
}
