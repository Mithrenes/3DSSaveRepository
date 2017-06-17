using Newtonsoft.Json;
using SaveCentral.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Net;
using System.IO;
using System.Windows;
using SaveCentral.UIControls;

namespace SaveCentral.WebService
{
    class Update
    {
        //Here I'm only updating the TextBox and Combobox info (DLink is the same)
        public static async Task<bool> UpdateSaveInfo(SavesClass SV, ProgressBar PB, Label LBL)
        {
            UpdateMessages.UpdateMsg(10, PB, LBL);
            SavesContainer SVC = new SavesContainer();
            try
            {
                SV.IsUpdate = "1"; //Stupid free server don't like PUT and DELETE methods so I have to force this part.
                string SV_obj = "";
                SV_obj = SV_obj + JsonConvert.SerializeObject(SV, Formatting.Indented);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/files_data/" + WebUtility.UrlEncode(SV.FileName)));
                request.Accept = "application/json";
                request.ContentType = "application/json";
                request.Headers.Add("authorization", Constants.ApiKey);
                request.Method = "POST";
                request.KeepAlive = true;
                byte[] byteArray = Encoding.UTF8.GetBytes(SV_obj);
                request.ContentLength = byteArray.Length;
                using (Stream DataStream = await request.GetRequestStreamAsync())
                {
                    DataStream.Write(byteArray, 0, byteArray.Length);
                    DataStream.Close();

                    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        using (Stream ResponseStream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(ResponseStream))
                            {
                                string ServerResponse = await sr.ReadToEndAsync();
                                SVC = JsonConvert.DeserializeObject<SavesContainer>(ServerResponse);
                                if (SVC.status.Equals("1"))
                                {
                                    //Success
                                    UpdateMessages.UpdateMsg(100, PB, LBL);
                                    MessageBox.Show(SVC.message);
                                    UpdateMessages.UpdateMsg(103, PB, LBL);
                                    return true;
                                }
                                else
                                {
                                    //Fail                                   
                                    UpdateMessages.UpdateMsg(101, PB, LBL);
                                    MessageBox.Show(SVC.message);
                                    UpdateMessages.UpdateMsg(103, PB, LBL);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UpdateMessages.UpdateMsg(102, PB, LBL);
                MessageBox.Show("The save info could no be updated due to the following error: " + e.Message);
                UpdateMessages.UploadMsg(103, PB, LBL);
                return false;
            }
        }
        public static async Task<bool> UpdateDLCount(SavesClass SV)
        {
            SavesContainer SVC = new SavesContainer();
            try
            {
                SV.IsUpdate = "1";
                string SV_obj = "";
                
                SV_obj = SV_obj + JsonConvert.SerializeObject(SV, Formatting.Indented);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/files_data"));
                request.Accept = "application/json";
                request.ContentType = "application/json";
                request.Method = "POST";

                byte[] byteArray = Encoding.UTF8.GetBytes(SV_obj);
                request.ContentLength = byteArray.Length;
                using (Stream DataStream = await request.GetRequestStreamAsync())
                {
                    DataStream.Write(byteArray, 0, byteArray.Length);
                    DataStream.Close();

                    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        using (Stream ResponseStream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(ResponseStream))
                            {
                                string ServerResponse = await sr.ReadToEndAsync();

                                SVC = JsonConvert.DeserializeObject<SavesContainer>(ServerResponse);
                                if (SVC.status.Equals("1"))
                                {
                                    //Success                                   
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
            }
            catch (Exception e)
            {               
                return false;
            }
        }
    }
}
