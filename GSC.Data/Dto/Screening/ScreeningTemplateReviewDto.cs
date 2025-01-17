﻿using System;
using System.Collections.Generic;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateReviewDto
    {
        public string Status { get; set; }
        public short ReviewLevel { get; set; }
        private DateTime? _date { get; set; }
        public string OfficerName { get; set; }
        public string RoleName { get; set; }

        public DateTime? Date
        {
            get => _date.UtcDate();
            set => _date = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public bool IsRepeat { get; set; }
        public int ProjectDesignTemplateId { get; set; }
    }

    public class RollbackReviewTemplateDto
    {
        public List<int> ScreeningTemplateIds { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
    }
}