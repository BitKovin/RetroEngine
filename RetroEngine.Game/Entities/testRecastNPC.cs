using DotRecast.Detour.Crowd;
using RetroEngine.NavigationSystem;
using RetroEngine.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{
    internal class testRecastNPC : Entity
    {

        StaticMesh mesh = new StaticMesh();

        DtCrowdAgent crowdAgent;

        public override void Start()
        {
            base.Start();

            crowdAgent = CrowdSystem.CreateAgent(this, Position);

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            mesh.LoadFromFile("models/cube.obj");
            mesh.texture = AssetRegistry.LoadTextureFromFile("cat.png");

            meshes.Add(mesh);

        }

        public override void Update()
        {
            base.Update();

            Position = crowdAgent.npos.FromRc();

            mesh.Position = Position;

            if (Input.GetAction("test").Pressed())
            {
                var hit = Physics.LineTrace(Camera.position, Camera.position + Camera.Forward * 100, bodyType: BodyType.World);

                crowdAgent.SetAgentTargetPosition(hit.HitPointWorld);

            }

        }

    }
}
