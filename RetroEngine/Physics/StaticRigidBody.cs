using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine
{
    internal class StaticRigidBody : RigidBody
    {

        RigidBody parrent;

        public StaticRigidBody(RigidBodyConstructionInfo constructionInfo) : base(constructionInfo)
        {
        }

        public void SetParrent(RigidBody parrent)
        {
            this.parrent = parrent;
        }

        public void UpdateFromParrent()
        {
            CollisionFlags = parrent.CollisionFlags;
        }


    }
}
