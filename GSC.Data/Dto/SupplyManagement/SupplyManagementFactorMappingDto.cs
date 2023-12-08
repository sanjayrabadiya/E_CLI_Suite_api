using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.SupplyManagement
{
    public class SupplyManagementFactorMappingDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public Fector Factor { get; set; }
        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

    }
    public class SupplyManagementFactorMappingGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }

        public string ProjectCode { get; set; }

        public string FactorName { get; set; }

        public string VisitName { get; set; }

        public string TemplateName { get; set; }

        public string VariableName { get; set; }

        public int? AuditReasonId { get; set; }
        public string ReasonOth { get; set; }

        public string Reason { get; set; }

        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }

}
