using FluentFTP;
using SaveCentral.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaveCentral.Misc;
using System.Windows.Controls;
using SaveCentral.SQLite;
using System.Data;
using SaveCentral.WebService;
using System.Data.SQLite;

namespace SaveCentral
{
    class DynamicControls
    {
        public static async Task<List<ListBoxItems>> GetGSavesFromDevice(string DeviceIP, string FolderPath)
        {
            try
            {
                List<ListBoxItems> LBI = new List<ListBoxItems>();
                FtpListItem[] FTPitems = await Task.Run(() => FTP.GetAllSavesInDevice(DeviceIP, FolderPath));
                foreach (FtpListItem FTPitem in FTPitems)
                {
                    if (FTPitem.Type == FtpFileSystemObjectType.Directory)
                    {
                        LBI.Add(new ListBoxItems()
                        {
                            FilePath = FTPitem.FullName,
                            GameName = FTPitem.Name,
                            ImageSource = Images.GetFiletypeBitmapImage("folder")
                        });
                    }                  
                }

                return LBI;
            }
            catch
            {
                return null;
            }          
        }
        public static async Task<List<ListBoxItems>> GetAllSavesForCurrentUser(string Username)
        {
            List<SQLiteParameter> prm = new List<SQLiteParameter>();
            List<ListBoxItems> LBI = new List<ListBoxItems>();
            SQLiteHelper db = new SQLiteHelper();
            string query = "SELECT Username, FileName, GameName, SaveType, Region, Size, Title, Description, FilesIncluded, HasExtData, DLCount, Date_Created, Date_Modif FROM " + Constants.files_data + 
                            " WHERE Username = @Username ORDER BY GameName";
            prm.Add(new SQLiteParameter("@Username") { Value = Username });
            DataTable dt = await Task.Run(() => db.GetDataTable(query,prm));
            await Task.Run(() =>
            {
                foreach (DataRow dr in dt.Rows)
                {
                    LBI.Add(new ListBoxItems()
                    {
                        Username = dr.ItemArray[0].ToString(),
                        ImageSource = Images.GetFiletypeBitmapImage("Folder"),
                        FileName = dr.ItemArray[1].ToString(),
                        GameName = dr.ItemArray[2].ToString(),
                        SaveType = dr.ItemArray[3].ToString(),
                        Region = dr.ItemArray[4].ToString(),
                        Size = dr.ItemArray[5].ToString(),
                        Title = dr.ItemArray[6].ToString(),
                        Description = dr.ItemArray[7].ToString(),
                        FileIncluded = dr.ItemArray[8].ToString(),
                        HasExtData = dr.ItemArray[9].ToString(),
                        DLCount = dr.ItemArray[10].ToString(),
                        Date_Created = dr.ItemArray[11].ToString(),
                        Date_Modified = dr.ItemArray[12].ToString(),
                    });
                }
            });
            return LBI;
        }
        public static async Task<List<ListBoxItems>> GetAllSaves()
        {
            List<ListBoxItems> LBI = new List<ListBoxItems>();
            SQLiteHelper db = new SQLiteHelper();
            string query = "SELECT Username, FileName, GameName, SaveType, Region, Size, Title, Description, FilesIncluded, HasExtData, DLCount, Date_Created, Date_Modif FROM " + Constants.files_data + " ORDER BY GameName";
            DataTable dt = await Task.Run(() => db.GetDataTable(query,null));
            await Task.Run(() =>
            {
                foreach (DataRow dr in dt.Rows)
                {
                    LBI.Add(new ListBoxItems()
                    {
                        Username = dr.ItemArray[0].ToString(),
                        ImageSource = Images.GetFiletypeBitmapImage("Folder"),
                        FileName = dr.ItemArray[1].ToString(),
                        GameName = dr.ItemArray[2].ToString(),
                        SaveType = dr.ItemArray[3].ToString(),
                        Region = dr.ItemArray[4].ToString(),
                        Size = dr.ItemArray[5].ToString(),
                        Title = dr.ItemArray[6].ToString(),
                        Description = dr.ItemArray[7].ToString(),
                        FileIncluded = dr.ItemArray[8].ToString(),
                        HasExtData = dr.ItemArray[9].ToString(),
                        DLCount = dr.ItemArray[10].ToString(),
                        Date_Created = dr.ItemArray[11].ToString(),
                        Date_Modified = dr.ItemArray[12].ToString(),
                    });
                }
            });
            return LBI;
        }
        public static async Task<DataTable> GetAllSavesFirstLetter()
        {
            List<ListBoxItems> LBI = new List<ListBoxItems>();
            SQLiteHelper db = new SQLiteHelper();
            string query = "SELECT DISTINCT(substr(GameName,1,1)) AS GameName FROM " + Constants.files_data + " ORDER BY GameName";
            DataTable dt = await Task.Run(() => db.GetDataTable(query, null));
            
            return dt;
        }

