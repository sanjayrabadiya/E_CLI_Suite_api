using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Configuration
{
    public class PharmacyConfigDto : BaseDto
    {
        [Required(ErrorMessage = "Form is required.")]
        public int FormId { get; set; }

        public string FormName { get; set; }

        [Required(ErrorMessage = "Template is required.")]
        public int VariableTemplateId { get; set; }
        // public int? CompanyId { get; set; }

        public string TemplateName { get; set; }
    }
}