using GSC.Data.Entities.Common;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class IecirbDto : BaseDto
    {
        public int? ManageSiteId { get; set; }
        [Required(ErrorMessage = "IECIRB Name is required.")]
        public string IECIRBName { get; set; }
        [Required(ErrorMessage = "Registration Number is required.")]
        public string RegistrationNumber { get; set; }
        [Required(ErrorMessage = "IECIRB Contact Name is required.")]
        public string IECIRBContactName { get; set; }
        [Required(ErrorMessage = "IECIRB Contact Email is required.")]
        public string IECIRBContactEmail { get; set; }
        [Required(ErrorMessage = "IECIRB Contact Number is required.")]
        public string IECIRBContactNumber { get; set; }
        public int? CompanyId { get; set; }
    }

    public class IecirbGridDto : BaseAuditDto
    {
        public string IECIRBName { get; set; }
        public string RegistrationNumber { get; set; }
        public string IECIRBContactName { get; set; }
        public string IECIRBContactEmail { get; set; }
        public string IECIRBContactNumber { get; set; }
    }
}
