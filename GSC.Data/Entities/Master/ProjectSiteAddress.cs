using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class ProjectSiteAddress : BaseEntity, ICommonAduit
    {
        public int ManageSiteId { get; set; }
        public int ManageSiteAddressId { get; set; }
        public int ProjectId { get; set; }

        public ManageSite ManageSite { get; set; }
        public ManageSiteAddress ManageSiteAddress { get; set; }
        public Project Project { get; set; }
    }
}
