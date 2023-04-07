using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class ManageSiteAddress : BaseEntity, ICommonAduit
    {
        public string SiteAddress { get; set; }
        public int ManageSiteId { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public int? CityId { get; set; }
        public City City { get; set; }
        public string Facilities { get; set; }
        public ManageSite ManageSite { get; set; }
    }
}
