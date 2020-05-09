using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Configuration
{
    public class EmailTemplateDto : BaseDto
    {
        [Required(ErrorMessage = "Template key value is required.")]
        public string KeyName { get; set; }

        [Required(ErrorMessage = "EMail Setting is required.")]
        public int EMailSettingId { get; set; }

        [Required(ErrorMessage = "EMail Subject is required.")]
        public string SubjectName { get; set; }

        public string Bcc { get; set; }

        [Required(ErrorMessage = "EMail Body is required.")]
        public string Body { get; set; }

        [Required(ErrorMessage = "Company is required.")]
        public int CompanyId { get; set; }
    }
}