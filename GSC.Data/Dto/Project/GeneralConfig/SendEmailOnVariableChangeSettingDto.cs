using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Data.Dto.Project.Generalconfig
{
    public class SendEmailOnVariableChangeSettingDto : BaseDto
    {
        public int ProjectDesignVariableId { get; set; }
        public string Email { get; set; }
        public string EmailTemplate { get; set; }
        public string CollectionValue { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }

    public class SendEmailOnVariableChangeSettingGridDto : BaseAuditDto
    {
        public string ProjectCode { get; set; }
        public string ProjectDesignVisit { get; set; }
        public string ProjectDesignTemplate { get; set; }
        public string ProjectDesignVariable { get; set; }
        public string CollectionValue { get; set; }
        public string Email { get; set; }
        public string EmailTemplate { get; set; }
    }

}
