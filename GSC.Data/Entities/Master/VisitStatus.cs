using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Entities.Master
{
    public class VisitStatus : BaseEntity
    {
        public Int16 Code { get; set; }
        public string StatusName { get; set; }
        public string DisplayName { get; set; }

        public int? CompanyId { get; set; }
    }
}
