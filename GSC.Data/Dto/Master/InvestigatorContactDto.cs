using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;

namespace GSC.Data.Dto.Master
{
    public class InvestigatorContactDto : BaseDto
    {
        [Required(ErrorMessage = "Investigator Name is required.")]
        public string NameOfInvestigator { get; set; }
        [Required(ErrorMessage = "Investigator Email is required.")]
        public string EmailOfInvestigator { get; set; }
        [Required(ErrorMessage = "Therapeutic Indication is required.")]
        public int? TrialTypeId { get; set; }

        [Required(ErrorMessage = "RegistrationNumber is required.")]
        public string RegistrationNumber { get; set; }
        public string SiteName { get; set; }

        [Required(ErrorMessage = "ContactNumber is required.")]
        public string ContactNumber { get; set; }
        public int? CompanyId { get; set; }
    }

    public class InvestigatorContactGridDto : BaseAuditDto
    {
        public string NameOfInvestigator { get; set; }
        public string EmailOfInvestigator { get; set; }
        public string TherapeuticIndication { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string RegistrationNumber { get; set; }

    }
}