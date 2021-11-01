using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.CTMS
{
    public class ManageMonitoringReportDto : BaseDto
    {
        public int ManageMonitoringVisitId { get; set; }
        public short Status { get; set; }
        public int VariableTemplateId { get; set; }
    }

    public class ManageMonitoringReportGridDto : BaseAuditDto
    {
        public string Status { get; set; }
        public MonitoringReportStatus StatusId { get; set; }
        public string VariableTemplate { get; set; }
        public int VariableTemplateId { get; set; }
        public string ActivityName { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int ManageMonitoringVisitId { get; set; }
        public int ProjectId { get; set; }
    }

    public class ManageMonitoringTemplateBasic
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int ManageMonitoringVisitId { get; set; }
        public int VariableTemplateId { get; set; }
    }

    public class ManageMonitoringReportValueSaveDto : BaseDto
    {
        public int VariableTemplateId { get; set; }

        public List<ManageMonitoringReportVariableDto> MonitoringVariableValueList { get; set; }
    }
}
