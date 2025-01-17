﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplate : BaseEntity
    {
        public int ProjectDesignTemplateId { get; set; }

        public string ScreeningTemplateName { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public int? ParentId { get; set; }
        public int? Progress { get; set; }
        public short? ReviewLevel { get; set; }
        public short? LastReviewLevel { get; set; }
        public short? StartLevel { get; set; }
        public int ScreeningVisitId { get; set; }
        public ScreeningVisit ScreeningVisit { get; set; }
        public ICollection<ScreeningTemplateValue> ScreeningTemplateValues { get; set; }

        [ForeignKey("ParentId")]
        public ICollection<ScreeningTemplate> Children { get; set; }

        public List<ScreeningTemplateReview> ScreeningTemplateReview { get; set; }
        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate?.UtcDateTime();
            set => _scheduleDate = value?.UtcDateTime();
        }

        private DateTime? _actualeDate { get; set; }
        public DateTime? ActualDate
        {
            get => _actualeDate?.UtcDateTime();
            set => _actualeDate = value?.UtcDateTime();
        }
        public bool IsLocked { get; set; }
        public bool IsHardLocked { get; set; }
        public bool IsCompleteReview { get; set; }
        public bool IsDisable { get; set; }
        public int? RepeatSeqNo { get; set; }
        public bool? IsHide { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public bool IsNA { get; set; }

    }
}