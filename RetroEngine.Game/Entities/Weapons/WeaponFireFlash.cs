using Microsoft.Xna.Framework;
using RetroEngine.Entities.Light;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities.Weapons
{
    internal class WeaponFireFlash : PointLight
    {

        public float Duration = 0.2f;

        public float Radius = 5;

        public float FlashIntensity = 5;

        public WeaponFireFlash() 
        {
            Color = new Vector3(1,1,0.4f);
        }

        public override void AsyncUpdate()
        {
            base.AsyncUpdate();

            float progress = MathHelper.Saturate(((float)(Time.gameTime - SpawnTime)) / Duration);

            radius = Radius * MathHelper.Lerp(1,0.5f, progress);

            Intensity = MathHelper.Lerp(Intensity, 0, progress);

            //DrawDebug.Sphere(0.01f, Position, lightData.Color, 0.1f);

            //DrawDebug.Line(Position, Camera.position,null, 0.01f);


        }

        public static void CreateAt(Vector3 location, float duraion = 0.2f, float radius = 4, float FlashIntensity = 0.4f)
        {
            WeaponFireFlash flash = Level.GetCurrent().AddEntity(new WeaponFireFlash {Position = location, Duration = duraion, Radius = radius, FlashIntensity = FlashIntensity}) as WeaponFireFlash;

            flash.enabled = true;

            flash.Position = location;

            flash.Start();
            flash.Destroy(duraion);
        }

    }
}
