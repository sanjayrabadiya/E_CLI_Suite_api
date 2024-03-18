using GSC.Common.Base;
using GSC.Common.Common;
using System;

namespace GSC.Data.Entities.Master
{
    public class PatientStatus : BaseEntity, ICommonAduit
    {

        public Int16 Code { get; set; }
        public string StatusName { get; set; }
        public int? CompanyId { get; set; }
    }
}
