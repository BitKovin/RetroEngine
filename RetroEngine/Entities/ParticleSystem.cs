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

        public float ParticleSizeMultiplier = 1;

        public ParticleSystem() { }

        public override void Start()
        {
            base.Start();

            foreach (var emitter in emitters)
            {
                emitter.Position = Position;
                emitter.Start();
            }

            updated = true;
        }

        public static void Preload(string systemName)
        {


            if (GameMain.CanLoadAssetsOnThisThread() == false)
            {
                Logger.Log("can't preload particle system on this thread");
                return;
            }

            var sys = ParticleSystemFactory.CreateByTechnicalName(systemName);

            if (sys == null)
            {
                Logger.Log($"ERRROR: failed to load system {sys}");
                return;
            }

            sys.LoadAssetsIfNeeded();

            sys.Start();
            sys.Update();
            sys.VisualUpdate();
            sys.AsyncUpdate();
            foreach(var mesh in sys.meshes)
            {
                mesh.RenderPreparation();
            }
            sys.LoadAssetsIfNeeded();
            sys.AsyncUpdate();
            sys.Destroy();
        }

        public override void LateUpdate()
        {
            base.LateUpdate();

        }

        public virtual void StopAll()
        {
            foreach (var emitter in emitters)
            {
                emitter.Emitting = false;
                emitter.SpawnRate = 0;
            }
        }

        public override void Update()
        {
            base.Update();

            if (emitters.Count == 0)
                Destroy();

        }
        bool updated = false;

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();


            List<ParticleEmitter> list = new List<ParticleEmitter>(emitters);

            lock(list)
            lock (emitters)
            {
                foreach (var emitter in list)
                {
                    if (emitter.destroyed)
                        emitters.Remove(emitter);

                    emitter.Position = Position;
                    emitter.Rotation = Rotation;
                    emitter.ParticleSizeMultiplier = ParticleSizeMultiplier;
                    emitter.Update();
                }
            }

            updated = true;

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            lock (emitters)
            {
                foreach (var emitter in emitters)
                {
                    emitter.LoadAssets();
                }
            }

            foreach (var emitter in emitters)
            {
                meshes.Add(emitter);
            }

        }

        public static ParticleSystem Create(string name)
        {
            ParticleSystem system = ParticleSystemFactory.CreateByTechnicalName(name);
            Level.GetCurrent().AddEntity(system);
            return system;
        }

        public virtual void SetTrailTransform(Vector3 p1, Vector3 p2)
        {
            Position = (p1 + p2)/2;

            Rotation = MathHelper.FindLookAtRotation(p1, p2);

            ParticleSizeMultiplier = Vector3.Distance(p1,p2);


        }

        public override void Destroy()
        {
            base.Destroy();

            foreach(var emitter in emitters)
                emitter.Destroy();

        }

    }
}
