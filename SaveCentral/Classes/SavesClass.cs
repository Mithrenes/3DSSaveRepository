using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveCentral.Classes
{
    public class SavesClass
    {
        // Auto-Impl Properties for trivial get and set
        public string Username { get; set; }
        public string IdUser { get; set; }
        public string FileName { get; set; }
        public string GameName { get; set; }
        public string SaveType { get; set; }
        public string Region { get; set; }
        public string Size { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilesIncluded { get; set; }
        public string HasExtData { get; set; }
        public string DLink { get; set; }
        public string DLCount { get; set; }
        public string Date_Created { get; set; }
        public string Date_Modif { get; set; }
        public string IsUpdate { get; set; }
        public string IsDelete { get; set; }
        public SavesClass() { }


    }
    public class SavesContainer
    {

        public SavesClass Save;
        public SavesClass[] Saves;
        public string message { get; set; }
        public string status { get; set; }
    }
}
