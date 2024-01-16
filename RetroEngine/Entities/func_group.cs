using RetroEngine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Entities
{
    [LevelObject("func_group")]
    internal class func_group : Entity
    {

        string layerName = "default";


        public override void FromData(EntityData data)
        {
            base.FromData(data);

            string type = data.GetPropertyString("_tb_type");

            if (type == "_tb_layer")
            {
                Layer = (int)data.GetPropertyFloat("_tb_id");

                layerName = data.GetPropertyString("_tb_name", $"layer{Layer}");

                Level.GetCurrent().TryAddLayerName(layerName, Layer);
            }
        }
    }
}
