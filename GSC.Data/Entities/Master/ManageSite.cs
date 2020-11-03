using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using System.Collections.Generic;

namespace GSC.Data.Entities.Master
{
    public class ManageSite : BaseEntity
    {
        public string SiteName { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public string SiteAddress { get; set; }
        public bool Status { get; set; }
        public int CityId { get; set; }
        public int? CompanyId { get; set; }
        public City City { get; set; }
        public List<ManageSiteRole> ManageSiteRole { get; set; } = null;
    }
}
