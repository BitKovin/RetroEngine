using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RetroEngine.Map;
using RetroEngine.NavigationSystem;
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

        DynamicObstacleHelper DynamicObstacleHelper = new DynamicObstacleHelper();

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

            DynamicObstacleHelper.Meshes = meshes;

        }

        public override void Destroy()
        {

            DynamicObstacleHelper?.Destroy();

            base.Destroy();
        }

        public override void Start()
        {
            base.Start();

            Entity offsetPoint = Level.GetCurrent().FindEntityByName(offsetPointName);

            if (offsetPoint == null) return;

            offsetPosition = offsetPoint.Position;
            offsetRotation = offsetPoint.Rotation - new Vector3(0,90,0);

            DynamicObstacleHelper.Update();

        }

        public override void OnDamaged(float damage, Entity causer = null, Entity weapon = null)
        {
            base.OnDamaged(damage, causer, weapon);
            //open = !open;
        }

        public override void OnAction(string action)
        {
            base.OnAction(action);

            if (action == "open")
            {
                open = true;
                return;
            }
            if (action == "close")
            {
                open = false;
                return;
            }

            open = !open;

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


            Vector3 startPos = Position;

            Position = Vector3.Lerp(Vector3.Zero, targetLocation, progress);

            Rotation = Vector3.Lerp(Vector3.Zero, offsetRotation, progress);


            Position -= offsetPosition;

            DrawDebug.Line(offsetPosition, offsetPosition + Rotation.GetForwardVector(), Vector3.UnitX, 0.01f);
            DrawDebug.Line(offsetPosition, offsetPosition + offsetRotation.GetForwardVector(), Vector3.UnitZ, 0.01f);


            // Apply rotation transformation
            Matrix rotationMatrix = offsetRotation.GetRotationMatrix();

            Rotation = Quaternion.Slerp(Quaternion.Identity, Quaternion.CreateFromRotationMatrix(rotationMatrix), progress).ToEulerAnglesDegrees();

            rotationMatrix = Rotation.GetRotationMatrix();

            Position = Vector3.Transform(Position, Quaternion.CreateFromRotationMatrix(rotationMatrix));

            // Move position back to world space
            Position += offsetPosition;

            if (Position == startPos && SpawnTime + 1 < Time.gameTime) return;

            foreach (var body in bodies)
            {
                body.SetPosition(Position);
                body.SetRotation(Rotation);
            }


            foreach(var mesh in meshes)
            {
                mesh.Position = Position;
                mesh.Rotation = Rotation;

                DynamicObstacleHelper.Update();

            }
        }
    }
}
