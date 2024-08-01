using BulletSharp.SoftBody;
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

        float time = 1;

        [JsonInclude]
        public float progress = 0;

        [JsonInclude]
        public bool open = false;

        Vector3 offsetPosition = Vector3.Zero;
        Vector3 offsetRotation = Vector3.Zero;

        public MovebleBrush() 
        {
            mergeBrushes = true;

            SaveGame = true;
        }

        string offsetPointName;

        public override void FromData(EntityData data)
        {
            base.FromData(data);

            targetLocation = data.GetPropertyVector("targetLocation",new Vector3(0,0,0));

            time = data.GetPropertyFloat("time", time);

            foreach (StaticMesh mesh in meshes)
            {
                mesh.Static = false;
            }

            offsetPointName = data.GetPropertyString("rotationPointName");

        }

        public override void Start()
        {
            base.Start();

            Entity offsetPoint = Level.GetCurrent().FindEntityByName(offsetPointName);

            if (offsetPoint == null) return;

            offsetPosition = offsetPoint.Position;
            offsetRotation = offsetPoint.Rotation;

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

        public override void FinalizeFrame()
        {

            if(open)
            {
                progress += Time.DeltaTime/time;
            }else
            {
                progress -= Time.DeltaTime/time;
            }

            progress = Math.Clamp(progress, 0, 1);  

            


            Position = Vector3.Lerp(Vector3.Zero, targetLocation, progress);

            Rotation = Vector3.Lerp(Vector3.Zero, offsetRotation, progress);


            Position -= offsetPosition;

            // Apply rotation transformation
            Matrix rotationMatrix = Matrix.CreateRotationX(offsetRotation.X / 180 * (float)Math.PI) *
                                    Matrix.CreateRotationY(offsetRotation.Y / 180 * (float)Math.PI) *
                                    Matrix.CreateRotationZ(offsetRotation.Z / 180 * (float)Math.PI);

            Rotation = Quaternion.Lerp(Quaternion.Identity, Quaternion.CreateFromRotationMatrix(rotationMatrix), progress).ToEulerAnglesDegrees();

            rotationMatrix = Matrix.CreateRotationX(Rotation.X / 180 * (float)Math.PI) *
                                    Matrix.CreateRotationY(Rotation.Y / 180 * (float)Math.PI) *
                                    Matrix.CreateRotationZ(Rotation.Z / 180 * (float)Math.PI);

            Position = Vector3.Transform(Position, Quaternion.CreateFromRotationMatrix(rotationMatrix));

            // Move position back to world space
            Position += offsetPosition; 


            foreach (var body in bodies)
            {
                if(body.WorldTransform.Translation != Position)
                    body.SetPosition(Position);

                body.SetRotation(Rotation);
            }


            foreach(var mesh in meshes)
            {
                mesh.Position = Position;
                mesh.Rotation = Rotation;
            }

        }





    }
}
