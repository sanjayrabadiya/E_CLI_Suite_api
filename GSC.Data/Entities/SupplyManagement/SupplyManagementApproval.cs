﻿
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.SupplyManagement
{
    public class SupplyManagementApproval : BaseEntity
    {
        public int ProjectId { get; set; }
        public string EmailTemplate { get; set; }
        public SupplyManagementApprovalType ApprovalType { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public Entities.Master.Project Project { get; set; }
        public AuditReason AuditReason { get; set; }

        public int RoleId { get; set; }
    }
}