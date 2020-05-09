using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Configuration
{
    public class EmailSettingDto : BaseDto
    {
        [Required(ErrorMessage = "From Email is required.")]
        public string EmailFrom { get; set; }

        [Required(ErrorMessage = "Email Port is required.")]
        public string PortName { get; set; }

        public string DomainName { get; set; }

        [Required(ErrorMessage = "Email Password is required.")]
        public string EmailPassword { get; set; }

        public bool MailSsl { get; set; }

        public int CompanyId { get; set; }
    }
}