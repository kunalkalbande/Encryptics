using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class ZDPSettingsModel
    {
        [Display(Name = "Enable Zero Day Protection")]
        public bool IsEnabled { get; set; }
        public IEnumerable<ZDPFileTypeModel> FileTypes { get; set; }
    }

    public class ZDPFileTypeModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<ZDPSettingModel> ConfigurationSettings { get; set; }
    }

    public class ZDPSettingModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Setting { get; set; }
        public bool IsWaterMark { get; set; }
    }

    public enum ZDPSettingLevel
    {
        Allow,
        Disallow,
        Sanitise
    }
}