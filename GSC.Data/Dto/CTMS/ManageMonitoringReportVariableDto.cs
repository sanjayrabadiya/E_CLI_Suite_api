using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.CTMS
{
    public class ManageMonitoringReportVariableDto : BaseDto
    {
        public int ManageMonitoringReportId { get; set; }
        public string Value { get; set; }
        public int VariableId { get; set; }
        public bool IsNa { get; set; }
        public string ValueName { get; set; }
        public string OldValue { get; set; }
        public int ManageMonitoringReportVariableId { get; set; }
        public bool IsComment { get; set; }
        public ManageMonitoringReport ManageMonitoringReport { get; set; }
        public ICollection<ManageMonitoringReportVariableChildDto> Children { get; set; }
    }

    public class ManageMonitoringReportVariableGridDto : BaseAuditDto
    {
        public string Status { get; set; }
        public string VariableTemplate { get; set; }
    }

    public class ManageMonitoringValueBasic
    {
        public int VariableTemplateId { get; set; }
        public int VariableId { get; set; }
        public int Id { get; set; }
        public string Value { get; set; }
        public bool IsNa { get; set; }
        public int? UserRoleId { get; set; }
        public bool IsComment { get; set; }
        public ICollection<ManageMonitoringReportVariableChild> Children { get; set; }
    }

    public class ManageMonitoringReportVariableValueDto
    {
        public int Id { get; set; }
        public int VariableId { get; set; }
        public string ValueName { get; set; }
        public string VariableValue { get; set; }
        public int ManageMonitoringReportVariableChildId { get; set; }
        public string VariableValueOld { get; set; }
        public string Label { get; set; }
        public int SeqNo { get; set; }
    }
}