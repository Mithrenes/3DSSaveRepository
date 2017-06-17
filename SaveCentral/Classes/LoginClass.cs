using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveCentral.Classes
{
    public class LoginClass
    {
        // Auto-Impl Properties for trivial get and set
        public string IdUser { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
        public string Date_Created { get; set; }
        public string Date_Modif { get; set; }
        public string UsernameOrEmail { get; set; }
        // Constructor
        public LoginClass(string IdUser, string Username, string Email, string Password, string Date_Created, string Date_Modif)
        {
            this.IdUser = IdUser;
            this.Username = Username;
            this.Email = Email;
            this.Password = Password;
            this.Date_Created = Date_Created;
            this.Date_Modif = Date_Modif;
        }
        public LoginClass()
        {

        }

        // Methods
        public string GetIdUser() { return IdUser; }
        public string GetUsername() { return Username; }
        public string GetEmail() { return Email; }
        public string GetPassword() { return Password; }
        public string GetDate_Created() { return Date_Created; }
        public string GetDate_Modif() { return Date_Modif; }
    }

    public class LoginContainer
    {

        public LoginClass user;
        public string message { get; set; }
        public string status { get; set; }
    }

}

