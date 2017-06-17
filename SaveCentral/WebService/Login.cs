using Newtonsoft.Json;
using SaveCentral.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SaveCentral.WebService
{
    class Login
    {
        public static async Task<bool> ChkIfLoginPass(string User, string Password, Label lblSessionStatus)
        {
            LoginClass L = new LoginClass();
            LoginContainer LC = new LoginContainer();
            try
            {
                lblSessionStatus.Content = "Iniciando sesión...";
                lblSessionStatus.Background = Brushes.AliceBlue;

                string passwordSHA256 = Misc.Utils.GenerateSHA256String(Password);
                
                string L_obj = "";
                L.UsernameOrEmail = User;
                L.Password = passwordSHA256;
                L_obj = L_obj + JsonConvert.SerializeObject(L, Formatting.Indented);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Constants.URL + "/api.savecentral.com/v1/users/login"));
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
                                string ServerResponse = sr.ReadToEnd();
                                LC = JsonConvert.DeserializeObject<LoginContainer>(ServerResponse);
                                Constants.ApiKey = LC.user.ApiKey;
                                Constants.User = LC.user.Username;
                                lblSessionStatus.Content = Constants.lblLogin + LC.user.Username;
                                lblSessionStatus.FontWeight = FontWeights.Bold;
                                lblSessionStatus.Background = Brushes.LightGreen;
                                return true;
                            }
                        }                                           
                    }
                }
            }
            catch(WebException e)
            {
                if (e.Response != null)
                {
                    using (var errorResponse = (HttpWebResponse)e.Response)
                    {
                        using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                        {                            
                            string error = reader.ReadToEnd();
                            LC = JsonConvert.DeserializeObject<LoginContainer>(error);
                            //MessageBox.Show(LC.message);
                            Constants.ApiKey = "";
                            Constants.User = "";
                            lblSessionStatus.Content = Constants.lblNoLoged;
                            lblSessionStatus.FontWeight = FontWeights.Bold;
                            lblSessionStatus.Background = Brushes.LightGray;
                            return false;
                        }
                    }
                }
                Constants.ApiKey = "";
                Constants.User = "";
                return false;

            }                                 
        }
       
    }
}
