using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Light
{
    internal class LightVisibilityCheckMesh : StaticMesh
    {

        internal LightVisibilityCheckMesh() 
        {
            //Transperent = true;
        }

        public bool IsVisible()
        {
            return occluded == false && inFrustrum;
        }

        public override void DrawShadow(bool closeShadow = false, bool veryClose = false)
        {

        }

        public override Vector3 GetClosestToCameraPosition()
        {
            return Camera.rotation.GetForwardVector()*10000000 + Camera.position;
        }

        public override void DrawUnified()
        {

        }

    }
}
