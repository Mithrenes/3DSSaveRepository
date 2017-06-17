using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveCentral.Misc
{
    class Dir
    {
        public static void CreateDir(string TempFolderPath, string SavesPath, string ExtDataPath)
        {
            char separator = Path.DirectorySeparatorChar;
            if (!Directory.Exists(TempFolderPath + separator))
            {
                Directory.CreateDirectory(@TempFolderPath + separator);
            }
            if (SavesPath != null && !Directory.Exists(@SavesPath))
            {             
                Directory.CreateDirectory(@SavesPath);
            }
            if (ExtDataPath != null && !Directory.Exists(@ExtDataPath))
            {
                Directory.CreateDirectory(@ExtDataPath);
            }
        }

        public static void DeleteDir(string TempFolderPath)
        {
            char separator = Path.DirectorySeparatorChar;
            if (Directory.Exists(TempFolderPath + separator))
            {
                Directory.Delete(@TempFolderPath + separator, true);
            }
            if (Directory.Exists("JKSV" + separator))
            {
                Directory.Delete(@"JKSV" + separator, true);
            }        
        }
    }
}
