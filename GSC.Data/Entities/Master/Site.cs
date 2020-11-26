using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Master
{
    public class Site : BaseEntity, ICommonAduit
    {
        public int ManageSiteId { get; set; }
        public int InvestigatorContactId { get; set; }
        public int? CompanyId { get; set; }
        public ManageSite ManageSite { get; set; }
    }
}
