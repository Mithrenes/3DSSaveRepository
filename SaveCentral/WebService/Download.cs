using Newtonsoft.Json;
using SaveCentral.Classes;
using SaveCentral.SQLite;
using SaveCentral.UIControls;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaveCentral.WebService
{
    class Download
    {
        public static async Task<bool> GetAllSavesFromWeb()
        {
            SavesContainer SVC = new SavesContainer();
            string[] ColNames = {"Username", "FileName", "GameName", "SaveType", "Region", "Size", "Title", "Description", "FilesIncluded", "HasExtData", "DLCount", "Date_Created", "Date_Modif" };
            try
            {
                SQLiteHelper db = new SQLiteHelper();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/files_data"));
                request.Accept = "application/json";
                request.ContentType = "application/json";
                request.Headers.Add("authorization", Constants.ApiKey);
                request.Method = "GET";
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (Stream ResponseStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(ResponseStream))
                        {
                            string ServerResponse = sr.ReadToEnd();
                            SVC = JsonConvert.DeserializeObject<SavesContainer>(ServerResponse);
                            if (SVC.status.Equals("1"))
                            {
                                //Success
                                db.ClearTable(Constants.files_data);
                                if (db.OpenNonQueryTransaction())
                                {
                                    foreach (var saves in SVC.Saves)
                                    {
                                        
                                        Dictionary<string, string> Values = new Dictionary<string, string>();
                                        Values.Add(ColNames[0], saves.Username);
                                        Values.Add(ColNames[1], saves.FileName);
                                        Values.Add(ColNames[2], saves.GameName);
                                        Values.Add(ColNames[3], saves.SaveType);
                                        Values.Add(ColNames[4], saves.Region);
                                        Values.Add(ColNames[5], saves.Size);
                                        Values.Add(ColNames[6], saves.Title);
                                        Values.Add(ColNames[7], saves.Description);
                                        Values.Add(ColNames[8], saves.FilesIncluded);
                                        Values.Add(ColNames[9], saves.HasExtData);
                                        Values.Add(ColNames[10], saves.DLCount);
                                        Values.Add(ColNames[11], saves.Date_Created);
                                        Values.Add(ColNames[12], saves.Date_Modif);
                                       
                                        db.AddNonQueryForOpenTransaction(Constants.files_data, Values);
                                    }
                                    db.CloseNonQueryTransaction();
                                }
                                return true;
                            }
                            else
                            {
                                //Fail                                   
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not download info from web due to the following error: " + e.Message);
                return false;
            }
        }
        public static async Task<string> GetDLinkFromWeb(string FileName)
        {
            SavesContainer SVC = new SavesContainer();
            string[] ColNames = { "Username", "FileName", "GameName", "SaveType", "Region", "Size", "Title", "Description", "FilesIncluded", "HasExtData", "DLCount", "Date_Created", "Date_Modif" };
            try
            {
                SQLiteHelper db = new SQLiteHelper();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/files_data/" + WebUtility.UrlEncode(FileName)));
                request.Accept = "application/json";
                request.ContentType = "application/json";
                //request.Headers.Add("authorization", Constants.ApiKey);
                request.Method = "GET";
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (Stream ResponseStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(ResponseStream))
                        {
                            string ServerResponse = sr.ReadToEnd();
                            SVC = JsonConvert.DeserializeObject<SavesContainer>(ServerResponse);
                            if (SVC.status.Equals("1"))
                            {
                                //Success
                                foreach (var save in SVC.Saves)
                                {
                                    return save.DLink;
                                }
                                return "";

                            }
                            else
                            {
                                //Fail                                   
                                return "";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Could not get info from web due to the following error: " + e.Message);
                return "";
            }
        }
        public static async Task<string> GetDLCountFromWeb(string FileName, string OldValue)
        {
            SavesContainer SVC = new SavesContainer();
            string[] ColNames = { "Username", "FileName", "GameName", "SaveType", "Region", "Size", "Title", "Description", "FilesIncluded", "HasExtData", "DLCount", "Date_Created", "Date_Modif" };
            try
            {
                SQLiteHelper db = new SQLiteHelper();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/files_data/" + WebUtility.UrlEncode(FileName)));
                request.Accept = "application/json";
                request.ContentType = "application/json";
                //request.Headers.Add("authorization", Constants.ApiKey);
                request.Method = "GET";
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    using (Stream ResponseStream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(ResponseStream))
                        {
                            string ServerResponse = sr.ReadToEnd();
                            SVC = JsonConvert.DeserializeObject<SavesContainer>(ServerResponse);
                            if (SVC.status.Equals("1"))
                            {
                                //Success
                                foreach (var save in SVC.Saves)
                                {
                                    await Task.Run(() => {
                                        Dictionary<string, string> Values = new Dictionary<string, string>();
                                        List<SQLiteParameter> prm = new List<SQLiteParameter>();
                                        prm.Add(new SQLiteParameter("@FileName") { Value = FileName });
                                        Values.Add(ColNames[10], save.DLCount);
                                        db.Update(Constants.files_data, Values, "FileName = @FileName", prm);
                                    });
                                    return save.DLCount;
                                }
                                return OldValue;

                            }
                            else
                            {
                                //Fail                                   
                                return OldValue;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("Could not get info from web due to the following error: " + e.Message);
                return OldValue;
            }
        }
    }
}
