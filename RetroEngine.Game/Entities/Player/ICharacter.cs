using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Player
{
    internal interface ICharacter
    {
        public abstract float GetHealth();

        public abstract void SetHealth(float health);
        public abstract RigidBody GetPhysicsBody();

        public abstract bool isFirstPerson();

        public abstract SkeletalMesh GetSkeletalMesh();

    }
}
