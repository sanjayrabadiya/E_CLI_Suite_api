﻿using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementUnblindTreatmentDto : BaseDto
    {
        public List<SupplyManagementUnblindTreatmentGridDto> list { get; set; }
        public TreatmentUnblindType TypeofUnblind { get; set; }
        public int? RoleId { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public string Treatment { get; set; }
        public DateTime? UnblindDatetime { get; set; }

        public int? RandomizationId { get; set; }

    }

    public class SupplyManagementUnblindTreatmentGridDto : BaseAuditDto
    {

        public int ParentProjectId { get; set; }
        public int ProjectId { get; set; }
        public string StudyCode { get; set; }
        public string SiteCode { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public string VisitName { get; set; }
        public string TreatmentType { get; set; }
        public string ScreeningNo { get; set; }
        public string RandomizationNo { get; set; }
        public int RandomizationId { get; set; }
        public string TypeofUnblindName { get; set; }
        public int SupplyManagementUnblindTreatmentId { get; set; }
        public string ActionBy { get; set; }
        public string ActionByRole { get; set; }
        public DateTime? ActionDate { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

    }

}
