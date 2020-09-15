using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;

namespace GSC.Data.Dto.Master
{
    public class InvestigatorContactDetailDto : BaseDto
    {
        public int? InvestigatorContactId { get; set; }
        public int? ContactTypeId { get; set; }
        public int? SecurityRoleId { get; set; }
        [Required(ErrorMessage = "Contact Email is required.")]
        public string ContactEmail { get; set; }
        [Required(ErrorMessage = "Contact Name is required.")]
        public string ContactName { get; set; }
        [Required(ErrorMessage = "Contact No is required.")]
        public string ContactNo { get; set; }
        public int? CompanyId { get; set; }
    }

    public class InvestigatorContactDetailGridDto : BaseAuditDto
    {
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactNo { get; set; }
        public string ContactType { get; set; }
        public string SecurityRole { get; set; }
    }
}
