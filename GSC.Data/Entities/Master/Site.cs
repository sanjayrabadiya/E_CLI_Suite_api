using GSC.Data.Entities.Common;
using GSC.Data.Entities.Location;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Master
{
    public class Site : BaseEntity
    {
        public int ManageSiteId { get; set; }
        public int InvestigatorContactId { get; set; }
        public int? CompanyId { get; set; }
        //public City City { get; set; }
        public ManageSite ManageSite { get; set; }
        public IList<Iecirb> IECIRB { get; set; } = null;
        //public Iecirb IECIRB { get; set; }
    }
}
