using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bankLoanDeductionRecon
{
    class Config
    {
        public short BankID { get; set; }
        public string DownloadFolder { get; set; }
        public string DbaseConStr { get; set; }
        public string SftpHost { get; set; }
        public string SftpPort { get; set; }
        public string SftpUser { get; set; }
        public string SftpPass { get; set; }
        public string SftpSshHostKeyFingerprint { get; set; }
        public string SftpRemotePath { get; set; }
        public string BankWsPassword { get; set; }
        public short IsDownloadFromSFTP { get; set; }

    }
}
