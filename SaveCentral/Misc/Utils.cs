using CG.Web.MegaApiClient;
using SaveCentral.WebService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaveCentral.Misc
{
    class Utils
    {

        public static void ChkIfUpgradeRequired()
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
        }
        private class MyWebClient : System.Net.WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 4 * 1000; //Wait 4 secs before giving up
                return w;
            }
        }
        public static bool IsInternetAvailable()
        {
            try
            {
                using (MyWebClient client = new MyWebClient())
                {
                    using (Stream stream = client.OpenRead("https://www.google.com/"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if database is online.
        /// </summary>
        /// <returns>True if online, false if offline.</returns>
        public static bool IsServerOnline()
        {
            try
            {
                using (MyWebClient client = new MyWebClient())
                {
                    using (Stream stream = client.OpenRead(Constants.URL))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Checks if we can login to MEGA
        /// </summary>
        /// <returns>True if online, false if offline.</returns>
        public static bool IsMegaOnline()
        {
            try
            {
                MegaApiClient client = new MegaApiClient();

                client.LoginAnonymous();
                if (client.IsLoggedIn)
                {
                    client.Logout();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Convert string to SHA256
        /// </summary>
        /// <param name="inputString">String to convert.</param>
        /// <returns>SHA256 as string.</returns>
        public static string GenerateSHA256String(string inputString)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha256.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }
        /// <summary>
        /// Convierte un array de byteas a string
        /// </summary>
        /// <param name="hash">Es el array de bytes a convertir.</param>
        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i <= hash.Length - 1; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
        /// <summary>
        /// Checks if string is a valid email.
        /// </summary>
        /// <param name="strIn">Email (string) to validate. </param>
        /// <returns>True if email has a valid format, otherwise returns false.</returns>
        static bool invalid;
        public static bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }
        public static bool ZipMultiFile(string ZipPath, string ZipFilename, string[] PathFiletoZipArray, CompressionLevel CL = CompressionLevel.Optimal)
        {
            try
            {
                Directory.CreateDirectory(ZipPath);
                using (FileStream fs = new FileStream(@ZipPath + ZipFilename + ".zip", FileMode.Create))
                using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    foreach (String SingleFiletoZip in PathFiletoZipArray)
                    {
                        arch.CreateEntryFromFile(@SingleFiletoZip, Path.GetFileName(SingleFiletoZip), CL);
                    }
                    FileInfo fi = new FileInfo(@ZipPath + @ZipFilename + ".zip");
                    //Constants.SizeOfFileToUpload = (int)(fi.Length / 1024);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public static string ReplaceFirstOccurrence(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }
    }
}
