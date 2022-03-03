using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.CTMS
{
    public class CtmsMonitoringReportVariableValueDto : BaseDto
    {
        public int CtmsMonitoringReportId { get; set; }
        public int StudyLevelFormVariableId { get; set; }
        public string Value { get; set; }
        public string ValueName { get; set; }
        public string OldValue { get; set; }
        public bool IsNa { get; set; }
        public CtmsCommentStatus? QueryStatus { get; set; }
        public CtmsMonitoringReport CtmsMonitoringReport { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
        public ICollection<CtmsMonitoringReportVariableValueChildDto> Children { get; set; }
    }

    public class CtmsMonitoringReportVariableValueBasic : BaseAuditDto
    {
        public int CtmsMonitoringReportId { get; set; }
        public int StudyLevelFormVariableId { get; set; }
        public string Value { get; set; }
        public string ValueName { get; set; }
        public string OldValue { get; set; }
        public int? UserRoleId { get; set; }
        public bool IsComment { get; set; }
        public CtmsCommentStatus QueryStatus { get; set; }
        public bool IsNa { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
        public ICollection<CtmsMonitoringReportVariableValueChild> Children { get; set; }
    }

    public class CtmsMonitoringReportVariableValueSaveDto : BaseDto
    {
        //public int VariableTemplateId { get; set; }
        public List<CtmsMonitoringReportVariableValueDto> CtmsMonitoringReportVariableValueList { get; set; }
    }
}