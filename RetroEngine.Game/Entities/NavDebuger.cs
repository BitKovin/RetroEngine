using Microsoft.Xna.Framework;
using RetroEngine.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Entities
{
    internal class NavDebuger : Entity
    {

        Vector3 startPoint = Vector3.Zero;
        Vector3 targetPoint = Vector3.Zero;

        List<Vector3> locations = new List<Vector3>();

        Delay updateDelay = new Delay();

        int index = 0;

        Box box;

        public NavDebuger() 
        {
            Start();
        }

        public override void Start()
        {
            base.Start();

            box = Level.GetCurrent().AddEntity(new Box()) as Box;

            box.size = new Vector3(0.4f);
        }

        public override void Update()
        {
            base.Update();

            if(Input.GetAction("test").Pressed())
            {
                startPoint = Camera.position;

                updateNavPoints();

            }

            if (Input.GetAction("test2").Pressed())
            {
                targetPoint = Camera.position;

                updateNavPoints();

            }

            if (updateDelay.Wait()) return;
            updateDelay.AddDelay(0.3f);

            index++;

            if (locations.Count > 0) 
            {
                while (index >= locations.Count)
                    index -= locations.Count;

                box.Position = locations[index];
            }

        }

        void updateNavPoints()
        {
            locations = Navigation.FindPath(startPoint, targetPoint);
        }

    }
}
