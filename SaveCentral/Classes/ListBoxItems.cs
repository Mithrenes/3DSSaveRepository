using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SaveCentral.Classes
{
    public class ListBoxItems
    {
        public BitmapImage ImageSource { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Username { get; set; }
        public string GameName { get; set; }
        public string SaveType { get; set; }
        public string Region { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string FileIncluded { get; set; }
        public string HasExtData { get; set; }
        public string Date_Created { get; set; }
        public string Date_Modified { get; set; }
        public string DLCount { get; set; }
        
    }
}
