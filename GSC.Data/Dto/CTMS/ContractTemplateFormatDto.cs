using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class ContractTemplateFormatDto : BaseDto
    {
        public string TemplateCode { get; set; }
        [Required(ErrorMessage = "Template Name is required.")]
        public string TemplateName { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Template Format is required.")]
        public string TemplateFormat { get; set; }
    }
    public class ContractTemplateFormatGridDto : BaseAuditDto
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string TemplateFormat { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
