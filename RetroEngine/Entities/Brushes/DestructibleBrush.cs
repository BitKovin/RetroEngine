using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Map;
using RetroEngine.NavigationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Brushes
{
    [LevelObject("destructible")]
    public class DestructibleBrush : Entity
    {

        List<Vector3> particleLocations = new List<Vector3>();

        string systemName = "destructionWood";

        public DestructibleBrush() 
        {
            mergeBrushes = true;

            SaveGame = true;
        }

        DynamicObstacleHelper DynamicObstacleHelper = new DynamicObstacleHelper();

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Health = data.GetPropertyFloat("health",30);
            CalculateParticleSpawnLocations();

            systemName = data.GetPropertyString("paricleSystem", "testSystem");

            ParticleSystem.Preload(systemName);

            foreach(StaticMesh mesh in meshes)
            {
                mesh.Static = false;
            }

            DynamicObstacleHelper.Meshes = meshes;
            //DynamicObstacleHelper.Update();

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            if (Health <= 0)
            {
                foreach (Vector3 location in particleLocations)
                {
                    ParticleSystem system = ParticleSystem.Create(systemName);
                    system.Position = location;
                    system.Start();

                }

                Destroy();

            }

        }

        public override void Update()
        {
            base.Update();

            if(GameMain.SkipFrames>0)
                DynamicObstacleHelper.Update();

            //DynamicObstacleHelper.DebugDraw();

        }

        public override void Destroy()
        {

            DynamicObstacleHelper.Destroy();

            base.Destroy();
        }

        void CalculateParticleSpawnLocations()
        {
            Vector3 avgLocation = new Vector3();
            float n = 0;

            foreach(StaticMesh sMesh in meshes)
            {
                foreach(ModelMesh mesh in sMesh.model.Meshes)
                {
                    foreach(ModelMeshPart part in mesh.MeshParts)
                    {
                        VertexData[] vertices = new VertexData[part.VertexBuffer.VertexCount];
                        part.VertexBuffer.GetData(vertices);

                        foreach(VertexData vertex in vertices)
                        {
                            if(particleLocations.Contains(vertex.Position) == false)
                                particleLocations.Add(vertex.Position);

                            avgLocation += vertex.Position;
                            n += 1;
                        }

                    }
                }
            }

            avgLocation /= n;

            Random rand = new Random();

            for (int i = 0; i < particleLocations.Count; i++)
            {
                Vector3 location = particleLocations[i];

                particleLocations[i] = Vector3.Lerp(location, avgLocation, Math.Min((float)rand.NextDouble() + 0.2f,1));
            }

            particleLocations.Add(avgLocation);

        }

        protected override void LoadAssets()
        {
            base.LoadAssets();

            ParticleSystem.Preload(systemName);

        }

    }
}
