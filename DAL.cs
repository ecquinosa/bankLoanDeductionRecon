﻿
using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace bankLoanDeductionRecon
{
    class DAL : IDisposable

    {

        private DataTable dtResult;
        //private DataSet dsResult;
        private object objResult;
        private IDataReader _readerResult;
        private string strErrorMessage;

        private SqlConnection con;
        private SqlCommand cmd;
        private SqlDataAdapter da;

        private string ConStr { get; set; }

        public string ErrorMessage { get { return strErrorMessage; } }

        public DataTable TableResult { get { return dtResult; } }       

        public object ObjectResult { get { return objResult; } }

        public DAL(Config config)
        {
            ConStr = config.DbaseConStr;
        }

        public void ClearAllPools()
        {
            SqlConnection.ClearAllPools();
        }

        private void OpenConnection()
        {
            if (con == null) con = new SqlConnection(ConStr);
        }

        private void CloseConnection()
        {
            if (cmd != null) cmd.Dispose();
            if (da != null) da.Dispose();
            if (_readerResult != null)
            {
                _readerResult.Close();
                _readerResult.Dispose();
            }
            if (con != null)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
            ClearAllPools();
        }

        private void ExecuteNonQuery(CommandType cmdType)
        {
            cmd.CommandType = cmdType;

            // If con.State = ConnectionState.Open Then con.Close()
            // con.Open()
            if (con.State == ConnectionState.Closed)
                con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

        private void _ExecuteScalar(CommandType cmdType)
        {
            cmd.CommandType = cmdType;

            // If con.State = ConnectionState.Open Then con.Close()
            // con.Open()
            if (con.State == ConnectionState.Closed) con.Open();
            object _obj;
            _obj = cmd.ExecuteScalar();
            con.Close();

            objResult = _obj;
        }

        private void _ExecuteReader(CommandType cmdType)
        {
            cmd.CommandType = cmdType;

            // If con.State = ConnectionState.Open Then con.Close()
            // con.Open()
            if (con.State == ConnectionState.Closed)
                con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            _readerResult = reader;
        }

        private void FillDataAdapter(CommandType cmdType)
        {
            cmd.CommandTimeout = 0;
            cmd.CommandType = cmdType;
            da = new SqlDataAdapter(cmd);
            DataTable _dt = new DataTable();
            da.Fill(_dt);
            dtResult = _dt;
        }

        public bool SelectQuery(string strQuery)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand(strQuery, con);

                FillDataAdapter(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }      

        public bool IsConnectionOK(string strConString = "")
        {
            try
            {
                if (strConString != "")
                    ConStr = strConString;
                OpenConnection();

                con.Open();
                con.Close();

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }        

        public bool ExecuteQuery(string strQuery)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand(strQuery, con);

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool ExecuteScalar(string strQuery)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand(strQuery, con);

                _ExecuteScalar(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool Check_LoanDeductionIfExist(EOD eod)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand("SELECT COUNT(*) FROM tbl_LoanDeductionRecon WHERE PagIBIGID=@PagIBIGID AND PaymentRefNo=@PaymentRefNo AND ReferenceFile=@ReferenceFile", con);
                cmd.Parameters.AddWithValue("PagIBIGID", eod.PagIBIGID);
                cmd.Parameters.AddWithValue("PaymentRefNo", eod.PaymentRefNo);
                cmd.Parameters.AddWithValue("ReferenceFile", eod.ReferenceFile);

                _ExecuteScalar(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        public bool Add_LoanDeductionRecon(EOD eod, short bankID)
        {
            try
            {
                OpenConnection();
                cmd = new SqlCommand(string.Format("INSERT INTO tbl_LoanDeductionRecon (BankID, PagIBIGID, ActualTxn_Date, Processed_Date, Processed_Time, AcctNo, BranchCode, Transaction_Amount, PaymentRefNo, Remarks, ReferenceFile, DatePosted) VALUES (@BankID, @PagIBIGID, @ActualTxn_Date, @Processed_Date, @Processed_Time, @AcctNo, @BranchCode, @Transaction_Amount, @PaymentRefNo, @Remarks, @ReferenceFile, GETDATE())"), con);
                cmd.Parameters.AddWithValue("BankID", bankID);
                cmd.Parameters.AddWithValue("PagIBIGID", eod.PagIBIGID);
                cmd.Parameters.AddWithValue("ActualTxn_Date", eod.ActualTxn_Date);
                cmd.Parameters.AddWithValue("Processed_Date", eod.Processed_Date);
                cmd.Parameters.AddWithValue("Processed_Time", eod.Processed_Time);
                cmd.Parameters.AddWithValue("AcctNo", eod.AcctNo);
                cmd.Parameters.AddWithValue("BranchCode", eod.BranchCode);
                cmd.Parameters.AddWithValue("Transaction_Amount", eod.Transaction_Amount);
                cmd.Parameters.AddWithValue("PaymentRefNo", eod.PaymentRefNo);
                cmd.Parameters.AddWithValue("Remarks", eod.Remarks);
                cmd.Parameters.AddWithValue("ReferenceFile", eod.ReferenceFile);

                ExecuteNonQuery(CommandType.Text);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = ex.Message;
                return false;
            }
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    CloseConnection();
                }



                // Note disposing has been done.
                disposed = true;

            }
        }

    }
}
