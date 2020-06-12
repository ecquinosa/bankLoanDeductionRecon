using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace bankLoanDeductionRecon
{
      

    class Program
    {

        private static string configFile = AppDomain.CurrentDomain.BaseDirectory + "config";
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();        
        private static Config config;
        
        public enum bankID
        {
            UBP = 1,
            AUB
        }

        static void Main()
        {
            logger.Info("Application started");

            //validatations
            Console.WriteLine(DateTime.Now.ToString("MM/dd/yy hh:mm:ss ") + "Initializing...");
            if (!Init()) return;

            ProcessEODList();

            logger.Info("Application closed");
        }

        private static bool Init()
        {
            DAL dal = null;
            try
            {
                //check if another instance is running
                if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                {
                    logger.Info("Another instrance is running. Application will be closed.");
                    return false;
                }

                //check if file exists
                if (!File.Exists(configFile))
                {
                    logger.Error("Config file is missing");
                    return false;
                }

                try
                {
                    config = new Config();
                    var configData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Config>>(File.ReadAllText(configFile));
                    config = configData[0];
                    //dal.ConStr = config.DbaseConStr;
                }
                catch (Exception ex)
                {
                    logger.Error("Error reading config file. Runtime catched error " + ex.Message);
                    return false;
                }

                //check dbase connection
                dal = new DAL(config);
                if (!dal.IsConnectionOK())
                {
                    logger.Error("Connection to database failed. " + dal.ErrorMessage);
                    return false;
                }
                dal.Dispose();
                dal = null;

            }
            catch (Exception ex)
            {
                logger.Error("Runtime catched error " + ex.Message);
                return false;
            }

            return true;
        }

        private static bool ProcessEODList()
        {
            EOD eod = new EOD(logger);         

            

            if (config.IsDownloadFromSFTP == 0) logger.Info("IsDownloadFromSFTP is set to " + config.IsDownloadFromSFTP.ToString());
            else
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yy hh:mm:ss ") + "Downloading files from sftp...");
                if(!eod.DownloadFilesFromSftp(config)) return false;
            }

            foreach (var file in Directory.GetFiles(config.DownloadFolder))
            {
                Console.WriteLine(DateTime.Now.ToString("MM/dd/yy hh:mm:ss ") + "Populating data...");
                bool response = false;

                if (config.BankID == (short)bankID.UBP) response = eod.PopulateDataTXT(file);
                else if (config.BankID == (short)bankID.AUB) response = eod.PopulateDataXLS(file);

                if (response)
                {
                    Console.WriteLine(DateTime.Now.ToString("MM/dd/yy hh:mm:ss ") + "Uploading data...");
                    eod.Upload(config);

                    //housekeeping
                    string uploadedFolder = string.Format(@"{0}\{1}", config.DownloadFolder, "Uploaded");
                    if (!Directory.Exists(uploadedFolder)) Directory.CreateDirectory(uploadedFolder);
                    File.Move(file, string.Format(@"{0}\{1}", uploadedFolder, Path.GetFileName(file)));
                }
            }


            eod = null;
            
            return true;
        }

        
    }
}
