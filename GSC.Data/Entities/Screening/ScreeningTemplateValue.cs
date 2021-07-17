using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValue : BaseEntity
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public string DocPath { get; set; }
        public string MimeType { get; set; }
        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate?.UtcDateTime();
            set => _scheduleDate = value?.UtcDateTime();
        }
        public QueryStatus? QueryStatus { get; set; }
        public short ReviewLevel { get; set; }
        public short? AcknowledgeLevel { get; set; }
        public ScreeningTemplate ScreeningTemplate { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public ICollection<ScreeningTemplateValueComment> Comments { get; set; }
        public List<ScreeningTemplateValueChild> Children { get; set; }
        public List<ScreeningTemplateValueAudit> ScreeningTemplateValueAudits { get; set; }
        public List<ScreeningTemplateValueQuery> ScreeningTemplateValueQuerys { get; set; }
        public bool IsNa { get; set; }
        public int? UserRoleId { get; set; }

        [ForeignKey("UserRoleId")] 
        public SecurityRole SecurityRole { get; set; }
        public bool IsSystem { get; set; }
    }


    public class ScreeningTemplateValueBasic 
    {
        public int ScreeningTemplateId { get; set; }
        public QueryStatus? QueryStatus { get; set; }
    }
}