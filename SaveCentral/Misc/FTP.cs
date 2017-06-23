using FluentFTP;
using SaveCentral.Classes;
using SaveCentral.Misc;
using SaveCentral.UIControls;
using SaveCentral.WebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SaveCentral
{
    public class FTP
    {
        public static FtpListItem[] GetAllSavesInDevice(string DeviceIP, string FolderPath)
        {
            try
            {
                // create an FTP client
                using (FtpClient client = new FtpClient(DeviceIP))
                {
                    client.Port = 5000;
                    // if you don't specify login credentials, we use the "anonymous" user account
                    client.Credentials = new System.Net.NetworkCredential("anonymous", "anonymous");
                    // begin connecting to the server
                    client.Connect();
                    return client.GetListing(FolderPath);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not connect to FTP due to the following error: " + e.Message);
                return null;
            }           
        }
        public static async Task<bool> DownloadFilesToLocal(string DeviceIP, string SavesFTPPath, string SaveTitle, string SaveDescription, string SaveRegion, string SaveType, TextBox txtHasExtData, ProgressBar PB, Label LBL)
        {
            try
            {
                string FilesIncluded = "";
                UpdateMessages.UploadMsg(-1, PB, LBL);
                int HasExtData = 0;
                char separator = Path.DirectorySeparatorChar;
                string TempFolderPath = Constants.TempFolder + separator + "TempSaves";
                string SavesPath = SavesFTPPath.Substring(1);
                //MessageBox.Show(SavesPath);
                string ExtDataPath = Utils.ReplaceFirstOccurrence(SavesPath, "Saves", "ExtData");
                string fullPath = Path.GetFullPath(SavesFTPPath).TrimEnd(separator);
                string GameName = Path.GetFileName(Path.GetDirectoryName(fullPath));
                fullPath = Utils.ReplaceLastOccurrence(fullPath,separator.ToString(),"_");
                string ZipName = Path.GetFileName(fullPath) + "_" + Constants.User + ".zip";

                List<String> SavesList = new List<string>();
                List<String> ExtDataList = new List<string>();
                // create an FTP client
                using (FtpClient client = new FtpClient(DeviceIP))
                {
                    client.Port = 5000;
                    // if you don't specify login credentials, we use the "anonymous" user account
                    client.Credentials = new System.Net.NetworkCredential("anonymous", "anonymous");
                    // begin connecting to the server
                    client.Connect();
                    // retry up to 4 times when uploading a file (in case of file corruption)
                    client.RetryAttempts = 4;
                    // get a list of files and directories
                    foreach (FtpListItem item in await client.GetListingAsync(SavesFTPPath))
                    {
                        // if it is a file
                        if (item.Type == FtpFileSystemObjectType.File)
                        {
                            SavesPath = System.IO.Path.GetDirectoryName(item.FullName).Substring(1);
                            Dir.CreateDir(TempFolderPath, SavesPath, null);
                            // download the file
                            await client.DownloadFileAsync(@item.FullName.Substring(1), item.FullName, true, FtpVerify.Retry);
                            SavesList.Add(item.FullName);
                            FilesIncluded += item.Name + "|";
                        }
                        // if it is a folder then go deeper to fin more files
                        if (item.Type == FtpFileSystemObjectType.Directory)
                        {
                            foreach (FtpListItem itemLevel2 in await client.GetListingAsync(item.FullName))
                            {
                                SavesPath = System.IO.Path.GetDirectoryName(itemLevel2.FullName).Substring(1);
                                Dir.CreateDir(TempFolderPath, SavesPath, null);
                                await client.DownloadFileAsync(@itemLevel2.FullName.Substring(1), itemLevel2.FullName, true, FtpVerify.Retry);
                                SavesList.Add(itemLevel2.FullName);
                                FilesIncluded += itemLevel2.Name + "|";
                            }                          
                        }
                    }
                    
                    string ExtDataFTP = Utils.ReplaceFirstOccurrence(SavesFTPPath, "Saves", "ExtData");
                    if (client.DirectoryExists(ExtDataFTP))
                    {
                        foreach (FtpListItem item in await client.GetListingAsync(ExtDataFTP))
                        {
                            // if it is a file
                            if (item.Type == FtpFileSystemObjectType.File)
                            {
                                ExtDataPath = System.IO.Path.GetDirectoryName(item.FullName).Substring(1);
                                Dir.CreateDir(TempFolderPath, null, ExtDataPath);
                                // download the file
                                await client.DownloadFileAsync(@item.FullName.Substring(1), item.FullName, true, FtpVerify.Retry);
                                ExtDataList.Add(item.FullName);
                                FilesIncluded += "(ExtData) " + item.Name + "|";
                            }
                            // if it is a folder then go deeper to find more files
                            if (item.Type == FtpFileSystemObjectType.Directory)
                            {
                                foreach (FtpListItem itemLevel2 in await client.GetListingAsync(item.FullName))
                                {
                                    ExtDataPath = System.IO.Path.GetDirectoryName(itemLevel2.FullName).Substring(1);
                                    Dir.CreateDir(TempFolderPath, null, ExtDataPath);
                                    await client.DownloadFileAsync(@itemLevel2.FullName.Substring(1), itemLevel2.FullName, true, FtpVerify.Retry);
                                    ExtDataList.Add(itemLevel2.FullName);
                                    FilesIncluded += "(ExtData) " + itemLevel2.Name + "|";                                   
                                }                              
                            }
                        }
                    }
                    
                    if (FilesIncluded.Length > 0)
                    {
                        FilesIncluded = FilesIncluded.Substring(0, FilesIncluded.Length - 1);
                    }
                                                         
                    //Check if the saves also hast ExtData
                    
                    if (ExtDataList.Count> 0)
                    {
                        HasExtData = 1;
                        txtHasExtData.Text = "Yes";
                    }
                    else
                    {                     
                        txtHasExtData.Text = "No";
                    }                   
                    
                    System.IO.Compression.ZipFile.CreateFromDirectory(@"JKSV" + separator, @TempFolderPath + separator + ZipName, System.IO.Compression.CompressionLevel.Optimal, true);

                    client.Disconnect();
                    //Uploading to mega
                    SavesClass SV = new SavesClass();
                    SV = await Mega.UploadFilesAsync(@TempFolderPath + separator + ZipName, Constants.User, PB, LBL); //Here I get DLink and FileSize
                    if (SV != null)
                    {
                        //If true then Insert/update record in DB
                        SV.IdUser = Constants.IdUser;
                        SV.FileName = ZipName;
                        SV.GameName = GameName;
                        SV.SaveType = SaveType;
                        SV.Region = SaveRegion;
                        SV.Title = SaveTitle;
                        SV.Description = SaveDescription;
                        SV.FilesIncluded = FilesIncluded;
                        SV.HasExtData = HasExtData.ToString();

                        if (SaveType != null)
                        {
                            if (await Insert.InsertNewSave(SV, PB, LBL))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (await Update.UpdateSaveFromFTP(SV, PB, LBL))
                            {
                                return true;
                            }
                        }
                        
                    }                 
                }
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not complete the operation due to the following error: " + e.Message);
                return false;
                // null;
            }
        }
        public static async Task<bool> CopyFilesFromLocalToFTP(string DeviceIP, string SavesLocalDir)
        {
            try
            {
                char separator = Path.DirectorySeparatorChar;
                List<String> SavesList = new List<string>();
                List<String> ExtDataList = new List<string>();
                string FTPpathToSave;
                
                using (FtpClient client = new FtpClient(DeviceIP))
                {
                    client.Port = 5000;
                    // if you don't specify login credentials, we use the "anonymous" user account
                    client.Credentials = new System.Net.NetworkCredential("anonymous", "anonymous");
                    // begin connecting to the server
                    client.Connect();
                    client.RetryAttempts = 4;
                    if (Directory.Exists(SavesLocalDir + separator + "JKSV" + separator + "Saves"))
                    {
                        SavesList = await DirSearch(SavesLocalDir + separator + "JKSV" + separator + "Saves", client);
                    }
                    if (Directory.Exists(SavesLocalDir + separator + "JKSV" + separator + "ExtData"))
                    {
                        ExtDataList = await DirSearch(SavesLocalDir + separator + "JKSV" + separator + "ExtData", client);
                    }
                        

                  //  if (SavesList.Count > 0)
                   // {
                    //    FTPpathToSave = System.IO.Path.GetDirectoryName(SavesList[0].Substring(SavesList[0].IndexOf("JKSV") - 1));
                     //   await client.UploadFilesAsync(SavesList, FTPpathToSave, FtpExists.Overwrite, true, FtpVerify.Retry);
                   // }
                    //if (ExtDataList.Count > 0)
                    //{
                     //   FTPpathToSave = System.IO.Path.GetDirectoryName(ExtDataList[0].Substring(SavesList[0].IndexOf("JKSV") - 1));
                      //  await client.UploadFilesAsync(ExtDataList, FTPpathToSave, FtpExists.Overwrite, true, FtpVerify.Retry);
                    //}
                    
                    client.Disconnect();
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not connect to FTP due to the following error: " + e.Message);
                return false;
            }
        }
        private static async Task<List<String>> DirSearch(string sDir, FtpClient client)
        {
            char separator = Path.DirectorySeparatorChar;
            List<String> files = new List<String>();
            try
            {

                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                    if (!client.DirectoryExists(System.IO.Path.GetDirectoryName(f.Substring(f.IndexOf("JKSV") - 1)))){
                        client.CreateDirectory(System.IO.Path.GetDirectoryName(f.Substring(f.IndexOf("JKSV") - 1)));
                    }
                    await client.UploadFileAsync(f, f.Substring(f.IndexOf("JKSV") - 1), FtpExists.Overwrite, true, FtpVerify.Retry);
                }
                if (files.Count > 0) {
                    //for (int i = 0; i<= files.Count - 1; i++)
                    //{
                    //    MessageBox.Show(files[i].ToString());
                   // }
                    
                    //await client.UploadFilesAsync(files, separator + sDir, FtpExists.Overwrite, true);
                }
                
                
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    Directory.CreateDirectory(d);
                    files.AddRange(await DirSearch(d, client));
                }
            }
            catch (System.Exception excpt)
            {
                MessageBox.Show("Could not connect to FTP due to the following error: " + excpt.Message);
            }

            return files;
        }
    }
}
