using System;
using System.Collections.Generic;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateValueDto : BaseDto
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public string DocPath { get; set; }
        public string DocFullPath { get; set; }
        public string Text { get; set; }
        public FileModel FileModel { get; set; }
        public string OldValue { get; set; }
        public string TimeZone { get; set; }
        public QueryStatus? QueryStatus { get; set; }
        public short ReviewLevel { get; set; }
        public short? AcknowledgeLevel { get; set; }
        public ICollection<ScreeningTemplateValueChildDto> Children { get; set; }
        public string ValueName { get; set; }
        public bool IsNa { get; set; }
        public int? UserRoleId { get; set; }
        public bool IsSystem { get; set; }
        public List<EditCheckIds> EditCheckIds { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public ScreeningTemplateStatus ScreeningStatus { get; set; }
        public int ScreeningEntryId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
    }

    public class ScreeningTemplateValueSaveBasics
    {
        public int Id { get; set; }
        public List<EditCheckTargetValidationList> EditCheckResult { get; set; }
        public List<ScreeningTemplateValueChildBasic> Children { get; set; }
    }


    public class TemplateTotalQueryDto
    {
        public int ScreeningTemplateId { get; set; }
        public int Total { get; set; }
    }


    public class VariableQueryDto
    {
        public string QueryStatus { get; set; }
        public string VariableName { get; set; }
        public int Total { get; set; }
        public short Level { get; set; }
        public string LevelName { get; set; }
    }


    //public QueryStatus? QueryStatus { get; set; }

    public class ScreeningTemplateValueBasic
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int Id { get; set; }
        public string Value { get; set; }
        public bool IsNa { get; set; }
        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate?.UtcDateTime();
            set => _scheduleDate = value?.UtcDateTime();
        }
        public bool IsSystem { get; set; }
        public QueryStatus? QueryStatus { get; set; }
        public string DocPath { get; set; }
        public bool IsComment { get; set; }
        public short ReviewLevel { get; set; }
        public int? UserRoleId { get; set; }
        public short? AcknowledgeLevel { get; set; }
        public ICollection<ScreeningTemplateValueChild> Children { get; set; }

    }
}