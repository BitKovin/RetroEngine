using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Recast.Toolset.Tools;
using Microsoft.Xna.Framework;
using RetroEngine.NavigationSystem;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{

    [LevelObject("testRecast")]
    public class testRecast : Entity
    {

        public override void Update()
        {
            base.Update();


            if(Input.GetAction("test").Pressed())
                FindPath();


            if (Input.GetAction("test2").Pressed())
            {
                
                var hit = Physics.LineTrace(Camera.position.ToPhysics(), (Camera.position + Camera.Forward*100).ToPhysics(), null, bodyType: BodyType.World | BodyType.MainBody);

                Position = hit.HitPointWorld;

                DrawDebug.Sphere(0.2f, Position, Vector3.UnitX);

            }

            if (Input.GetAction("test3").Pressed())
            {

                var hit = Physics.LineTrace(Camera.position.ToPhysics(), (Camera.position + Camera.Forward * 100).ToPhysics(), null, bodyType: BodyType.World | BodyType.MainBody);

                NavigationSystem.Recast.AddObstacleCapsule(hit.HitPointWorld, 2, 2);



                DrawDebug.Sphere(2, hit.HitPointWorld, Vector3.UnitZ, 500);

            }


        }

        void FindPath()
        {
            DtNavMeshQuery navMeshQuery = new DtNavMeshQuery(NavigationSystem.Recast.dtNavMesh);


            IDtQueryFilter filter = new DtQueryDefaultFilter();

            List<long> longs = new List<long>();

            RcTestNavMeshTool rcTestNavMeshTool = new RcTestNavMeshTool();

            long startRef = 0;
            long endRef = 0;



            RcVec3f m_polyPickExt = new RcVec3f(2, 4, 2);


    
            Vector3 startPos = Position;
            Vector3 endPos = Camera.position;

            navMeshQuery.FindNearestPoly(startPos.ToRc(), m_polyPickExt, filter, out startRef, out var _, out var _);
            navMeshQuery.FindNearestPoly(endPos.ToRc(), m_polyPickExt, filter, out endRef, out var _, out var _);

            List<RcVec3f> path = new List<RcVec3f>();

            rcTestNavMeshTool.FindFollowPath(NavigationSystem.Recast.dtNavMesh, navMeshQuery, startRef, endRef, startPos.ToRc(), endPos.ToRc(), filter, true, ref longs, 0, ref path);

            DrawDebug.Path(path.ConvertPath(), Vector3.UnitZ, 2);

        }

    }
}
