using CG.Web.MegaApiClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaveCentral.Classes;
using SaveCentral.UIControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static CG.Web.MegaApiClient.MegaApiClient;

namespace SaveCentral.WebService
{
    class Mega
    {
        internal class MALogin : IDisposable
        {
            public MegaApiClient Login()
            {
                MegaApiClient client = new MegaApiClient();
                AuthInfos AI = JsonConvert.DeserializeObject<AuthInfos>(Constants.AuthInfoJsonString);
                client.Login(AI);
                return client;
            }
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected bool Disposed { get; private set; }

            /// <summary>
            /// Dispose worker method. See http://coding.abel.nu/2012/01/disposable
            /// </summary>
            /// <param name="disposing">Are we disposing? 
            /// Otherwise we're finalizing.</param>
            protected virtual void Dispose(bool disposing)
            {
                Disposed = true;
            }
        }

        /// <summary>
        /// Creates a folder with the username in case it doesn't already exist and returns the folder's name. If the folder already exists checks if the FileName already exists, and if it does
        /// deletes it, then returns the folder's name.
        /// </summary>
        /// <param name="client">MegaApiClient instance</param>       
        /// <param name="Username">Curent user's Username</param>
        /// <param name="FileName">File prepared to be uploaded (name must include extension). If the file already exists in Mega then deletes the old one before uploading this new version.</param>
        /// <returns>INode - Folder's name.</returns>
        public static INode ChkIfFolderAndFileExist(MegaApiClient client, string Username, string FileName)
        {
            IEnumerable<INode> nodes = client.GetNodes();
            INode root = nodes.Single(n => n.Type == NodeType.Root);
            IEnumerable<INode> FoldersInRoot = client.GetNodes(root);

            INode FolderSearched = nodes.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name == Username);
            if (FolderSearched == null)
            {
                return client.CreateFolder(Username, root);

            }
            else if (DeleteFileIfExist(client, nodes.Where(n => n.Type == NodeType.Directory && n.Name == Username), FileName))
            {
                return FolderSearched;
            }
            else { return null; }
        }
        private static bool DeleteFileIfExist(MegaApiClient client, IEnumerable<INode> UserFolder, string FileName)
        {
            try
            {
                INode F = UserFolder.FirstOrDefault();
                IEnumerable<INode> T = client.GetNodes(F);
                INode S = T.FirstOrDefault(n => n.Name == FileName);
                if (S == null)
                {
                    return true;
                }
                else
                {
                    //Delete older version to avoid duplicates.
                    client.Delete(S, false);
                    return true;
                }

            }
            catch
            {
                return false;
            }
        }

