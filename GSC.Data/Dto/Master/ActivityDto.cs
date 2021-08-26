using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class ActivityDto : BaseDto
    {
        [Required(ErrorMessage = "Activity Code is required.")]
        public string ActivityCode { get; set; }

        [Required(ErrorMessage = "Activity Name is required.")]
        public string ActivityName { get; set; }

        [Required(ErrorMessage = "Module Name is required.")]
        public AuditModule ModuleId { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ActivityGridDto : BaseAuditDto
    {
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
        public string ModuleName { get; set; }

    }
}