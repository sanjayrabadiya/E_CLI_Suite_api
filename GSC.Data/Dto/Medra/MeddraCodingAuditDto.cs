﻿using GSC.Data.Entities.Common;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraCodingAuditDto : BaseDto
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

        public DateTime? CreatedDate { get; set; }

        public string CreateUser { get; set; }
        public string Value { get; set; }
        public string PT { get; set; }
        public string HLT { get; set; }
        public string HLGT { get; set; }
        public string SocCode { get; set; }
        public string SOCValue { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }
        public long? LLT { get; set; }
        public string Code { get; set; }
        public string PrimarySoc { get; set; }

    }
}
