using BulletSharp;
using RetroEngine;
using RetroEngine.Entities;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{

    [LevelObject("water")]
    public class Water : TriggerBase
    {

        string target = "";

        string enterAction = "trigger_enter";
        string exitAction = "trigger_exit";

        public Water()
        {
            AffectNavigation = false;
        }



        public override void Start()
        {
            base.Start();

            foreach (var body in bodies)
            {
                body.SetBodyType(PhysicsSystem.BodyType.Liquid);
            }

            foreach(var mesh in meshes)
            {
                mesh.Transperent = true;
                mesh.Transparency = 0.9f;
                mesh.TwoSided = true;

                mesh.DistanceSortingRadius = -10000;
            }

        }



        public override void FromData(EntityData data)
        {
            base.FromData(data);

            target = data.GetPropertyString("target");

            enterAction = data.GetPropertyString("onEnterAction", enterAction);
            exitAction = data.GetPropertyString("onExitAction",exitAction);

        }

        public override void OnTriggerEnter(Entity entity)
        {
            base.OnTriggerEnter(entity);

            entity?.OnAction("water_enter");

        }

        public override void OnTriggerExit(Entity entity)
        {
            base.OnTriggerExit(entity);

            entity?.OnAction("water_exit");

        }

    }
}
