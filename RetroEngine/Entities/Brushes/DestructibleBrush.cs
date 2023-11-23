using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Map;
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

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            Health = data.GetPropertyFloat("health",30);
            CalculateParticleSpawnLocations();

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);

            if (Health <= 0)
            {
                foreach (Vector3 location in particleLocations)
                {
                    ParticleSystem system = ParticleSystem.Create("hitSmoke");
                    system.Position = location;
                    system.Start();
                }

                Destroy();

            }

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
                        VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[part.VertexBuffer.VertexCount];
                        part.VertexBuffer.GetData(vertices);

                        foreach(VertexPositionNormalTexture vertex in vertices)
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

            for(int i = 0; i < particleLocations.Count; i++)
            {
                Vector3 location = particleLocations[i];

                particleLocations[i] = (location + avgLocation) / 2f;
            }

            particleLocations.Add(avgLocation);

        }

    }
}
