using GSC.Data.Entities.Common;
using System.Collections.Generic;
using GSC.Data.Entities.Location;

namespace GSC.Data.Dto.Master
{
    public class ManageSiteAddressDto : BaseDto
    {
        public string SiteAddress { get; set; }
        public int ManageSiteId { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public int? CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public int? CompanyId { get; set; }
        public List<string> Facilities { get; set; }
        public ManageSiteDto ManageSite { get; set; }
        public City City { get; set; }
    }

    public class ManageSiteAddressGridDto : BaseAuditDto
    {
        public string SiteAddress { get; set; }
        public int ManageSiteId { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public int? CityId { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Facilities { get; set; }
    }
}
