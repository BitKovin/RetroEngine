using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using RetroEngine.SaveSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.ParticleSystem
{

    /// <summary>
    /// Emitting must always stay "true" on every emitter. Otherwise emitter will be destroyed
    /// </summary>
    /// 

    public class GlobalParticleSystem : ParticleSystemEnt
    {


        static Dictionary<string, GlobalParticleSystem> existingSystems = new Dictionary<string, GlobalParticleSystem>();

        [JsonInclude]
        public string SystemName = "";

        public GlobalParticleSystem() : base() 
        {

        }


        static GlobalParticleSystem GetOrCreateGlobalSystem(string systemName)
        {
            lock (existingSystems)
            {
                if (existingSystems.ContainsKey(systemName))
                {
                    var sys = existingSystems[systemName];
                    if (sys.Destroyed == false)
                    {
                        return existingSystems[systemName];
                    }
                }


                return CreateGlobalSystem(systemName);

            }
        }

        static GlobalParticleSystem CreateGlobalSystem(string systemName)
        {
            var system = ParticleSystemEnt.Create(systemName);

            var globalSystem = system as GlobalParticleSystem;

            globalSystem.SystemName = systemName;

            globalSystem.SaveGame = true;

            if (existingSystems.ContainsKey(systemName))
            {


                existingSystems[systemName] = globalSystem;
            }
            else
            {
                existingSystems.TryAdd(systemName, globalSystem);
            }

            Logger.Log($"creating global system {systemName}");
            

            return globalSystem;

        }

        protected virtual void EmitAt(Vector3 position, Vector3 orientation, Vector3 Scale)
        {

        }

        public static void EmitAt(string systemName,Vector3 position, Vector3 orientation, Vector3 Scale)
        {
            var sys = GetOrCreateGlobalSystem(systemName);

            if (sys == null) return;

            sys.EmitAt(position, orientation, Scale);

        }

        public override void Start()
        {
            base.Start();

        }

        public override void Destroy()
        {
            base.Destroy();

            existingSystems.Remove(SystemName);

        }

        public override void LoadData(EntitySaveData Data)
        {
            base.LoadData(Data);

            if (SaveGame && SystemName != "")
            {
                if(existingSystems.ContainsKey(SystemName))
                {
                    existingSystems[SystemName].SaveGame = false;
                    existingSystems[SystemName].Destroy();
                    existingSystems.Remove(SystemName);
                }
                existingSystems.Add(SystemName, this);
            }

        }

    }
}
