using FluentFTP;
using SaveCentral.Classes;
using SaveCentral.UIControls;
using SaveCentral.WebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SaveCentral
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isOtherProcRunning = false;
        int countLoadLB = 0;
        string SaveRegionUpload, SaveTypeUpload, SaveRegionMySaves;
        List<ListBoxItems> listGameSaves = new List<ListBoxItems>();
        List<ListBoxItems> listSaveSlots = new List<ListBoxItems>();
        List<ListBoxItems> listMyGameSaves = new List<ListBoxItems>();
        List<ListBoxItems> listMySaveSlots = new List<ListBoxItems>();
        List<ListBoxItems> listGameSavesToDownload = new List<ListBoxItems>();
        List<ListBoxItems> listGSToDownloadFilesIncluded = new List<ListBoxItems>();
        public MainWindow()
        {
            InitializeComponent();
            AutoUpdaterDotNET.AutoUpdater.Start(Constants.URL + "/Updater.xml");
            Misc.Dir.DeleteDir(Constants.TempFolder + System.IO.Path.DirectorySeparatorChar + "TempSaves");
            Misc.Dir.DeleteDir(Constants.TempFolder + System.IO.Path.DirectorySeparatorChar + "TempDL");
            Misc.Utils.ChkIfUpgradeRequired();
            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Tick += ShowServersStatus;
            _timer.Interval = new TimeSpan(0, 0, 0);
            _timer.Start();
            GC.KeepAlive(_timer);
            if (Properties.Settings.Default.SaveAccount == true)
            {
                ChkBoxRememberAccount.IsChecked = true;
                Usertxt.Text = Properties.Settings.Default.User;
                Passwordtxt.Password = Properties.Settings.Default.Password;
            }
            else
            {
                ChkBoxRememberAccount.IsChecked = false;
            }
            if (Properties.Settings.Default.SaveIPAddress)
            {
                chkRememberDeviceIP.IsChecked = true;
                txtDeviceIPAddress.Text = Properties.Settings.Default.DeviceIPAddress;
            }
            else
            {
                chkRememberDeviceIP.IsChecked = false;
            }
            
        }
        private async Task<bool> LoadSaves()
        {
            SQLite.Tables T = new SQLite.Tables();
            T.CreateTables();
            if (File.Exists("Database.db"))
            {
                await Task.Run(() => Download.GetAllSavesFromWeb());
                await GetMySaves();
                await GetAllSaves();
                return true;
            }
            return false;
        }
        private async Task GetMySaves()
        {
            listMyGameSaves = await DynamicControls.GetAllSavesForCurrentUser(Constants.User);
            lbMyGameSaves.ItemsSource = listMyGameSaves;
            lbMyGameSaves.Items.Refresh();
        }
        private async Task GetAllSaves()
        {
            listGameSavesToDownload = await DynamicControls.GetAllSaves();
            lbGameSavesToDownload.ItemsSource = listGameSavesToDownload;
            lbGameSavesToDownload.Items.Refresh();
        }
        private async Task RefreshLB()
        {
            DynamicControls.ResetUpdateAndDLUI(txtDLSaveTitle, txtDLSaveDescription, cmbDLSaveRegion, txtDLSaveType, txtDLHasExtData, txtDLCount, txtDLUploader, lbFilesIncludedInDownload);
            DynamicControls.ResetUpdateAndDLUI(txtMUSaveTitle, txtMUSaveDescription, cmbMUSaveRegion, txtMUSaveType, txtMUHasExtData, null, null, lbMySaveSlotsInGameFolder);
            DynamicControls.ResetUploadUI(txtSaveTitle, txtSaveDescription, cmbSaveRegion, txtHasExtData,lbSaveSlotsInGameFolder, lbGameSavesInDevice);

            await LoadSaves();
        }
        private async Task<bool> UserLogIn()
        {
            if (ChkBoxRememberAccount.IsChecked == true)
            {
                if (!Usertxt.Text.Equals("") || !Passwordtxt.Password.Equals(""))
                {
                    Properties.Settings.Default.User = Usertxt.Text;
                    Properties.Settings.Default.Password = Passwordtxt.Password;
                    Properties.Settings.Default.Save();
                }
                if (await Login.ChkIfLoginPass(Properties.Settings.Default.User, Properties.Settings.Default.Password, lblSessionStatus))
                {
                    TabItemUS.IsEnabled = true;
                    TabItemMS.IsEnabled = true;
                    HideAndShow.ShowLogOutbtn(btnLogIn, btnLogOut, Usertxt, Passwordtxt, ChkBoxRememberAccount, chkHeader);                                                                                          
                    return true;
                }
                else
                {
                    TabSaves.SelectedItem = TabItemDS;
                    TabItemUS.IsEnabled = false;
                    TabItemMS.IsEnabled = false;
                    HideAndShow.ShowLogInbtn(btnLogIn, btnLogOut, Usertxt, Passwordtxt, ChkBoxRememberAccount, chkHeader);
                    return false;
                }
            }
            else
            {
                if (await Login.ChkIfLoginPass(Usertxt.Text, Passwordtxt.Password, lblSessionStatus))
                {
                    TabItemUS.IsEnabled = true;
                    TabItemMS.IsEnabled = true;
                    HideAndShow.ShowLogOutbtn(btnLogIn, btnLogOut, Usertxt, Passwordtxt, ChkBoxRememberAccount, chkHeader);
                    return true;
                }
                else
                {
                    TabSaves.SelectedItem = TabItemDS;
                    TabItemUS.IsEnabled = false;
                    TabItemMS.IsEnabled = false;
                    HideAndShow.ShowLogInbtn(btnLogIn, btnLogOut, Usertxt, Passwordtxt, ChkBoxRememberAccount, chkHeader);
                    return false;
                }
            }
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private async void cmbFilterSaveByRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFilterSaveByRegion.SelectedIndex >=0)
            {
                listGameSavesToDownload = await DynamicControls.LoadWithFilter(cmbFilterSaveByRegion.SelectedValue.ToString(), txtFilterSearch.Text);
                lbGameSavesToDownload.ItemsSource = listGameSavesToDownload;
                lbGameSavesToDownload.Items.Refresh();
                DynamicControls.ResetUpdateAndDLUI(txtDLSaveTitle, txtDLSaveDescription, cmbDLSaveRegion, txtDLSaveType, txtDLHasExtData, txtDLCount, txtDLUploader, lbFilesIncludedInDownload);
            }           
        }
        private async void txtFilterSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string SelValue = "";
            if (cmbFilterSaveByRegion.SelectedIndex >= 0)
            {
                SelValue = cmbFilterSaveByRegion.SelectedValue.ToString();
            }
            listGameSavesToDownload = await DynamicControls.LoadWithFilter(SelValue, txtFilterSearch.Text);
            lbGameSavesToDownload.ItemsSource = listGameSavesToDownload;
            lbGameSavesToDownload.Items.Refresh();
            DynamicControls.ResetUpdateAndDLUI(txtDLSaveTitle, txtDLSaveDescription, cmbDLSaveRegion, txtDLSaveType, txtDLHasExtData, txtDLCount, txtDLUploader, lbFilesIncludedInDownload);
        }
        private async void lbGameSavesToDownload_SelChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbGameSavesToDownload.SelectedIndex;
            if (lbGameSavesToDownload.HasItems && index >= 0)
            {
                listGSToDownloadFilesIncluded = await DynamicControls.GetFilesIncludedFromLocalDB(listGameSavesToDownload[index].Username, listGameSavesToDownload[index].FileName, txtDLSaveTitle, txtDLSaveDescription, cmbDLSaveRegion, txtDLSaveType, txtDLHasExtData, txtDLCount, txtDLUploader);
                if (listGSToDownloadFilesIncluded == null)
                {
                    lbFilesIncludedInDownload.ItemsSource = null;
                }
                else
                {
                    lbFilesIncludedInDownload.ItemsSource = listGSToDownloadFilesIncluded;
                }
                lbFilesIncludedInDownload.Items.Refresh();
            }
        }
        private async void lbGameSavesToDownload_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            isOtherProcRunning = true;
            int index = lbGameSavesToDownload.SelectedIndex;
            gridUpload.IsEnabled = false;
            TabSaves.IsEnabled = false;
            if (index >= 0)
            {
                if(await Mega.DownloadFilesAsync(listGameSavesToDownload[index].FileName, Constants.TempFolder, txtDeviceIPAddress.Text, PBNotifyProgress, lblNotifyProgress))
                {
                    UpdateMessages.DownloadMsg(106, PBNotifyProgress, lblNotifyProgress);
                    listGameSavesToDownload[index].DLCount = txtDLCount.Text = await Download.GetDLCountFromWeb(listGameSavesToDownload[index].FileName, txtDLCount.Text);
                    UpdateMessages.DownloadMsg(103, PBNotifyProgress, lblNotifyProgress);
                    MessageBox.Show("Save downloaded and transferred successfully to Device.");
                }              
            }
            gridUpload.IsEnabled = true;
            TabSaves.IsEnabled = true;            
            isOtherProcRunning = false;
        }
        private async void lbGameSavesInDevice_SelChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbGameSavesInDevice.SelectedIndex;
            if (lbGameSavesInDevice.HasItems && index >= 0)
            {
                listSaveSlots = await DynamicControls.GetGSavesFromDevice(txtDeviceIPAddress.Text, listGameSaves[index].FilePath);
                if (listGameSaves == null)
                {
                    lbSaveSlotsInGameFolder.ItemsSource = null;
                }
                else
                {
                    lbSaveSlotsInGameFolder.ItemsSource = listSaveSlots;
                }
                lbSaveSlotsInGameFolder.Items.Refresh();
            }
        }
        private void lbSaveSlotsInGameFolder_SelChanged(object sender, SelectionChangedEventArgs e)
        {
                    
        }
       
        public async void ShowServersStatus(object sender, EventArgs e)
        {
            ((DispatcherTimer)sender).Interval = new TimeSpan(0, 0, 30);
            if (await GetServersAvailability())
            {
                if (countLoadLB == 0)
                {
                    await LoadSaves();//<-- ONLY LOAD THIS ONE OR THERE IS A HIGH CHANCE TO GET A CRASH ERROR FOR THE LISTBOXES
                    countLoadLB++;
                }
            }
        }
        public async Task<bool> GetServersAvailability()
        {
            if (await Task.Run(() => Misc.Utils.IsServerOnline()))
            {
                EllipseIsServerOnline.Fill = Brushes.Green;
                if (await Task.Run(() => Misc.Utils.IsMegaOnline()))
                {
                    EllipseIsMegaOnline.Fill = Brushes.Green;
                    if (countLoadLB == 0) { await UserLogIn(); }
                    if (!isOtherProcRunning) { TabSaves.IsEnabled = true; }
                    return true;
                }
                else
                {
                    EllipseIsMegaOnline.Fill = Brushes.Red;
                    if (!isOtherProcRunning) { TabSaves.IsEnabled = false; }
                    lblSessionStatus.Content = Constants.lblErrLogin;
                    lblSessionStatus.Background = Brushes.LightSalmon;
                    lblSessionStatus.FontWeight = FontWeights.Bold;
                    return false;
                }
            }
            else
            {
                if (!isOtherProcRunning) { TabSaves.IsEnabled = false; }
                EllipseIsServerOnline.Fill = Brushes.Red;
                EllipseIsMegaOnline.Fill = Brushes.Red;
                lblSessionStatus.Content = Constants.lblErrLogin;
                lblSessionStatus.Background = Brushes.LightSalmon;
                lblSessionStatus.FontWeight = FontWeights.Bold;
                await UserLogIn();
            }
            return false;
        }
        private void btnRightMenuHide_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbHideRightMenu", btnRightMenuHide, btnRightMenuShow, pnlRightMenu);
        }
        private void btnRightMenuShow_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbShowRightMenu", btnRightMenuHide, btnRightMenuShow, pnlRightMenu);
        }
        private void ShowHideMenu(string _Storyboard, Button btnHide, Button btnShow, StackPanel pnl)
        {
            Storyboard sb = Resources[_Storyboard] as Storyboard;
            sb.Begin(pnl);

            if (_Storyboard.Contains("Show"))
            {
                btnHide.Visibility = Visibility.Visible;
                btnShow.Visibility = Visibility.Hidden;
            }
            else if (_Storyboard.Contains("Hide"))
            {
                btnHide.Visibility = Visibility.Hidden;
                btnShow.Visibility = Visibility.Visible;
            }
        }
        private void ChkBoxRememberAccount_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UserSettings.RememberLogin(ChkBoxRememberAccount, Usertxt, Passwordtxt);
        }
        private async void btnCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (Misc.Utils.IsValidEmail(txtNewEmail.Text) && !txtNewNomUser.Text.Equals("") && !txtNewPassword.Password.Equals(""))
            {
                btnCreateAccount.IsEnabled = false;
                if (await Insert.CreateNewAccount(txtNewNomUser.Text, txtNewEmail.Text, txtNewPassword.Password))
                {
                    ChkBoxRememberAccount.IsChecked = false;
                    Usertxt.Text = txtNewNomUser.Text;
                    Passwordtxt.Password = txtNewPassword.Password;
                    ChkBoxRememberAccount.IsChecked = true;
                    txtNewNomUser.Text = "";
                    txtNewEmail.Text = "";
                    txtNewPassword.Password = "";
                    chkHeader.IsChecked = false;
                    btnCreateAccount.IsEnabled = true;
                    ShowHideMenu("sbHideRightMenu", btnRightMenuHide, btnRightMenuShow, pnlRightMenu);
                    await UserLogIn();
                    
                    await RefreshLB();
                }
            }
            else
            {
                MessageBox.Show("Username, email and/or password are not valid.");
            }
            btnCreateAccount.IsEnabled = true;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as CheckBox).Name != "chkHeader")
            {
                e.Handled = true;
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as CheckBox).Name != "chkHeader")
            {
                e.Handled = true;
            }
        }
        private async void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            if (Usertxt.Text.Equals("") || Passwordtxt.Password.Equals(""))
            {
                MessageBox.Show("User an password cannot be left black.");
                return;
            }
            btnLogIn.IsEnabled = false;
            try
            {
                if (await UserLogIn() == false)
                {
                    MessageBox.Show("Username and/or email are incorrect. (Or the server is't working properly)");
                }
                btnLogIn.IsEnabled = true;
            }
            catch
            {
                btnLogIn.IsEnabled = true;
            }
            await RefreshLB();
        }
        private async void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            await UserLogOut();
            await RefreshLB();
        }
        private async Task UserLogOut()
        {
            btnLogOut.IsEnabled = false;
            try
            {
                if (await Login.ChkIfLoginPass("", "", lblSessionStatus))
                {
                    TabItemUS.IsEnabled = true;
                    TabItemMS.IsEnabled = true;
                    HideAndShow.ShowLogOutbtn(btnLogIn, btnLogOut, Usertxt, Passwordtxt, ChkBoxRememberAccount, chkHeader);
                }
                else
                {
                    TabSaves.SelectedItem = TabItemDS;
                    TabItemUS.IsEnabled = false;
                    TabItemMS.IsEnabled = false;
                    HideAndShow.ShowLogInbtn(btnLogIn, btnLogOut, Usertxt, Passwordtxt, ChkBoxRememberAccount, chkHeader);
                }
                btnLogOut.IsEnabled = true;
            }
            catch
            {
                btnLogOut.IsEnabled = true;
            }
        }

        private void chkRememberDeviceIP_Checked(object sender, RoutedEventArgs e)
        {
            UserSettings.RememberDeviceIP(chkRememberDeviceIP, txtDeviceIPAddress);

        }

        private async void btnFTPConnect_Click(object sender, RoutedEventArgs e)
        {
            listGameSaves = await DynamicControls.GetGSavesFromDevice(txtDeviceIPAddress.Text, "/JKSV/Saves");
            if (listGameSaves == null)
            {
                lbGameSavesInDevice.ItemsSource = null;
                SaveTypeUpload = "";
                txtSaveType.Text = "";
            }
            else
            {
                SaveTypeUpload = ((int)Constants.SaveType.N3DS).ToString();
                txtSaveType.Text = "Nintendo 3DS";
                lbGameSavesInDevice.ItemsSource = listGameSaves;
            }
            ShowHideMenu("sbHideRightMenu", btnRightMenuHide, btnRightMenuShow, pnlRightMenu);
            lbGameSavesInDevice.Items.Refresh();
        }
        

        private async void btnUploadSave_Click(object sender, RoutedEventArgs e)
        {
            if (lbSaveSlotsInGameFolder.HasItems && lbSaveSlotsInGameFolder.SelectedIndex >= 0 )
            {
                if (!txtSaveTitle.Text.Equals(""))
                {
                    if (!txtSaveDescription.Equals(""))
                    {
                        if (cmbSaveRegion.SelectedIndex >= 0)
                        {
                            isOtherProcRunning = true;
                            gridUpload.IsEnabled = false;
                            TabSaves.IsEnabled = false;
                            if (await FTP.DownloadFilesToLocal(txtDeviceIPAddress.Text, listSaveSlots[lbSaveSlotsInGameFolder.SelectedIndex].FilePath, txtSaveTitle.Text, txtSaveDescription.Text, SaveRegionUpload, SaveTypeUpload, txtHasExtData, PBNotifyProgress, lblNotifyProgress))
                            {
                                DynamicControls.ResetUploadUI(txtSaveTitle, txtSaveDescription, cmbSaveRegion, txtHasExtData, lbSaveSlotsInGameFolder, lbGameSavesInDevice);
                                await LoadSaves();
                            }
                            //Deleting TempFolder once the upload is done.
                            Misc.Dir.DeleteDir(Constants.TempFolder + System.IO.Path.DirectorySeparatorChar + "TempSaves");
                            gridUpload.IsEnabled = true;
                            TabSaves.IsEnabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Select the region for the save you are uploading.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("You must write a description for the save to upload. Say % of completion, achievements unlocket, etc...");
                    }
                }
                else
                {
                    MessageBox.Show("You must write a title for the save to upload. Choose something brief and descriptive.");
                }
            }
            else
            {
                MessageBox.Show("You must select one save slot to upload.");
            }
            isOtherProcRunning = false;
            gridUpload.IsEnabled = true;
            TabSaves.IsEnabled = true;
        }

        private void cmbSaveRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbSaveRegion.SelectedIndex >= 0)
            {
                SaveRegionUpload = DynamicControls.GetDeviceRegion(cmbSaveRegion.SelectedValue.ToString());               
            }
            else
            {
                SaveRegionUpload = "";
            }           
        }
        private void cmbMUSaveRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbMUSaveRegion.HasItems && cmbMUSaveRegion.SelectedIndex >= 0)
            {
                SaveRegionMySaves = DynamicControls.GetDeviceRegion(cmbMUSaveRegion.SelectedValue.ToString());              
            }
            else
            {
                SaveRegionMySaves = "";
            }
        }

        private async void btnUpdateMySaves_Click(object sender, RoutedEventArgs e)
        {
            if (lbMyGameSaves.HasItems && lbMyGameSaves.SelectedIndex >= 0)
            {
                if (!txtMUSaveTitle.Text.Equals(""))
                {
                    if (!txtMUSaveDescription.Equals(""))
                    {
                        if (cmbMUSaveRegion.SelectedIndex >= 0)
                        {
                            isOtherProcRunning = true;
                            gridMyUploads.IsEnabled = false;
                            TabSaves.IsEnabled = false;
                            SavesClass SV = new SavesClass();
                            SV.FileName = listMyGameSaves[lbMyGameSaves.SelectedIndex].FileName;
                            SV.Title = txtMUSaveTitle.Text;
                            SV.Description = txtMUSaveDescription.Text;
                            SV.Region = SaveRegionMySaves;                            
                            if (await Update.UpdateSaveInfo(SV, PBNotifyProgress, lblNotifyProgress))
                            {
                                DynamicControls.ResetUploadUI(txtMUSaveTitle, txtMUSaveDescription, cmbMUSaveRegion, txtMUHasExtData, lbMySaveSlotsInGameFolder, lbMyGameSaves);
                                await LoadSaves();
                            }
                            gridMyUploads.IsEnabled = true;
                            TabSaves.IsEnabled = true;
                        }
                        else
                        {
                            MessageBox.Show("Select the region for the save you are updating.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("You must write a description for the save you want to update. Say % of completion, achievements unlocket, etc...");
                    }
                }
                else
                {
                    MessageBox.Show("You must write a title for the save you want to update. Choose something brief and descriptive.");
                }
            }
            else
            {
                MessageBox.Show("You must select one save to update.");
            }
            isOtherProcRunning = false;
            gridMyUploads.IsEnabled = true;
            TabSaves.IsEnabled = true;
        }
        private async void lbMyGameSaves_SelChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = lbMyGameSaves.SelectedIndex;
            if (lbMyGameSaves.HasItems && index >= 0)
            {
                listMySaveSlots = await DynamicControls.GetFilesIncludedFromLocalDB(listMyGameSaves[index].Username, listMyGameSaves[index].FileName, txtMUSaveTitle, txtMUSaveDescription, cmbMUSaveRegion, txtMUSaveType, txtMUHasExtData, null, null);
                if (listMySaveSlots == null)
                {
                    lbMySaveSlotsInGameFolder.ItemsSource = null;
                }
                else
                {
                    lbMySaveSlotsInGameFolder.ItemsSource = listMySaveSlots;
                }
                lbMySaveSlotsInGameFolder.Items.Refresh();
            }
        }

        private async void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbMyGameSaves.SelectedIndex == -1)
            {
                return;
            }
            else
            {
                isOtherProcRunning = true;
                gridMyUploads.IsEnabled = false;
                TabSaves.IsEnabled = false;
                SavesClass SV = new SavesClass();
                SV.FileName = listMyGameSaves[lbMyGameSaves.SelectedIndex].FileName;
                if(await Delete.DeleteSelSave(SV, PBNotifyProgress, lblNotifyProgress))
                {
                    await RefreshLB();
                }
                isOtherProcRunning = false;
                gridMyUploads.IsEnabled = true;
                TabSaves.IsEnabled = true;
            }           
        }
        private async void MenuItemUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lbMyGameSaves.SelectedIndex == -1)
            {
                return;
            }
            else
            {
                if (lbMyGameSaves.HasItems && lbMyGameSaves.SelectedIndex >= 0)
                {
                    if (!txtMUSaveTitle.Text.Equals(""))
                    {
                        if (!txtMUSaveDescription.Equals(""))
                        {
                            if (cmbMUSaveRegion.SelectedIndex >= 0)
                            {
                                isOtherProcRunning = true;
                                gridMyUploads.IsEnabled = false;
                                TabSaves.IsEnabled = false;

                                //AGREGAR FilePath = FTPitem.FullName, al listMyGameSaves (desde el ftp) para poder saber desde donde debo recobrar los archivos para hacer upload y luego update.
                                listMyGameSaves[lbMyGameSaves.SelectedIndex].FilePath = await Mega.DownloadFilesAsyncForUpdate(listMyGameSaves[lbMyGameSaves.SelectedIndex].FileName, Constants.TempFolder, PBNotifyProgress, lblNotifyProgress);
                                if (listMyGameSaves[lbMyGameSaves.SelectedIndex].FilePath != null && await FTP.DownloadFilesToLocal(txtDeviceIPAddress.Text, listMyGameSaves[lbMyGameSaves.SelectedIndex].FilePath, txtMUSaveTitle.Text, txtMUSaveDescription.Text, SaveRegionMySaves, null, txtMUHasExtData, PBNotifyProgress, lblNotifyProgress))
                                {
                                    DynamicControls.ResetUploadUI(txtMUSaveTitle, txtMUSaveDescription, cmbMUSaveRegion, txtMUHasExtData, lbMySaveSlotsInGameFolder, lbMyGameSaves);
                                    await LoadSaves();
                                }
                                //Deleting TempFolder once the upload is done.
                                Misc.Dir.DeleteDir(Constants.TempFolder + System.IO.Path.DirectorySeparatorChar + "TempSaves");
                                gridMyUploads.IsEnabled = true;
                                TabSaves.IsEnabled = true;
                            }
                            else
                            {
                                MessageBox.Show("Select the region for the save you are updating.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("You must write a description for the save you want to update. Say % of completion, achievements unlocket, etc...");
                        }
                    }
                    else
                    {
                        MessageBox.Show("You must write a title for the save you want to update. Choose something brief and descriptive.");
                    }
                }
                else
                {
                    MessageBox.Show("You must select one save to update.");
                }
                isOtherProcRunning = false;
                gridMyUploads.IsEnabled = true;
                TabSaves.IsEnabled = true;
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.RememberDeviceIP(chkRememberDeviceIP, txtDeviceIPAddress);
            Misc.Dir.DeleteDir(Constants.TempFolder + System.IO.Path.DirectorySeparatorChar + "TempSaves");
            Misc.Dir.DeleteDir(Constants.TempFolder + System.IO.Path.DirectorySeparatorChar + "TempDL");
            base.OnClosing(e);
        }
    }
}
