﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class EmailConfigurationEditCheck : BaseEntity
    {
        public int ProjectId { get; set; }
        public string CheckFormula { get; set; }
        public string SourceFormula { get; set; }
        public string SampleResult { get; set; }
        public string ErrorMessage { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
        public int? EditCheckRoleAuditReasonId { get; set; }
        public string EditCheckRoleReasonOth { get; set; }
        public GSC.Data.Entities.Master.Project Project { get; set; }

        public List<EmailConfigurationEditCheckDetail> EmailConfigurationEditCheckDetailList { get; set; }
    }
}