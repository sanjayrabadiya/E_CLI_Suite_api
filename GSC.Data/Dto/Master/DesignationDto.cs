using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class DesignationDto : BaseDto
    {
        [Required(ErrorMessage = "Designation Cod is required.")]
        public string DesignationCod { get; set; }

        [Required(ErrorMessage = "Name OF Designation is required.")]
        public string NameOFDesignation { get; set; }

        [Required(ErrorMessage = "Yers Of Experience is required.")]
        public int YersOfExperience { get; set; }
        public string Department { get; set; }
    }
    public class DesignationGridDto : BaseAuditDto
    {
        public string DesignationCod { get; set; }
        public string NameOFDesignation { get; set; }
        public int YersOfExperience { get; set; }
        public string Department { get; set; }
    }
}
