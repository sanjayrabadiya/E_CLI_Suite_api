using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraHlgtHltComp : BaseEntity
    {
        public int MedraConfigId{get;set;}
        public long hlgt_code { get; set; }
        public long hlt_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
