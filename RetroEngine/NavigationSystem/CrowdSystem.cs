using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Crowd;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.NavigationSystem
{
    public static class CrowdSystem
    {

        private static DtCrowd dtCrowd;

        private static Dictionary<Entity, DtCrowdAgent> entToAgent = new Dictionary<Entity, DtCrowdAgent>();

        private static DtCrowdAgentConfig _agCfg;

        internal static void Init()
        {

            _agCfg = new DtCrowdAgentConfig();

            entToAgent.Clear();

            DtCrowdConfig dtCrowdConfig = new DtCrowdConfig(1);

            dtCrowd = new DtCrowd(dtCrowdConfig, Recast.dtNavMesh);

            // Setup local avoidance option to different qualities.
            // Use mostly default settings, copy from dtCrowd.
            DtObstacleAvoidanceParams option = new DtObstacleAvoidanceParams(dtCrowd.GetObstacleAvoidanceParams(0));

            // Low (11)
            option.velBias = 0.5f;
            option.adaptiveDivs = 5;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 1;
            dtCrowd.SetObstacleAvoidanceParams(0, option);


            // Medium (22)
            option.velBias = 0.5f;
            option.adaptiveDivs = 5;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 2;
            dtCrowd.SetObstacleAvoidanceParams(1, option);

            // Good (45)
            option.velBias = 0.5f;
            option.adaptiveDivs = 7;
            option.adaptiveRings = 2;
            option.adaptiveDepth = 3;
            dtCrowd.SetObstacleAvoidanceParams(2, option);

            // High (66)
            option.velBias = 0.5f;
            option.adaptiveDivs = 7;
            option.adaptiveRings = 3;
            option.adaptiveDepth = 3;

            dtCrowd.SetObstacleAvoidanceParams(3, option);


        }

        public static DtCrowdAgent CreateAgent(Entity entity, Vector3 position, float radius = 0.5f, float height = 2, float speed = 5, float aceleration = 10)
        {

            DtCrowdAgentParams agentParams = new DtCrowdAgentParams();
            agentParams.height = height;
            agentParams.radius = radius;
            agentParams.maxSpeed = speed;
            agentParams.maxAcceleration = aceleration;

            agentParams.collisionQueryRange = radius * 3;
            agentParams.updateFlags = _agCfg.GetUpdateFlags();
            agentParams.pathOptimizationRange = radius * 40;
            agentParams.obstacleAvoidanceType = _agCfg.obstacleAvoidanceType;
            agentParams.separationWeight = _agCfg.separationWeight;

            agentParams.queryFilterType

            var agent = dtCrowd.AddAgent(position.ToRc(), agentParams);

            entToAgent.Remove(entity);
            entToAgent.Add(entity, agent);

            return agent;

        }

        public static void RemoveAgent(DtCrowdAgent agent)
        {
            dtCrowd.RemoveAgent(agent);
        }

        public static void SetAgentTargetPosition(this DtCrowdAgent agent, Vector3 position)
        {

            DtNavMeshQuery navquery = dtCrowd.GetNavMeshQuery();
            IDtQueryFilter filter = dtCrowd.GetFilter(0);
            RcVec3f halfExtents = dtCrowd.GetQueryExtents();

            navquery.FindNearestPoly(position.ToRc(), halfExtents, filter, out var _moveTargetRef, out var _moveTargetPos, out var _);

            dtCrowd.RequestMoveTarget(agent, _moveTargetRef, _moveTargetPos);
        }

        internal static void Update()
        {
            lock (dtCrowd)
            {
                dtCrowd.Update(Time.DeltaTime, null);
            }
        }

        private static RcVec3f CalcVel(RcVec3f pos, RcVec3f tgt, float speed)
        {
            RcVec3f vel = RcVec3f.Subtract(tgt, pos);
            vel.Y = 0.0f;
            vel = RcVec3f.Normalize(vel);
            return vel * speed;
        }


    }
}
