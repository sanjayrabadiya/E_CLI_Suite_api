using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class ProjectSiteAddressDto : BaseDto
    {
        public int ManageSiteId { get; set; }
        public int ManageSiteAddressId { get; set; }
        public int ProjectId { get; set; }

        public ManageSiteDto ManageSite { get; set; }
        public ManageSiteAddressDto ManageSiteAddress { get; set; }
        public ProjectDto Project { get; set; }
    }

    public class ProjectSiteAddressGridDto : BaseAuditDto
    {
        public string SiteAddress { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public string Facilities { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
    }
}
