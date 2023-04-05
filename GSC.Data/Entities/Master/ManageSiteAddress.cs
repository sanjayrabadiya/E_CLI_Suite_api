using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class ManageSiteAddress : BaseEntity, ICommonAduit
    {
        public string SiteAddress { get; set; }
        public int ManageSiteId { get; set; }
        public ManageSite ManageSite { get; set; }
    }
}
