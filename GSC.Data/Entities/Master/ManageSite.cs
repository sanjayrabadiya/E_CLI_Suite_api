﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Master
{
    public class ManageSite : BaseEntity, ICommonAduit
    {
        public string SiteName { get; set; }
        public string ContactName { get; set; }
        public string SiteEmail { get; set; }
        public string ContactNumber { get; set; }
        public string SiteAddress { get; set; }
        public bool Status { get; set; }
        public int? CityId { get; set; }
        public int? CompanyId { get; set; }
        public City City { get; set; }
        public List<ManageSiteRole> ManageSiteRole { get; set; } = null;
        public List<Iecirb> Iecirb { get; set; } = null;
        public List<ManageSiteAddress> ManageSiteAddress { get; set;} = null;
    }
}
