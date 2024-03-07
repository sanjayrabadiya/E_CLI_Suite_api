using GSC.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
    public class TaskTemplateDto : BaseDto
    {
        [Required(ErrorMessage = "Template Code is required.")]
        public string TemplateCode { get; set; }
        [Required(ErrorMessage = "Template Name is required.")]
        public string TemplateName { get; set; }
    }
    public class TaskTemplateGridDto : BaseAuditDto
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }

    }
}
