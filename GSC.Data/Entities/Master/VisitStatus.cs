﻿using GSC.Common.Base;
using GSC.Common.Common;
using System;

namespace GSC.Data.Entities.Master
{
    public class VisitStatus : BaseEntity, ICommonAduit
    {
        public Int16 Code { get; set; }
        public string StatusName { get; set; }
        public string DisplayName { get; set; }
        public bool IsAuto { get; set; }
        public int? CompanyId { get; set; }
    }
}
