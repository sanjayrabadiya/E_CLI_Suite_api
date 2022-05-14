using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class SendEmailOnVariableChangeSetting : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVariableId { get; set; }
        public string Email { get; set; }
        public string EmailTemplate { get; set; }
        public string CollectionValue { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
    }
}