        public static async Task<SavesClass> UploadFilesAsync(string PathFileToUpload, string Username, ProgressBar PB, Label LBL)
        {
            string Filename = Path.GetFileName(PathFileToUpload);
            string DLink, FileSize;
            try
            {
                using (MALogin MAL = new MALogin())
                {

                    MegaApiClient client = MAL.Login();
                    INode Folder = await Task.Run(() => ChkIfFolderAndFileExist(client, Username, Filename));
                    if (Folder != null)
                    {

                        Progress<double> ze = new Progress<double>(p => PB.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                        new Action(() => { UpdateMessages.UploadMsg((int)p, PB, LBL); })));
                        using (FileStream stream = File.Open(@PathFileToUpload, FileMode.Open))
                        {
                            INode FileUploaded = await client.UploadAsync(stream, Filename, Folder, ze); //Uploading File from PC to Mega
                            //INode FileUploaded = await client.UploadFileAsync(PathFileToUpload, Folder, ze); //Uploading File from PC to Mega
                            if (FileUploaded != null)
                            {
                                DLink = (await client.GetDownloadLinkAsync(FileUploaded)).ToString();
                                FileSize = (((int)(FileUploaded.Size / 1024))).ToString(); //Size in Kb                                                      
                                client.Logout();
                                GC.Collect(); // TENGO QUE LLAMAR ESTO O NO DEJA BORRAR EL ARCHIVO ZIP PORQUE DICE QUE AUN ESTA EN USO. 
                                if (!DLink.Equals(""))
                                {
                                    SavesClass SC = new SavesClass();
                                    SC.DLink = DLink;
                                    SC.Size = FileSize;
                                    return SC;
                                }
                                else
                                {
                                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.UploadMsg(101, PB, LBL); }));
                                    return null;
                                }
                                
                            }
                            PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.UploadMsg(101, PB, LBL); }));
                            return null;
                        }                                                   
                    }
                    else
                    {
                        PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.UploadMsg(101, PB, LBL); }));
                        return null;
                    }
                }
            }
            catch
            {
                PB.Dispatcher.Invoke(DispatcherPriority.Normal,new Action(() => { UpdateMessages.UploadMsg(102, PB, LBL); }));
                return null;
            }
        }
        public static async Task<bool> DownloadFilesAsync(string FileName, string PathToDownload, string DeviceIP, ProgressBar PB, Label LBL)
        {
            bool b = false;
            try
            {
                char separator = Path.DirectorySeparatorChar;
                string DirToDownload = PathToDownload + "TempDL";
                Directory.CreateDirectory(DirToDownload);
                //MessageBox.Show(lbitemsToDownload[lbFilesToDownload.SelectedIndex].FileName + " " + lbitemsToDownload[lbFilesToDownload.SelectedIndex].NomUser);
                string DLink = await Download.GetDLinkFromWeb(FileName);
                if (DLink.Equals(""))
                {
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(101, PB, LBL); }));
                    MessageBox.Show("Could not get download link. Please try again. If the problem persists report it.");
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(103, PB, LBL); }));
                    return b;
                }
                else
                {
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(0, PB, LBL); }));
                    SavesClass SV = new SavesClass();
                    SV.FileName = FileName;
                    SV.DLCount = "1";
                    await Update.UpdateDLCount(SV);
                    MegaApiClient client = new MegaApiClient();
                    client.LoginAnonymous();
                    Progress<double> ze = new Progress<double>(p => PB.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                        new Action(() =>
                                                                        {
                                                                            UpdateMessages.DownloadMsg((int)p, PB, LBL);
                                                                        })));
                    await client.DownloadFileAsync(new Uri(DLink), DirToDownload + separator + FileName, ze);
                    ZipFile.ExtractToDirectory(DirToDownload + separator + FileName, DirToDownload);
                    File.Delete(DirToDownload + separator + FileName);
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(104, PB, LBL); }));
                    b = await FTP.CopyFilesFromLocalToFTP(DeviceIP, DirToDownload);
                    GC.Collect(); // TENGO QUE LLAMAR ESTO O NO DEJA BORRAR EL ARCHIVO ZIP PORQUE DICE QUE AUN ESTA EN USO.                            
                    client.Logout();
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(105, PB, LBL); }));
                    Misc.Dir.DeleteDir(Constants.TempFolder + System.IO.Path.DirectorySeparatorChar + "TempDL");
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(103, PB, LBL); }));
                    //Misc.Utils.TryToDeleteFile(DirToDownload + "/" + FileName);
                    return b;
                }

            }
            catch (Exception e)
            {
                UpdateMessages.DownloadMsg(102, PB, LBL);
                MessageBox.Show("Save could not be downloaded due to the following error: " + e.Message);
                UpdateMessages.DownloadMsg(103, PB, LBL);
                return b;
            }
        }
        public static async Task<string> DownloadFilesAsyncForUpdate(string FileName, string PathToDownload, ProgressBar PB, Label LBL)
        {
            string filepath = null;
            try
            {
                char separator = Path.DirectorySeparatorChar;
                string DirToDownload = PathToDownload + "TempDL";
                Directory.CreateDirectory(DirToDownload);
                //MessageBox.Show(lbitemsToDownload[lbFilesToDownload.SelectedIndex].FileName + " " + lbitemsToDownload[lbFilesToDownload.SelectedIndex].NomUser);
                string DLink = await Download.GetDLinkFromWeb(FileName);
                if (DLink.Equals(""))
                {
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(101, PB, LBL); }));
                    MessageBox.Show("Could not get download link. Please try again. If the problem persists report it.");
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(103, PB, LBL); }));
                    return filepath;
                }
                else
                {
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(0, PB, LBL); }));
                    
                    MegaApiClient client = new MegaApiClient();
                    client.LoginAnonymous();
                    Progress<double> ze = new Progress<double>(p => PB.Dispatcher.Invoke(DispatcherPriority.Normal,
                                                                        new Action(() =>
                                                                        {
                                                                            UpdateMessages.DownloadMsg((int)p, PB, LBL);
                                                                        })));
                    await client.DownloadFileAsync(new Uri(DLink), DirToDownload + separator + FileName, ze);
                    ZipFile.ExtractToDirectory(DirToDownload + separator + FileName, DirToDownload);
                    File.Delete(DirToDownload + separator + FileName);
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(104, PB, LBL); }));
                   
                    GC.Collect(); // TENGO QUE LLAMAR ESTO O NO DEJA BORRAR EL ARCHIVO ZIP PORQUE DICE QUE AUN ESTA EN USO.                            
                    client.Logout();
                    string[] subdirectoryEntries = Directory.GetDirectories(DirToDownload + separator + "JKSV"); //With this I get Saves and ExtData (if it exists)
                    int count = 0;
                    foreach (string st in subdirectoryEntries)
                    {
                        string[] subdirectoryEntrieslv2 = Directory.GetDirectories(st); //Here I get game name
                        foreach (string st2 in subdirectoryEntrieslv2)
                        {
                            string[] subdirectoryEntrieslv3 = Directory.GetDirectories(st2); //Here I get gslot name
                            foreach (string st3 in subdirectoryEntrieslv3)
                            {
                                if (st3.Contains("Saves") && !st3.Contains("ExtData"))
                                {
                                    filepath = st3.Substring(st3.IndexOf("JKSV") - 1);
                                }
                            }
                        }                       
                        count++; //If this is 2 then it means the save hast ExtData folder
                    }
                    
                    Misc.Dir.DeleteDir(Constants.TempFolder + System.IO.Path.DirectorySeparatorChar + "TempDL");
                    PB.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { UpdateMessages.DownloadMsg(103, PB, LBL); }));
                    return filepath;
                }

            }
            catch (Exception e)
            {
                UpdateMessages.DownloadMsg(102, PB, LBL);
                MessageBox.Show("Save could not be downloaded due to the following error: " + e.Message);
                UpdateMessages.DownloadMsg(103, PB, LBL);
                return null;
            }
        }
        /// <summary>
        /// Generates a JSON file with login encrypted data to use with Mega Api
        /// </summary>
        /// <returns>true if everything went ok</returns>
        public static bool GetAuthInfo()
        {
            try
            {
                MegaApiClient client = new MegaApiClient();
                AuthInfos AU = MegaApiClient.GenerateAuthInfos("megatest@gmail.com", "password"); //Your email and pass used to create the mega account
                MegaApiClient.AuthInfos AF = AU;// = new MegaApiClient.AuthInfos()
                MessageBox.Show(AF.PasswordAesKey.ToString());
                string json = JsonConvert.SerializeObject(AF);
                JObject Auth = (JObject)JToken.FromObject(AF);
                // write JSON directly to a file
                using (StreamWriter file = File.CreateText("C:/Auth.json")) //location where the json file is going to be saved
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    Auth.WriteTo(writer);
                }                            
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
