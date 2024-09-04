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

        public float LOSAvoidanceStrength = 2;

        public override float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef, DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {

            float baseCost = base.GetCost(pa, pb, prevRef, prevTile, prevPoly, curRef, curTile, curPoly, nextRef, nextTile, nextPoly);

            float additionalCoast = 0;

            Vector3 polyPos = RecastHelper.GetAvgVertexPositions(curTile, curPoly);

            

            var hit = Physics.LineTraceForStatic(polyPos.ToPhysics(), Camera.position.ToPhysics());

            if(hit.HasHit == false)
            {
                additionalCoast += LOSAvoidanceStrength; //Vector3.Distance(hit.HitPointWorld, Camera.position);

                additionalCoast *= Vector3.Dot(Camera.Forward, (polyPos - Camera.position).Normalized()) + 1;
                additionalCoast /= 2;

            }
            else
            {
                additionalCoast = (Vector3.Dot(Camera.Forward, (polyPos - Camera.position).Normalized()) + 1)/5 * LOSAvoidanceStrength;
            }

            //DrawDebug.Text(polyPos, (baseCost + additionalCoast).ToString(), 0.1f);

            return baseCost * additionalCoast;
        }

    }
}
