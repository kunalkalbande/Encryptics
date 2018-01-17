using System.ComponentModel.DataAnnotations;
using Encryptics.WebPortal.Models;
using System;

namespace Encryptics.WebPortal.Areas.UserAdmin.Models
{
    public class ImportFilesListModel
    {
        public PageableViewModel<ImportFileModel> Type { get; set; }
    }

    public class ImportFileModel
    {
        public int Id { get; set; }
        [Display(Name= "File Name")]
        public string FileName { get; set; }
        [Display(Name = "Date Uploaded")]
        public DateTime DateUploaded { get; set; }
        public long Accounts { get; set; }
    }
}