﻿using GSC.Common.Base;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraCodingAudit : BaseEntity
    {
        public int MeddraCodingId { get; set; }
        public int? MeddraLowLevelTermId { get; set; }
        public int? MeddraSocTermId { get; set; }
        public int? CompanyId { get; set; }
        public string Note { get; set; }
        public string Action { get; set; }
        public int? UserRoleId { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }

        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
    }
}
