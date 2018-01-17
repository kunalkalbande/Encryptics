using System;
using System.ComponentModel.DataAnnotations;
using Encryptics.WebPortal.Models;
using MyResources = Encryptics.WebPortal.Properties.Resources;

namespace Encryptics.WebPortal.Areas.UserAccount.Models
{
    public class DeviceModel : IModelData
    {
        //[Display(Name = "Device ID")]
        public long Id { get; set; }

        //[Display(Name = "Device Name")]
        public string Name { get; set; }

        //[Display(Name = "Device Type")]
        public DeviceType Type { get; set; }

        //[Display(Name = "Deployment Date")]
        public DateTime DateDeployed { get; set; }

        //[Display(Name = "# of Encryptions")]
        public int Encrypts { get; set; }

        //[Display(Name = "# of Decryptions")]
        public int Decrypts { get; set; }

        //[Display(Name = "Active Session")]
        [BooleanDisplayValues(typeof(MyResources), trueValueName: "ActiveDisplay", falseValueName: "InactiveDisplay")]
        [UIHint("DeviceSessionStatus")]
        public bool HasActiveSession { get; set; }

        //[Display(Name = "TokenID")]
        public long? TokenId { get; set; }

        //[Display(Name = "Status")]
        public DeviceStatus Status { get; set; }
    }

    public enum DeviceStatus
    {
        //[Display(Name = "None")]
        None,
        //[Display(Name = "Active")]
        Active,
        //[Display(Name = "Suspended")]
        Suspended,
    }

    public enum DeviceType
    {
        [Display(Name = @"Laptop")]
        WinRAndR,
        [Display(Name = @"Laptop")]
        WinPro,
        [Display(Name = @"Mobile")]
        IosRAndR,
        [Display(Name = @"Mobile")]
        IosPro,
        [Display(Name = @"Mobile")]
        DroidRAndR,
        [Display(Name = @"Mobile")]
        DroidPro,
        [Display(Name = @"Laptop")]
        JavaRAndR,
        [Display(Name = @"Laptop")]
        JavaPro,
        [Display(Name = @"Mobile")]
        WinPhoneRAndR,
        [Display(Name = @"Mobile")]
        WinPhonePro,
        [Display(Name = @"Laptop")]
        MacRAndR,
        [Display(Name = @"Laptop")]
        MacPro,
    }
}