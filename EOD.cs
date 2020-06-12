using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace bankLoanDeductionRecon
{
    class EOD
    {                
        public string PagIBIGID { get; set; }
        public string ActualTxn_Date { get; set; }
        public string Processed_Date { get; set; }
        public string Processed_Time { get; set; }
        public string AcctNo { get; set; }
        public string BranchCode { get; set; }
        public string Transaction_Amount { get; set; }
        public string PaymentRefNo { get; set; }
        public string Remarks { get; set; }
        public string ReferenceFile { get; set; }

        public string ErrorMessage { get; set; }
        private NLog.Logger logger;

        public List<EOD> eodList = new List<EOD>();       

        public EOD(NLog.Logger logger)
        {
            this.logger = logger;   
        }

        public bool DownloadFilesFromSftp(Config config)
        {           
            if (!Directory.Exists(config.DownloadFolder)) Directory.CreateDirectory(config.DownloadFolder);

            SFTP sftp = new SFTP(config);
            try
            {
                if (!sftp.DownloadFiles())
                {
                    logger.Error(sftp.ErrorMessage.Replace("\r", " "));
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            finally
            {
                sftp = null;
            }

            return true;
        }

        public bool PopulateDataTXT(string filePath)
        {
            eodList.Clear();
            EOD eod = null;
            int totalRecord = 0;
            bool IsValidHeaderFound = false;

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line.Trim() != "")
                        {
                            if (line.Trim().Contains("Total")) { }
                            else if (line.Trim().Contains("Pag-IBIG MID	    ")) { IsValidHeaderFound = true; }
                            else
                            {
                                if (IsValidHeaderFound)
                                {                                    
                                    eod = new EOD(logger);
                                    eod.PagIBIGID = line.Substring(0, 12).Trim();
                                    eod.ActualTxn_Date = line.Substring(40, 20).Trim();
                                    eod.Processed_Date = line.Substring(20, 20).Trim();
                                    eod.Processed_Time = DateTime.Now.ToString("hh:mm:ss");
                                    eod.AcctNo = "";
                                    eod.BranchCode = "";
                                    eod.Remarks = "";
                                    eod.PaymentRefNo = line.Substring(60, 20).Trim();
                                    eod.Transaction_Amount = line.Substring(80).Trim();
                                    eod.ReferenceFile = Path.GetFileName(filePath);
                                    eodList.Add(eod);
                                    eod = null;
                                    totalRecord += 1;
                                }
                            }                            
                        }
                    }
                    sr.Dispose();
                    sr.Close();
                }

                logger.Info(string.Format("File {0} Total record {1}", Path.GetFileName(filePath), totalRecord.ToString("N0")));

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
            finally
            {
                if (eod != null) eod = null;
            }
        }

        public bool PopulateDataXLS(string filePath)
        {
            eodList.Clear();
            EOD eod = null;
            int totalRecord = 0;

            try
            {
                //readXLS(filePath);
                System.Data.DataTable dt = SourceData(filePath);
                if (dt != null)
                {
                    foreach (System.Data.DataRow rw in dt.Rows)
                    {
                        if (rw[7].ToString() == "") { }
                        else if (rw[7].ToString().Contains("Common")) { }
                        else
                        {
                            eod = new EOD(logger);
                            eod.PagIBIGID = rw[0].ToString();
                            eod.ActualTxn_Date = rw[1].ToString();
                            eod.Processed_Date = rw[2].ToString();
                            eod.Processed_Time = rw[3].ToString();
                            eod.AcctNo = rw[4].ToString();
                            eod.BranchCode = rw[5].ToString();
                            eod.Transaction_Amount = rw[6].ToString();
                            eod.PaymentRefNo = rw[7].ToString();
                            eod.Remarks = rw[8].ToString();
                            eod.ReferenceFile = Path.GetFileName(filePath);
                            eodList.Add(eod);
                            eod = null;
                            totalRecord += 1;
                        }                        
                    }                    
                }

                logger.Info(string.Format("File {0} Total record {1}", Path.GetFileName(filePath), totalRecord.ToString("N0")));

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return false;
            }
            finally
            {
                if (eod != null) eod = null;
            }
        }

        public bool Upload(Config config)
        {
            DAL dal = new DAL(config);
            int successDBInsert = 0;
            int failedDBInsert = 0;
            int successWS = 0;
            int failedWS = 0;                
            foreach (var eod in eodList)
            {
                try
                {
                    if (dal.Check_LoanDeductionIfExist(eod))
                    {
                        if (Convert.ToInt32(dal.ObjectResult) == 0)
                        {
                            if (dal.Add_LoanDeductionRecon(eod, config.BankID))
                            {
                                logger.Info(string.Format("MID {0} PaymentRefNo {1} ReferenceFile {2}. Inserted successfully.", eod.PagIBIGID, eod.PaymentRefNo, eod.ReferenceFile, dal.ErrorMessage));
                                successDBInsert += 1;                                
                            }
                            else
                            {
                                logger.Error(string.Format("MID {0} PaymentRefNo {1} ReferenceFile {2}. DAL error {3}", eod.PagIBIGID, eod.PaymentRefNo, eod.ReferenceFile, dal.ErrorMessage));
                                failedDBInsert += 1;
                            }
                        }
                        else
                        {                            
                            logger.Error(string.Format("MID {0} PaymentRefNo {1} ReferenceFile {2} already exist", eod.PagIBIGID, eod.PaymentRefNo, eod.ReferenceFile));
                            failedDBInsert += 1;
                        }

                        //if (config.BankID == (short)Program.bankID.AUB)
                        //{
                            bank_ws.ACC_MS_WEBSERVICE ws = new bank_ws.ACC_MS_WEBSERVICE();
                            var response = ws.ConfirmLoanDeduction_AUBwithDate(config.BankID.ToString(), config.BankWsPassword, eod.PaymentRefNo, eod.PagIBIGID, Convert.ToDateTime(eod.Processed_Date));
                            if (response.Split('|')[1] != "00")
                            {
                                logger.Error(string.Format("ConfirmLoanDeduction_AUBwithDate ws failed. MID {0} PaymentRefNo {1} ReferenceFile {2}. Error {3}", eod.PagIBIGID, eod.PaymentRefNo, eod.ReferenceFile, response.Split('|')[0]));
                                failedWS += 1;
                            }
                            else
                            {
                                logger.Info(string.Format("MID {0} PaymentRefNo {1} ReferenceFile {2}. ConfirmLoanDeduction_AUBwithDate response is success.", eod.PagIBIGID, eod.PaymentRefNo, eod.ReferenceFile, dal.ErrorMessage));
                                successWS += 1;
                            }
                        //}
                    }
                    else
                    {
                        logger.Error(string.Format("MID {0} PaymentRefNo {1} ReferenceFile {2}. DAL error {3}", eod.PagIBIGID, eod.PaymentRefNo, eod.ReferenceFile,dal.ErrorMessage));
                        failedDBInsert += 1;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }

            logger.Info(string.Format("Uploaded to db: Success {0} Failed {1} Total {2}, Webservice responses: Success {3} Failed {4} Total {5}", successDBInsert.ToString("N0"), failedDBInsert.ToString("N0"), (successDBInsert + failedDBInsert).ToString("N0"), successWS.ToString("N0"), failedWS.ToString("N0"), (successWS + failedWS).ToString("N0")));
            dal.Dispose();
            dal = null;
            return true;
        }

        public System.Data.DataTable SourceData(string FilePath)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            var fileName = FilePath;
            var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text\"";
            try
            {
                using (var conn = new System.Data.OleDb.OleDbConnection(connectionString))
                {
                    conn.Open();

                    var sheets = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM [" + sheets.Rows[0]["TABLE_NAME"].ToString() + "] ";

                        var adapter = new System.Data.OleDb.OleDbDataAdapter(cmd);
                        //var ds = new System.Data.DataSet();
                        adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                dt = null;
            }

            return dt;
        }

    }
}
