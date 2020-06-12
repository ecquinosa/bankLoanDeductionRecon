using System;
using System.Collections.Generic;
using WinSCP;
using System.IO;


namespace bankLoanDeductionRecon
{
    class SFTP
    {

        public string ErrorMessage { get; set; }

        private delegate void dlgtProcess(); 
        private Config config;

        public SFTP(Config config)
        {
            this.config = config;
        }

        private SessionOptions sessionOptions()
        {
            return new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = config.SftpHost,
                UserName = config.SftpUser,
                Password = config.SftpPass,
                PortNumber = Convert.ToInt32(config.SftpPort),
                SshHostKeyFingerprint = config.SftpSshHostKeyFingerprint
            };
        }       

        public bool DownloadFiles()
        {
            try
            {  
                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions());

                    // Download files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult =
                        session.GetFiles(config.SftpRemotePath, string.Format(@"{0}\",config.DownloadFolder), false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Console.WriteLine(DateTime.Now.ToString("MM/dd/yy hh:mm:ss ") + "Download of {0} succeeded", transfer.FileName);
                    }
                }                

                return true;
            }
            catch (Exception e)
            {
                ErrorMessage = e.Message;
                return false;
            }
        }

        //public bool Upload_SFTP_Files(Cstring path, bool IsZip, ref string errMsg)
        //{
        //    try
        //    {
        //        int intFileCount = Directory.GetFiles(SFTP_LOCALPATH).Length;

        //        if (intFileCount == 0)
        //        {
        //            errMsg = string.Format("[Upload] {0} is empty. No file to push.", SFTP_LOCALPATH);                  
        //            return false;
        //        }             

        //        using (Session session = new Session())
        //        {                                 
        //            session.DisableVersionCheck = true;

        //            session.Open(sessionOptions());

        //            // Upload files
        //            TransferOptions transferOptions = new TransferOptions();
        //            transferOptions.TransferMode = TransferMode.Binary;
        //            //transferOptions.ResumeSupport.State = TransferResumeSupportState.Smart;                  

        //            //transferOptions.PreserveTimestamp = false;

        //            //Console.Write(AppDomain.CurrentDomain.BaseDirectory);
        //            string remotePath = SFTP_REMOTEPATH_ZIP;
        //            if (!IsZip) remotePath = SFTP_REMOTEPATH_PAGIBIGMEMU;

        //             TransferOperationResult transferResult = null;
        //            if (File.Exists(path))
        //            {
        //                {
        //                    if (!session.FileExists(remotePath + Path.GetFileName(path)))
        //                    {
        //                        transferResult = session.PutFiles(string.Format(@"{0}*", path), remotePath, false, transferOptions);
        //                    }

        //                    else
        //                    {
        //                        errMsg = string.Format("Upload_SFTP_Files(): Remote file exist " + Path.GetFileName(path));                               
        //                        return false;
        //                    }
        //                }
        //            }
        //              else

        //                transferResult = session.PutFiles(string.Format(@"{0}\*", SFTP_LOCALPATH), remotePath, false, transferOptions);


        //                // Throw on any error
        //                transferResult.Check();

        //                // Print results
        //                foreach (TransferEventArgs transfer in transferResult.Transfers)
        //                {
        //                    //Console.WriteLine(TimeStamp() + Path.GetFileName(transfer.FileName) + " transferred successfully");
        //                    //string strFilename = Path.GetFileName(transfer.FileName);
        //                    //File.Delete(transfer.FileName);
        //                }                        
        //            }

        //        //Console.WriteLine("Success sftp transfer " + path);
        //        //System.Threading.Thread.Sleep(100);

        //        return true;

        //    }                            
        //    catch (Exception ex)
        //    {
        //        errMsg = string.Format("Upload_SFTP_Files(): Runtime error {0}", ex.Message);
        //        Console.WriteLine(errMsg);
        //        //Utilities.WriteToRTB(errMsg, ref rtb, ref tssl);
        //        return false;
        //    }
        //}

        //private static string BANK_REPO = "";
        //private static System.Text.StringBuilder sbDone = new System.Text.StringBuilder();
        //private static int TotalSftpTransfer;

        //public bool SynchronizeDirectories(string bank_repo, ref string errMsg, ref int _TotalSftpTransfer)
        //{
        //    try
        //    {
        //        BANK_REPO = bank_repo;                

        //        string forTransferFolder = SFTP_LOCALPATH + "\\FOR_TRANSFER";                

        //        int intFileCount = Directory.GetFiles(forTransferFolder).Length;

        //        //if (intFileCount == 0)
        //        //{
        //        //    errMsg = string.Format("[Upload] {0} is empty. No file to push.", forTransferFolder);                    
        //        //    return false;
        //        //}                

        //        using (Session session = new Session())
        //        {
        //            TransferOptions transferOptions = new TransferOptions();
        //            transferOptions.TransferMode = TransferMode.Binary;
        //            transferOptions.FilePermissions = null;
        //            transferOptions.PreserveTimestamp = false;


        //            // Will continuously report progress of synchronization
        //            session.FileTransferred += FileTransferred;                    

        //            // Connect
        //            session.Open(sessionOptions());                    

        //            // Synchronize files
        //            SynchronizationResult synchronizationResult;                 
        //            synchronizationResult = session.SynchronizeDirectories(SynchronizationMode.Remote, @forTransferFolder, SFTP_REMOTEPATH_ZIP, false, false, SynchronizationCriteria.None, transferOptions);                    

        //            // Throw on any error
        //            synchronizationResult.Check();
        //        }

        //        _TotalSftpTransfer = TotalSftpTransfer;

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        errMsg = string.Format("SynchronizeDirectories(): Runtime error {0}", ex.Message);
        //        Console.WriteLine(errMsg);                
        //        return false;
        //    }
        //}        

        //private static void FileTransferred(object sender, TransferEventArgs e)
        //{
        //    string msg = "";
        //    if (e.Error == null)
        //    {
        //        msg = string.Format("{0}Upload of {1} succeeded", Utilities.TimeStamp(), Path.GetFileName(e.FileName));
        //        Console.WriteLine(msg);
        //        Utilities.SaveToSystemLog(msg);

        //        //sbDone.AppendLine(Path.GetFileName(e.FileName));
        //        string destiFile = e.FileName.Replace("FOR_TRANSFER", "DONE");
        //        if (File.Exists(destiFile))
        //        {
        //            string destiFileExisting = string.Format("{0}_{1}.zip", Path.GetFileNameWithoutExtension(destiFile), new FileInfo(destiFile).CreationTime.ToString("yyyyMMdd_hhmmss"));
        //            if(!File.Exists(destiFileExisting))File.Move(destiFile, destiFileExisting);
        //        }                

        //        File.Move(e.FileName, destiFile);

        //        //string doneIDs = "";
        //        //if (System.IO.File.Exists(Utilities.DoneIDsFile())) doneIDs = System.IO.File.ReadAllText(Utilities.DoneIDsFile());
        //        //if (doneIDs == "") Utilities.SaveToDoneIDs(Path.GetFileNameWithoutExtension(e.FileName));
        //        //else Utilities.SaveToDoneIDs("," + Path.GetFileNameWithoutExtension(e.FileName));

        //        TotalSftpTransfer += 1;
        //        System.Threading.Thread.Sleep(100);
        //    }
        //    else
        //    {
        //        msg = string.Format("{0}Upload of {1} failed: {2}", Utilities.TimeStamp(), Path.GetFileName(e.FileName), e.Error);
        //        Console.WriteLine(msg);
        //        Utilities.SaveToErrorLog(msg);                
        //    }

        //    if (e.Chmod != null)
        //    {
        //        if (e.Chmod.Error == null)
        //        {
        //            msg = string.Format("{0}Permissions of {1} set to {2}", Utilities.TimeStamp(), Path.GetFileName(e.Chmod.FileName), e.Chmod.FilePermissions);
        //            Console.WriteLine(msg);
        //            Utilities.SaveToSystemLog(msg);
        //        }
        //        else
        //        {                    
        //            msg = string.Format("{0}Setting permissions of {1} failed: {2}", Utilities.TimeStamp(), Path.GetFileName(e.Chmod.FileName), e.Chmod.Error);
        //            Console.WriteLine(msg);
        //            Utilities.SaveToErrorLog(msg);
        //        }
        //    }
        //    else
        //    {
        //        //Console.WriteLine("{0}Permissions of {1} kept with their defaults", TimeStamp(), e.Destination);
        //    }

        //    if (e.Touch != null)
        //    {
        //        if (e.Touch.Error == null)
        //        {                    
        //            msg = string.Format("{0}Timestamp of {1} set to {2}", Utilities.TimeStamp(), Path.GetFileName(e.Touch.FileName), e.Touch.LastWriteTime);
        //            Console.WriteLine(msg);
        //            Utilities.SaveToSystemLog(msg);
        //        }
        //        else
        //        {                    
        //            msg = string.Format("{0}Setting timestamp of {1} failed: {2}", Utilities.TimeStamp(), Path.GetFileName(e.Touch.FileName), e.Touch.Error);
        //            Console.WriteLine(msg);
        //            Utilities.SaveToErrorLog(msg);
        //        }
        //    }
        //    else
        //    {
        //        // This should never happen during "local to remote" synchronization                
        //        msg = string.Format("{0}Timestamp of {1} kept with its default (current time)", Utilities.TimeStamp(), e.Destination);
        //        Console.WriteLine(msg);
        //        Utilities.SaveToErrorLog(msg);
        //    }
        //}




    }
}
