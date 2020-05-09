using System.Collections.Generic;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Helper.DocumentService;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateValueDto : BaseDto
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public string DocPath { get; set; }
        public string Text { get; set; }
        public FileModel FileModel { get; set; }
        public string OldValue { get; set; }
        public string TimeZone { get; set; }
        public QueryStatus? QueryStatus { get; set; }
        public short ReviewLevel { get; set; }
        public short? AcknowledgeLevel { get; set; }
        public ProjectDesignVariableDto ProjectDesignVariable { get; set; }
        public ICollection<ScreeningTemplateValueAuditDto> Audits { get; set; }
        public ICollection<ScreeningTemplateValueCommentDto> Comments { get; set; }
        public ICollection<ScreeningTemplateValueChildDto> Children { get; set; }
        public string ValueName { get; set; }
        public bool IsNa { get; set; }
        public int? EditCheckDetailId { get; set; }
        public int? UserRoleId { get; set; }
        public bool IsSystem { get; set; }
        public ScreeningStatus ScreeningStatus { get; set; }
    }

    public class QueryStatusDto
    {
        public int TotalQuery { get; set; }
        public List<QueryStatusCount> Items { get; set; }
    }

    public class QueryStatusCount
    {
        public int Total { get; set; }
        public string QueryStatus { get; set; }
    }

    public class PeriodQueryStatusDto
    {
        public int ScreeningEntryId { get; set; }
        public short? AcknowledgeLevel { get; set; }
        public QueryStatus? QueryStatus { get; set; }
    }
}