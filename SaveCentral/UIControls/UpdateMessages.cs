using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SaveCentral.UIControls
{
    class UpdateMessages
    {
        /// <summary>
        /// Updates a progress bar and a label to inform of the current upload progress.
        /// </summary>
        /// <param name="updateStatus">Integer representing upload progress.</param>
        /// <param name="PB">Progress bar to be updated</param>
        /// <param name="lblUpdateStatus">Label to display string to inform update status.</param>
        /// <returns>True if everything went ok..</returns>
        public static bool UploadMsg(int updateStatus, ProgressBar PB, Label lblUpdateStatus)
        {
            try
            {
                if (updateStatus > 0 && updateStatus <= 100)
                {
                    PB.Value = updateStatus;
                    lblUpdateStatus.Content = "Uploading save...";
                }
                else
                {
                    PB.Value = 0;
                }

                switch (updateStatus)
                {
                    case -1:
                        lblUpdateStatus.Content = "Retrieving files through FTP...";
                        return true;
                    case 0:
                        lblUpdateStatus.Content = "Uploading save...";
                        return true;
                    case 1:
                        lblUpdateStatus.Content = "Uploading save...";
                        return true;
                    case 100:
                        lblUpdateStatus.Content = "File uploaded. Saving info in DB...";
                        return true;
                    case 101:
                        lblUpdateStatus.Content = "The file could not be uploaded. Try again.";
                        return true;
                    case 102:
                        lblUpdateStatus.Content = "An unknown error ocurred. Upload could not be completed.";
                        return true;
                    case 103:
                        lblUpdateStatus.Content = "";
                        return true;
                    default:                  
                        return true;
                }
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// Updates a progress bar and a label to inform of the current download progress.
        /// </summary>
        /// <param name="updateStatus">Integer representing download progress.</param>
        /// <param name="PB">Progress bar to be updated</param>
        /// <param name="lblUpdateStatus">Label to display string to inform update status.</param>
        /// <returns>True if everything went ok..</returns>
        public static bool DownloadMsg(int updateStatus, ProgressBar PB, Label lblUpdateStatus)
        {
            try
            {
                if (updateStatus > 0 && updateStatus <= 100)
                {
                    PB.Value = updateStatus;
                    lblUpdateStatus.Content = "Downloading...";
                }
                else
                {
                    PB.Value = 0;
                }

                switch (updateStatus)
                {
                    case 0:
                        lblUpdateStatus.Content = "Downloading...";
                        return true;
                    case 100:
                        lblUpdateStatus.Content = "Save downloaded. Unzipping files.";
                        return true;
                    case 101:
                        lblUpdateStatus.Content = "The file could not be downloaded. Try again.";
                        return true;
                    case 102:
                        lblUpdateStatus.Content = "An unknown error ocurred. Upload could not be completed."; 
                        return true;
                    case 103:
                        lblUpdateStatus.Content = "";
                        return true;
                    case 104:
                        lblUpdateStatus.Content = "Trying to move files through FTP.";
                        return true;
                    case 105:
                        lblUpdateStatus.Content = "Deleting temp files.";
                        return true;
                    case 106:
                        lblUpdateStatus.Content = "Updating download count.";
                        return true;
                    default:
                        //lblUpdateStatus.Content = i;                   
                        return true;
                }
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// Updates a progress bar and a label to inform of the current update progress.
        /// </summary>
        /// <param name="updateStatus">Integer representing upload progress.</param>
        /// <param name="PB">Progress bar to be updated</param>
        /// <param name="lblUpdateStatus">Label to display string to inform update status.</param>
        /// <returns>True if everything went ok..</returns>
        public static bool UpdateMsg(int updateStatus, ProgressBar PB, Label lblUpdateStatus)
        {
            try
            {
                if (updateStatus > 0 && updateStatus <= 100)
                {
                    PB.Value = updateStatus;
                    lblUpdateStatus.Content = "Updating save...";
                }
                else
                {
                    PB.Value = 0;
                }

                switch (updateStatus)
                {
                    case 10:
                        lblUpdateStatus.Content = "Updating save...";
                        return true;                 
                    case 100:
                        lblUpdateStatus.Content = "Save Updated.";
                        return true;
                    case 101:
                        lblUpdateStatus.Content = "The file could not be updated. Try again.";
                        return true;
                    case 102:
                        lblUpdateStatus.Content = "An unknown error ocurred. Update could not be completed.";
                        return true;
                    case 103:
                        lblUpdateStatus.Content = "";
                        return true;
                    default:
                        return true;
                }
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// Updates a progress bar and a label to inform of the current delete progress.
        /// </summary>
        /// <param name="updateStatus">Integer representing delete progress.</param>
        /// <param name="PB">Progress bar to be updated</param>
        /// <param name="lblUpdateStatus">Label to display string to inform update status.</param>
        /// <returns>True if everything went ok..</returns>
        public static bool DeleteMsg(int updateStatus, ProgressBar PB, Label lblUpdateStatus)
        {
            try
            {
                if (updateStatus > 0 && updateStatus <= 100)
                {
                    PB.Value = updateStatus;
                    lblUpdateStatus.Content = "Deleting save...";
                }
                else
                {
                    PB.Value = 0;
                }

                switch (updateStatus)
                {
                    case 10:
                        lblUpdateStatus.Content = "Deleting save...";
                        return true;
                    case 100:
                        lblUpdateStatus.Content = "Save Deleted.";
                        return true;
                    case 101:
                        lblUpdateStatus.Content = "The file could not be deleted. Try again.";
                        return true;
                    case 102:
                        lblUpdateStatus.Content = "An unknown error ocurred. Delete could not be completed.";
                        return true;
                    case 103:
                        lblUpdateStatus.Content = "";
                        return true;
                    default:
                        return true;
                }
            }
            catch
            {
                return false;
            }

        }
    }
}
