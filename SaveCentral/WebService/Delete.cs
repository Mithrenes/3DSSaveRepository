using CG.Web.MegaApiClient;
using Newtonsoft.Json;
using SaveCentral.Classes;
using SaveCentral.UIControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static SaveCentral.WebService.Mega;

namespace SaveCentral.WebService
{
    class Delete
    {
        public static async Task<bool> DeleteSelSave(SavesClass SV, ProgressBar PB, Label LBL)
        {
            UpdateMessages.DeleteMsg(10, PB, LBL);
            SavesContainer SVC = new SavesContainer();
            try
            {
                SV.IsDelete = "1";
                string SV_obj = "";

                SV_obj = SV_obj + JsonConvert.SerializeObject(SV, Formatting.Indented);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/files_data/" + WebUtility.UrlEncode(SV.FileName)));
                request.Accept = "application/json";
                request.ContentType = "application/json";
                request.Headers.Add("authorization", Constants.ApiKey);
                request.Method = "POST";

                byte[] byteArray = Encoding.UTF8.GetBytes(SV_obj);
                request.ContentLength = byteArray.Length;
                using (Stream DataStream = await request.GetRequestStreamAsync())
                {
                    DataStream.Write(byteArray, 0, byteArray.Length);
                    DataStream.Close();
                    UpdateMessages.DeleteMsg(50, PB, LBL);
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
                                    using (MALogin MAL = new MALogin())
                                    {
                                        MegaApiClient client = MAL.Login();
                                        //DELETE FILE FROM MEGA                 
                                        var nodes = client.GetNodes();
                                        var SelNodes = nodes.First(n => n.Name == SV.FileName && n.Type == NodeType.File);
                                        await client.DeleteAsync(SelNodes, false);
                                        client.Logout();
                                    }
                                    UpdateMessages.DeleteMsg(100, PB, LBL);
                                    MessageBox.Show(SVC.message);
                                    UpdateMessages.DeleteMsg(103, PB, LBL);
                                    return true;
                                }
                                else
                                {
                                    //Fail                                   
                                    UpdateMessages.DeleteMsg(101, PB, LBL);
                                    MessageBox.Show(SVC.message);
                                    UpdateMessages.DeleteMsg(103, PB, LBL);
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                UpdateMessages.DeleteMsg(102, PB, LBL);
                //MessageBox.Show("Save could not be deleted due to the following error:" + e.Message);
                if (e.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)e.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            string error = reader.ReadToEnd();
                            SVC = JsonConvert.DeserializeObject<SavesContainer>(error);


                            MessageBox.Show(SVC.message);

                            return false;
                        }
                    }
                }
                UpdateMessages.DeleteMsg(103, PB, LBL);               
                return false;
            }
        }
    }
}
