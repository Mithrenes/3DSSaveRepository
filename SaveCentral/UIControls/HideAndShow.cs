using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SaveCentral.UIControls
{
    class HideAndShow
    {
        public static void ShowLogOutbtn(Button btnLogIn, Button btnLogOut, TextBox Usertxt, PasswordBox Passwordtxt, CheckBox ChkBoxRememberAccount, CheckBox chkHeader)
        {
            btnLogIn.Visibility = System.Windows.Visibility.Hidden;
            btnLogOut.Visibility = System.Windows.Visibility.Visible;
            Usertxt.IsReadOnly = true;
            Passwordtxt.IsEnabled = false;
            ChkBoxRememberAccount.IsEnabled = false;
            chkHeader.IsChecked = false;
            chkHeader.IsEnabled = false;
        }
        public static void ShowLogInbtn(Button btnLogIn, Button btnLogOut, TextBox Usertxt, PasswordBox Passwordtxt, CheckBox ChkBoxRememberAccount, CheckBox chkHeader)
        {
            btnLogIn.Visibility = System.Windows.Visibility.Visible;
            btnLogOut.Visibility = System.Windows.Visibility.Hidden;
            Usertxt.IsReadOnly = false;
            Passwordtxt.IsEnabled = true;
            ChkBoxRememberAccount.IsEnabled = true;
            chkHeader.IsEnabled = true;
        }
    }
}