        public static async Task<List<ListBoxItems>> GetFilesIncludedFromLocalDB(string Username, string FileName, TextBox Title, TextBox Description, ComboBox Region, TextBox SaveType, TextBox HasExtData, TextBox DLCount, TextBox Uploader)
        {
            List<SQLiteParameter> prm = new List<SQLiteParameter>();
            List<ListBoxItems> ListitemsIncluded = new List<ListBoxItems>();
            DataTable dt;
            SQLiteHelper db = new SQLiteHelper();

            string query = "SELECT FilesIncluded, Title, Description, Region, SaveType, HasExtData, DLCount FROM " + Constants.files_data +
                           " WHERE Username = @Username AND FileName = @FileName";
            prm.Add(new SQLiteParameter("@Username") { Value = Username });
            prm.Add(new SQLiteParameter("@FileName") { Value = FileName });
            dt = await Task.Run(() => db.GetDataTable(query, prm));
            List<string> FilesIncluded = dt.Rows[0].ItemArray[0].ToString().Split('|').ToList<string>();
            foreach (string File in FilesIncluded)
            {
                await Task.Run(() =>
                {
                    ListitemsIncluded.Add(new ListBoxItems()
                    {
                        ImageSource = Images.GetFiletypeBitmapImage(System.IO.Path.GetExtension(File.ToLower())),
                        GameName = File
                    });
                });               
            }
            Title.Text = dt.Rows[0].ItemArray[1].ToString();
            Description.Text = dt.Rows[0].ItemArray[2].ToString();
            int _Region, _SaveType;
            Int32.TryParse(dt.Rows[0].ItemArray[3].ToString(), out _Region);
            Int32.TryParse(dt.Rows[0].ItemArray[4].ToString(), out _SaveType);
            Region.Text = Enum.GetName(typeof(Constants.AllRegions), _Region); 
            SaveType.Text = Enum.GetName(typeof(Constants.SaveType), _SaveType);

            if (dt.Rows[0].ItemArray[5].ToString().Equals("1"))
            {
                HasExtData.Text = "Yes";
            }
            else { HasExtData.Text = "No"; }
            if(DLCount != null && Uploader != null)
            {
                DLCount.Text = dt.Rows[0].ItemArray[6].ToString();
                Uploader.Text = Username;
            }
            
            return ListitemsIncluded;
        }
        public static void ResetUploadUI(TextBox txtSaveTitle, TextBox txtSaveDescription, ComboBox cmbSaveRegion, TextBox txtHasExtData, ListBox lbSaveSlotsInGameFolder, ListBox lbGameSavesInDevice)
        {
            txtSaveTitle.Clear();
            txtSaveDescription.Clear();
            cmbSaveRegion.SelectedIndex = -1;           
            txtHasExtData.Clear();
            lbSaveSlotsInGameFolder.UnselectAll();
            lbSaveSlotsInGameFolder.ItemsSource = null;
            lbGameSavesInDevice.UnselectAll();
        }
        public static void ResetUpdateAndDLUI(TextBox txtSaveTitle, TextBox txtSaveDescription, ComboBox cmbSaveRegion, TextBox SaveType, TextBox txtHasExtData, TextBox txtDLCount, TextBox txtUploader, ListBox lbSaveSlotsInGameFolder)
        {
            txtSaveTitle.Clear();
            txtSaveDescription.Clear();
            cmbSaveRegion.SelectedIndex = -1;
            txtSaveTitle.Clear();
            txtHasExtData.Clear();
            lbSaveSlotsInGameFolder.UnselectAll();
            lbSaveSlotsInGameFolder.ItemsSource = null;
            if (txtDLCount != null && txtUploader != null)
            {
                txtDLCount.Clear();
                txtUploader.Clear();
            }
        }

