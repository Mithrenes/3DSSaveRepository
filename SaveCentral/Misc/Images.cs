using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SaveCentral.Misc
{
    class Images
    {
        /// <summary>
        /// Gets the image that represents the file's extension.
        /// </summary>
        /// <param name="fileExtension">File's extension. (.doc, .pdf, etc...)</param>
        /// <returns>Un BitmapImage that can be asigned to an Image control.</returns>
        public static BitmapImage GetFiletypeBitmapImage(string fileExtension)
        {
            BitmapImage logo = new BitmapImage();
            switch (fileExtension.ToLower())
            {
                case ".bin":
                    logo.BeginInit();
                    logo.UriSource = new Uri("pack://application:,,,/SaveCentral;component/Resources/binicon.png");
                    logo.EndInit();
                    logo.Freeze();
                    return logo;
                case ".dat":
                    logo.BeginInit();
                    logo.UriSource = new Uri("pack://application:,,,/SaveCentral;component/Resources/daticon.png");
                    logo.EndInit();
                    logo.Freeze();
                    return logo;
                case ".sav":
                    logo.BeginInit();
                    logo.UriSource = new Uri("pack://application:,,,/SaveCentral;component/Resources/savicon.png");
                    logo.EndInit();
                    logo.Freeze();
                    return logo;              
                case ".zip":
                case ".7z":
                case ".rar":
                    logo.BeginInit();
                    logo.UriSource = new Uri("pack://application:,,,/SaveCentral;component/Resources/winraricon.png");
                    logo.EndInit();
                    logo.Freeze();
                    return logo;               
                case "folder":
                    logo.BeginInit();
                    logo.UriSource = new Uri("pack://application:,,,/SaveCentral;component/Resources/foldericon.png");
                    logo.EndInit();
                    logo.Freeze();
                    return logo;
                default:
                    logo.BeginInit();
                    logo.UriSource = new Uri("pack://application:,,,/SaveCentral;component/Resources/defaulticon.png");
                    logo.EndInit();
                    logo.Freeze();
                    return logo;

            }
        }
    }
}
