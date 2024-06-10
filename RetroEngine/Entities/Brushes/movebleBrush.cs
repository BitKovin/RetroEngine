using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RetroEngine.Entities.Brushes
{
    [LevelObject("movebleBrush")]
    public class MovebleBrush : Entity
    {

        Vector3 targetLocation;

        float time = 2;

        [JsonInclude]
        public float progress = 0;

        [JsonInclude]
        public bool open = false;

        public MovebleBrush() 
        {
            mergeBrushes = true;

            SaveGame = true;
        }

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            targetLocation = data.GetPropertyVector("targetLocation",new Vector3(0,3,0));

            time = data.GetPropertyFloat("time", time);

            foreach (StaticMesh mesh in meshes)
            {
                mesh.Static = false;
            }

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);
            open = !open;
        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            if (action == "open")
                open = true;

            if(action == "close")
                open = false;

        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            if(open)
            {
                progress += Time.DeltaTime/time;
            }else
            {
                progress -= Time.DeltaTime/time;
            }

            progress = Math.Clamp(progress, 0, 1);  

            Position = Vector3.Lerp(Vector3.Zero, targetLocation, progress);

            foreach(var body in bodies)
            {
                body.SetPosition(Position);
            }


            foreach(var mesh in meshes)
            {
                mesh.Position = Position;
            }

        }





    }
}
