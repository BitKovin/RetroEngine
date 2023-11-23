using RetroEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Effects.Particles
{
    [ParticleSystem("hitSmoke")]
    public class particle_system_hitSmoke : ParticleSystem
    {

        public particle_system_hitSmoke()
        {
            emitters.Add(new particle_hitSmoke());
        }

    }
}
