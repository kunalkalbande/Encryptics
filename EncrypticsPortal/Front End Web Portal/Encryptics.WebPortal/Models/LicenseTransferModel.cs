using System;

namespace Encryptics.WebPortal.Models
{
    public class LicenseTransferModel
    {
        public long FromEntity { get; set; }
        public long ToEntity { get; set; }
        public int TransferAmount { get; set; }
        public LicensePoolType FromPool { get; set; }
        public LicensePoolType ToPool { get; set; }
        public DateTime? UseByDate { get; set; }
    }

    public enum LicensePoolType
    {
        ActivePool,
        AvailablePool
    }
}