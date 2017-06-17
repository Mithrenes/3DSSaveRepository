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
    class Insert
    {
        /// <summary>
        /// <para>Creates a new account using the following variables: NomUser, Email and Password</para>
        /// <para>Error handling is Api side, so just catch the response in a try-catch</para>
        /// </summary>
        /// <param name="Username">Must be unique.</param>
        /// <param name="Email">Must be unique</param>
        /// <param name="Password">At least 1 character long.</param>
        /// <returns>True if the account was created successfully.</returns>
        public static async Task<bool> CreateNewAccount(string Username, string Email, string Password)
        {
            if (Misc.Utils.IsValidEmail(Email) && !Username.Equals("") && !Password.Equals(""))
            {
                LoginClass L = new LoginClass();
                LoginContainer LC = new LoginContainer();
                try
                {
                    string passwordSHA256 = Misc.Utils.GenerateSHA256String(Password);                 
                    string L_obj = "";
                    L.Username = Username;
                    L.Email = Email;
                    L.Password = passwordSHA256;
                    L_obj = L_obj + JsonConvert.SerializeObject(L, Formatting.Indented);

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/users/register"));
                    request.Accept = "application/json";
                    request.ContentType = "application/json";
                    request.Method = "POST";
                    
                    byte[] byteArray = Encoding.UTF8.GetBytes(L_obj);
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
                                    
                                    LC = JsonConvert.DeserializeObject<LoginContainer>(ServerResponse);
                                    //Success
                                    await SendMail.Send(Username, Email, Password);                                    
                                    MessageBox.Show(LC.message);
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    if (e.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)e.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string error = reader.ReadToEnd();
                                LC = JsonConvert.DeserializeObject<LoginContainer>(error);
                                if (LC.message.Contains("SQLSTATE[23000]: Integrity constraint violation"))
                                {
                                    MessageBox.Show("Username or Email already exist.");
                                }
                                else
                                {
                                    MessageBox.Show(LC.message);
                                }
                                
                                return false;
                            }
                        }
                    }
                    return false;
                }
            }
            else
            {
                MessageBox.Show("El usuario, contraseña o email introducidos no son válidos. Debes introducir un email válido y el usuario y contraseña no pueden estar vacios.");
                return false;
            }
        }

        public static async  Task<bool> InsertNewSave(SavesClass SV, ProgressBar PB, Label LBL)
        {
            
            SavesContainer SVC = new SavesContainer();
            try
            {
                string SV_obj = "";               

                SV_obj = SV_obj + JsonConvert.SerializeObject(SV, Formatting.Indented);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/files_data"));
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
                                    MessageBox.Show(SVC.message);
                                    UpdateMessages.UploadMsg(103, PB, LBL);
                                    return true;
                                }
                                else
                                {
                                    //Fail                                   
                                    await Delete.DeleteSelSave(SV, PB, LBL);
                                    MessageBox.Show("An error ocurred when trying to register the upload in the DB, please try again.");
                                    UpdateMessages.UploadMsg(103, PB, LBL);
                                    return false;
                                }
                            }                                                         
                        }
                    }
                }
            }
            catch (WebException e)
            {
                await Delete.DeleteSelSave(SV, PB, LBL);

                if (e.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)e.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            string error = reader.ReadToEnd();
                            SVC = JsonConvert.DeserializeObject<SavesContainer>(error);
                            MessageBox.Show("The save could no be uploaded due to the following error: " + SVC.message);
                        }
                    }
                }
                UpdateMessages.UploadMsg(103, PB, LBL);
                return false;
            }
        }       
    }
}
