﻿using System;
using GSC.Data.Entities.Common;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateValueAuditDto : BaseDto
    {
        private DateTime? _createdDate;
        public int ScreeningTemplateValueId { get; set; }
        public int? ProjectDesignVariableValueId { get; set; }
        public string Value { get; set; }
        public string Note { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string CreatedByName { get; set; }

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }

        public string OldValue { get; set; }
    }
}