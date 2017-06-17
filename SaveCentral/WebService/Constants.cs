using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveCentral.WebService
{
    class Constants
    {
        //PHP URL
        public static string URL = "https://savecentral.000webhostapp.com";//"http://localhost";// //URL where is the database and php files
        public static string MailURL = "https://savecentral.000webhostapp.com"; //URL from where the mails are sent

        //Global variables por ids
        public static string IdUser = "";
        public static string User = "";
        public static string ApiKey = "";        

        //Email stuff
        public static string SEND_ACCOUNT_DATA_MAIL = MailURL + "/send_mail.php";
        public static string Sender = "Scarecrow B <ScarecrowB@ScrapTown.Inc>";
        public static string Subject = "Account creation.";

        //Login Messages
        public static string lblLogin = "Login Info \nUser: ";
        public static string lblNoLoged = "Login Info \nNo Logged.";
        public static string lblErrLogin = "Login Info \nConnection Error.";

        //Misc
        public static string TempFolder = System.IO.Path.GetTempPath();
        public static string AuthInfoJsonString =""; //Write here the contents of the json Auth.json file

        //Name Tables
        public static string files_data = "files_data";

        //Enums used for other stuff
        public  enum SaveType
        {
            //ADD HERE ANY OTHER SAVES FOR ANY OTHER CONSOLE
            N3DS = 1 //This is for Nintendo 3DS because it doesn't allow to start with a number
        }

        public enum AllRegions
        {
            USA = 1,
            EUR = 2,
            JPN = 3
        }


    }
}
