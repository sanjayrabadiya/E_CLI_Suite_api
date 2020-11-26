using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraHltPrefComp : BaseEntity, ICommonAduit
    {
        public int MedraConfigId { get; set; }
        public long hlt_code { get; set; }
        public long pt_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
