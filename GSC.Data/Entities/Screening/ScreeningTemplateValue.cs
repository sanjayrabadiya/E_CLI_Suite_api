﻿using System;
using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

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
        public ICollection<ScreeningTemplateValueAudit> Audits { get; set; }
        public ICollection<ScreeningTemplateValueComment> Comments { get; set; }
        public List<ScreeningTemplateValueChild> Children { get; set; }
        public ICollection<ScreeningTemplateValueQuery> Queries { get; set; }
        public bool IsNa { get; set; }
        public int? UserRoleId { get; set; }
        public bool IsSystem { get; set; }
    }


    public class ScreeningTemplateValueBasic 
    {
        public int ScreeningTemplateId { get; set; }
        public QueryStatus? QueryStatus { get; set; }
    }
}