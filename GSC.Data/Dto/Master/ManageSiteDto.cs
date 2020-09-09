using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
   public class ManageSiteDto : BaseDto
    {
        [Required(ErrorMessage = "Site Name is required.")]
        public string SiteName { get; set; }
        [Required(ErrorMessage = "Contact Name is required.")]
        public string ContactName { get; set; }
        [Required(ErrorMessage = "Site Email is required.")]
        public string SiteEmail { get; set; }
        [Required(ErrorMessage = "Contact Number is required.")]
        public string ContactNumber { get; set; }
        [Required(ErrorMessage = "Site Address is required.")]
        public string SiteAddress { get; set; }
        public bool Status { get; set; }
        [Required(ErrorMessage = "City is required.")]
        public int CityId { get; set; }

        public string CityName { get; set; }
        public City City { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}
