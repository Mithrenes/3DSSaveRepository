using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SaveCentral
{
    class UserSettings
    {
        public static void RememberLogin(CheckBox ChkBoxRememberAccount, TextBox Usertxt, PasswordBox Passwordtxt)
        {
            try
            {
                if (ChkBoxRememberAccount.IsChecked == true && !Passwordtxt.Password.Equals(""))
                {
                    Properties.Settings.Default.User = Usertxt.Text;
                    Properties.Settings.Default.Password = Passwordtxt.Password;
                    Properties.Settings.Default.SaveAccount = true;
                    Properties.Settings.Default.Save();
                }
                else if (ChkBoxRememberAccount.IsChecked == false)
                {
                    Properties.Settings.Default.User = "";
                    Properties.Settings.Default.Password = "";
                    Passwordtxt.Password = "";
                    Properties.Settings.Default.SaveAccount = false;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            
        }
        public static void RememberDeviceIP(CheckBox chkRememberDeviceIP, TextBox txtDeviceIPAddress)
        {
            try
            {
                if (chkRememberDeviceIP.IsChecked == true)
                {
                    Properties.Settings.Default.DeviceIPAddress = txtDeviceIPAddress.Text;
                    Properties.Settings.Default.SaveIPAddress = true;
                    Properties.Settings.Default.Save();
                    txtDeviceIPAddress.Text = Properties.Settings.Default.DeviceIPAddress;
                }
                else
                {
                    Properties.Settings.Default.DeviceIPAddress = "";
                    Properties.Settings.Default.SaveIPAddress = false;
                    Properties.Settings.Default.Save();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
