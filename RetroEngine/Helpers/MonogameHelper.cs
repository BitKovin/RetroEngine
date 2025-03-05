using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RetroEngine.Helpers
{
    public static class MonogameHelper
    {

        // Set the center of the BoundingBox by returning a new one
        public static BoundingBox SetCenter(this BoundingBox boundingBox, Vector3 center)
        {
            Vector3 size = boundingBox.Max - boundingBox.Min;
            Vector3 newMin = center - size / 2;
            Vector3 newMax = center + size / 2;
            return new BoundingBox(newMin, newMax);
        }

        // Get the center of the BoundingBox (midpoint between Min and Max)
        public static Vector3 GetCenter(this BoundingBox boundingBox)
        {
            return (boundingBox.Min + boundingBox.Max) / 2;
        }

        // Set the size of the BoundingBox by returning a new one
        public static BoundingBox SetSize(this BoundingBox boundingBox, Vector3 size)
        {
            Vector3 center = GetCenter(boundingBox);
            Vector3 newMin = center - size / 2;
            Vector3 newMax = center + size / 2;
            return new BoundingBox(newMin, newMax);
        }

        // Get the size of the BoundingBox (difference between Max and Min)
        public static Vector3 GetSize(this BoundingBox boundingBox)
        {
            return boundingBox.Max - boundingBox.Min;
        }

    }
}
