using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    internal class AssetRegistry
    {

        const string ROOT_PATH = "../../../../";

        public static string FindPathForFile(string path)
        {

            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            if (File.Exists(ROOT_PATH + "Configuration/" + path))
                return ROOT_PATH + "Configuration/" + path;

            if (File.Exists(ROOT_PATH + "Configuration/textures/" + path))
                return ROOT_PATH + "GameData/textures/" + path;

            return path;
        }

    }
}
