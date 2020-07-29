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

        [Required(ErrorMessage = "Specialization is required.")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "RegistrationNumber is required.")]
        public string RegistrationNumber { get; set; }

        [Required(ErrorMessage = "Site Name is required.")]
        public int ManageSiteId { get; set; }

        [Required(ErrorMessage = "Site Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "ContactNumber is required.")]
        public string ContactNumber { get; set; }
        public int IecirbId { get; set; }
        public string IECIRBContactNo { get; set; }
        public string IECIRBContactName { get; set; }
        public string IECIRBContactEmail { get; set; }
        [Required(ErrorMessage = "City is required.")]
        public int CityId { get; set; }

        public string CityName { get; set; }
       // public int? CompanyId { get; set; }
        public City City { get; set; }

        public int StateId { get; set; }

        public int CountryId { get; set; }

        public string StateName { get; set; }

        public string CountryName { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public int? CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}