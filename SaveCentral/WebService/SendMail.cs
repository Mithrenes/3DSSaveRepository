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

namespace SaveCentral.WebService
{
    class SendMail
    {
        public static async Task<bool> Send(string Username, string Email, string Password)
        {
            MailClassContainer C = new MailClassContainer();
            MailClass LC = new MailClass();
            try
            {               
                string LC_obj = "";
                LC.Sender = Constants.Sender;
                LC.Receiver = Username + " <" + Email + ">";
                LC.Subject = Constants.Subject;
                LC.Message = "This address has been used to create an account for SaveCentral. \n" +
                             "Keep this mail with your login data since it is not stored and cannot be recovered. \n\n" +
                             "Username: " + Username + "\n" +
                             "Email: " + Email + "\n" +
                             "Password: " + Password +
                             "\n\n***Don't repply to this mail.***";
                LC_obj = LC_obj + JsonConvert.SerializeObject(LC, Formatting.Indented);

                WebRequest request = WebRequest.Create(new Uri(Constants.SEND_ACCOUNT_DATA_MAIL));
                request.Method = "POST";
                request.ContentType = "application/json";

                byte[] byteArray = Encoding.UTF8.GetBytes(LC_obj);
                request.ContentLength = byteArray.Length;
                using (Stream DataStream = await request.GetRequestStreamAsync())
                {
                    DataStream.Write(byteArray, 0, byteArray.Length);
                    DataStream.Close();

                    using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
                    {
                        using (Stream ResponseStream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(ResponseStream))
                            {
                                string ServerResponse = reader.ReadToEnd();                             
                                C = JsonConvert.DeserializeObject<MailClassContainer>(ServerResponse);
                                reader.Close();
                                ResponseStream.Close();
                                response.Close();
                            }                            
                        }                       
                    }                   
                }
                
                if (C.status.Equals("1"))
                {
                    //Success
                    MessageBox.Show(C.message.ToString());
                    return true;
                }
                else
                {
                    //Fail
                    MessageBox.Show(C.message.ToString());
                    return false;
                }

            }
            catch
            {               
                return false;
            }
        }
    }
}
