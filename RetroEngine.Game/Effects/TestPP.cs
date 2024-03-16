using RetroEngine.Graphic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetroEngine.Game.Effects
{
    internal class TestPP : PostProcessStep
    {

        public TestPP() 
        {
            Shader = AssetRegistry.GetPostProcessShaderFromName("TestPostProcessEffect");
        }

    }
}
