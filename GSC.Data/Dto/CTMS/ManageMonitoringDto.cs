using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.CTMS
{
    public class ManageMonitoringDto : BaseDto
    {
        [Required(ErrorMessage = "Activity is required.")]
        public int ActivityId { get; set; }

        [Required(ErrorMessage = "Study is required.")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Form is required.")]
        public int VariableTemplateId { get; set; }

        public int? CompanyId { get; set; }
    }

    public class ManageMonitoringGridDto : BaseAuditDto
    {
        public string Activity { get; set; }
        public string Project { get; set; }
        public string VariableTemplate { get; set; }

    }
}