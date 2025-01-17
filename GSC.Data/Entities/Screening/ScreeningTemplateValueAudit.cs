﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueAudit
    {
        [Key]
        public int Id { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public int? ProjectDesignVariableValueId { get; set; }
        public string Value { get; set; }
        public string Note { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public string OldValue { get; set; }
        public string IpAddress { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string TimeZone { get; set; }
        private DateTime? _createdDate;
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }


        public ScreeningTemplateValue ScreeningTemplateValue { get; set; }
        public ProjectDesignVariableValue ProjectDesignVariableValue { get; set; }
        [ForeignKey("ReasonId")] public AuditReason AuditReason { get; set; }

        public string Uuid { get; set; }
        public string System { get; set; }
        public string Platform { get; set; }
        public string DeviceBrowser { get; set; }
    }
}