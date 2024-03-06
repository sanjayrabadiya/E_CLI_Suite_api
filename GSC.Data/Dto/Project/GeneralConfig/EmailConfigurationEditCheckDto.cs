using GSC.Data.Entities.Common;
using System.Collections.Generic;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class EmailConfigurationEditCheckDto : BaseDto
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

        public List<EmailConfigurationEditCheckDetailDto> Children { get; set; }

    }

    public class EmailConfigurationEditCheckGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string CheckFormula { get; set; }
        public string SourceFormula { get; set; }
        public string SampleResult { get; set; }
        public string ErrorMessage { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string ReasonName { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
        public string Roles { get; set; }
        public int? EditCheckRoleAuditReasonId { get; set; }
        public string EditCheckRoleReasonOth { get; set; }
        public string EditCheckRoleAuditReasonName { get; set; }
    }

    public class EmailConfigurationEditCheckMailHistoryGridDto : BaseAuditDto
    {
        public int EmailConfigurationEditCheckId { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
        
    }
}
