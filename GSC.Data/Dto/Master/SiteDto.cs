using GSC.Common.Base;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class SiteDto : BaseEntity
    {
        public int ManageSiteId { get; set; }
        public int[] ManageSiteIds { get; set; }
        public int InvestigatorContactId { get; set; }
        public int? CompanyId { get; set; }
    }

    public class SiteGridDto : BaseAuditDto
    {
        public string SiteName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string IECIRBName { get; set; }
        public string ContactNumber { get; set; }
        public string SiteEmail { get; set; }
        public string ContactName { get; set; }
        public int ManageSiteId { get; set; }
    }
}
