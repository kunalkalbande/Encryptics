
namespace Encryptics.WebPortal.Areas.Company.Models
{
    public class EmailTemplateModel
    {
        public EmailTemplateType
            TemplateType { get; set; }

        public string Header { get; set; }

        public string Body { get; set; }

        public enum EmailTemplateType
        {
            PlainText = 0,
            Html = 1
        }
    }
}