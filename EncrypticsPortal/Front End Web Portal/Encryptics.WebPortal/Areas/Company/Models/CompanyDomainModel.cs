using System;
using System.ComponentModel.DataAnnotations;

namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class CompanyDomainModel
    {
        public long Id { get; set; }

        [Display(Name = "Domain")]
        [MaxLength(100)]
        public string Domain { get; set; }

        [Display(Name = "Created")]
        // [DisplayFormat(DataFormatString = "{0:dd MM yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime CreatedDate { get; set; }

    }
}