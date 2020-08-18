using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Entities.Master
{
    public class PatientStatus : BaseEntity
    {

        public Int16 Code { get; set; }
        public string StatusName { get; set; }
        public int? CompanyId { get; set; }
    }
}
