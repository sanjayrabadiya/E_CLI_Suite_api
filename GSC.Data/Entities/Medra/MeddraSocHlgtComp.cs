﻿using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraSocHlgtComp : BaseEntity, ICommonAduit
    {
        public int MedraConfigId { get; set; }
        public long soc_code { get; set; }
        public long hlgt_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
