using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Microsoft.Xna.Framework;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.NavigationSystem
{
    public class NavigationQueryFilterAvoidLOS : NavigationQueryFilter
    {

        public float LOSAvoidanceStrength = 1;

        public override float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef, DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            float baseCost = base.GetCost(pa, pb, prevRef, prevTile, prevPoly, curRef, curTile, curPoly, nextRef, nextTile, nextPoly);

            float additionalCost = 1f;

            Vector3 polyPos = RecastHelper.GetAvgVertexPosition(curTile, curPoly);
            Vector3 toPolyDir = (polyPos - Camera.position).Normalized();

            // Line trace to check if the poly is in sight
            var hit = Physics.LineTraceForStatic(polyPos.ToPhysics(), Camera.position.ToPhysics());

            // Modify the cost based on LOS and the dot product
            if (hit.HasHit == false)
            {
                // In direct LOS, apply stronger penalty

                additionalCost *= Vector3.Dot(Camera.Forward.XZ().Normalized(), toPolyDir.XZ().Normalized()) + 0.4f;

            }

            // Add the additional cost to the base cost
            return baseCost * additionalCost;
        }

    }
}