        public static async Task<List<ListBoxItems>> LoadWithFilter(string RegionSel, string txtFilterSearch, string JumpToLetter = "")
        {
            List<SQLiteParameter> prm = new List<SQLiteParameter>();
            RegionSel = GetDeviceRegion(RegionSel);
            List<ListBoxItems> LBI = new List<ListBoxItems>();
            SQLiteHelper db = new SQLiteHelper();
            string query = "";
            if (!RegionSel.Equals(""))
            {
                if (!JumpToLetter.Equals(""))
                {
                    query = "SELECT Username, FileName, GameName, SaveType, Region, Size, Title, Description, FilesIncluded, HasExtData, DLCount, Date_Created, Date_Modif FROM " + Constants.files_data +
                            " WHERE Region = '" + RegionSel + "' AND GameName LIKE @FilterSearch ORDER BY GameName";
                    prm.Add(new SQLiteParameter("@FilterSearch") { Value = JumpToLetter + '%' });
                }
                else
                {
                    if (!txtFilterSearch.Equals(""))
                    {
                        query = "SELECT Username, FileName, GameName, SaveType, Region, Size, Title, Description, FilesIncluded, HasExtData, DLCount, Date_Created, Date_Modif FROM " + Constants.files_data +
                                " WHERE Region = '" + RegionSel + "' AND Title LIKE @FilterSearch ORDER BY GameName";
                        prm.Add(new SQLiteParameter("@FilterSearch") { Value = '%' + txtFilterSearch + '%' });
                    }
                    else
                    {
                        query = "SELECT Username, FileName, GameName, SaveType, Region, Size, Title, Description, FilesIncluded, HasExtData, DLCount, Date_Created, Date_Modif FROM " + Constants.files_data + " WHERE Region = '" + RegionSel + "' ORDER BY GameName";
                        prm = null;
                    }
                }
               
                
            }else if (RegionSel.Equals(""))
            {
                if (!JumpToLetter.Equals(""))
                {
                    query = "SELECT Username, FileName, GameName, SaveType, Region, Size, Title, Description, FilesIncluded, HasExtData, DLCount, Date_Created, Date_Modif FROM " + Constants.files_data +
                            " WHERE GameName LIKE @FilterSearch ORDER BY GameName";
                    prm.Add(new SQLiteParameter("@FilterSearch") { Value = JumpToLetter + '%' });
                }
                else
                {
                    if (!txtFilterSearch.Equals(""))
                    {
                        query = "SELECT Username, FileName, GameName, SaveType, Region, Size, Title, Description, FilesIncluded, HasExtData, DLCount, Date_Created, Date_Modif FROM " + Constants.files_data +
                                " WHERE Title LIKE @FilterSearch ORDER BY GameName";
                        prm.Add(new SQLiteParameter("@FilterSearch") { Value = '%' + txtFilterSearch + '%' });

                    }
                    else
                    {
                        query = "SELECT Username, FileName, GameName, SaveType, Region, Size, Title, Description, FilesIncluded, HasExtData, DLCount, Date_Created, Date_Modif FROM " + Constants.files_data + " ORDER BY GameName";
                        prm = null;
                    }
                }
                            
            }
            
            DataTable dt = await Task.Run(() => db.GetDataTable(query,prm));

            await Task.Run(() =>
            {
                foreach (DataRow dr in dt.Rows)
                {
                    LBI.Add(new ListBoxItems()
                    {
                        Username = dr.ItemArray[0].ToString(),
                        ImageSource = Images.GetFiletypeBitmapImage("Folder"),
                        FileName = dr.ItemArray[1].ToString(),
                        GameName = dr.ItemArray[2].ToString(),
                        SaveType = dr.ItemArray[3].ToString(),
                        Region = dr.ItemArray[4].ToString(),
                        Size = dr.ItemArray[5].ToString(),
                        Title = dr.ItemArray[6].ToString(),
                        Description = dr.ItemArray[7].ToString(),
                        FileIncluded = dr.ItemArray[8].ToString(),
                        HasExtData = dr.ItemArray[9].ToString(),
                        DLCount = dr.ItemArray[10].ToString(),
                        Date_Created = dr.ItemArray[11].ToString(),
                        Date_Modified = dr.ItemArray[12].ToString(),
                    });
                }
            });

            return LBI;
        }

        public static string GetDeviceRegion(string selValue)
        {
            switch (selValue)
            {
                case "USA":
                    return ((int)Constants.AllRegions.USA).ToString();
                case "EUR":
                    return ((int)Constants.AllRegions.EUR).ToString();
                case "JPN":
                    return ((int)Constants.AllRegions.JPN).ToString();
                default:
                    return "";
            }
        }
    }
}
