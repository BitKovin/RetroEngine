using DotRecast.Core.Numerics;
using DotRecast.Detour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.NavigationSystem
{

    using static DtDetour;
    public class NavigationQueryFilter : IDtQueryFilter
    {

        private readonly float[] m_areaCost = new float[DT_MAX_AREAS]; //< Cost per area type. (Used by default implementation.)
        private int m_includeFlags; //< Flags for polygons that can be visited. (Used by default implementation.) 
        private int m_excludeFlags; //< Flags for polygons that should not be visited. (Used by default implementation.) 

        public NavigationQueryFilter()
        {
            m_includeFlags = 0xffff;
            m_excludeFlags = 0;
            for (int i = 0; i < DT_MAX_AREAS; ++i)
            {
                m_areaCost[i] = 1.0f;
            }
        }

        public NavigationQueryFilter(int includeFlags, int excludeFlags, float[] areaCost)
        {
            m_includeFlags = includeFlags;
            m_excludeFlags = excludeFlags;
            for (int i = 0; i < Math.Min(DT_MAX_AREAS, areaCost.Length); ++i)
            {
                m_areaCost[i] = areaCost[i];
            }

            for (int i = areaCost.Length; i < DT_MAX_AREAS; ++i)
            {
                m_areaCost[i] = 1.0f;
            }
        }

        public virtual bool PassFilter(long refs, DtMeshTile tile, DtPoly poly)
        {
            return (poly.flags & m_includeFlags) != 0 && (poly.flags & m_excludeFlags) == 0;
        }

        public virtual float GetCost(RcVec3f pa, RcVec3f pb, long prevRef, DtMeshTile prevTile, DtPoly prevPoly, long curRef,
            DtMeshTile curTile, DtPoly curPoly, long nextRef, DtMeshTile nextTile, DtPoly nextPoly)
        {
            return RcVec3f.Distance(pa, pb) * m_areaCost[curPoly.GetArea()];
        }

        public int GetIncludeFlags()
        {
            return m_includeFlags;
        }

        public void SetIncludeFlags(int flags)
        {
            m_includeFlags = flags;
        }

        public int GetExcludeFlags()
        {
            return m_excludeFlags;
        }

        public void SetExcludeFlags(int flags)
        {
            m_excludeFlags = flags;
        }

    }
}
