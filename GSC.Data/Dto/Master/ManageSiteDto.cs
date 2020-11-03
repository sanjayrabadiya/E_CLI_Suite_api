using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System.Collections.Generic;

namespace GSC.Data.Dto.Master
{
   public class ManageSiteDto : BaseDto
    {
        public string SiteName { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public string SiteAddress { get; set; }
        public bool Status { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public int? CompanyId { get; set; }
        public List<ManageSiteRole> ManageSiteRole { get; set; }
    }

     public class ManageSiteGridDto : BaseAuditDto
    {
        public string SiteName { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public string SiteAddress { get; set; }
        public bool Status { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
    }
}
