﻿using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using GSC.Helper;
using GSC.Shared.DocumentService;

namespace GSC.Data.Dto.CTMS
{
    public class StudyPlantaskParameterDto : BaseDto
    {
        public int StudyPlanId { get; set; }
        public int? TaskId { get; set; }
        [Required(ErrorMessage = "Task Name is required.")]
        public string TaskName { get; set; }
        public int? ParentId { get; set; }
        //[Required(ErrorMessage = "Start Date is required.")]
        public DateTime? StartDate { get; set; }
        //[Required(ErrorMessage = "End Date is required.")]
        public DateTime? EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public int TaskOrder { get; set; }
        [Required(ErrorMessage = "Duration is required.")]
        public Position Position { get; set; }
        public bool IsMileStone { get; set; }
        public int Duration { get; set; }
        public List<DependentTaskParameterDto> DependentTask { get; set; }
        public int? DependentTaskId { get; set; }
        public ActivityType? ActivityType { get; set; }
        public int OffSet { get; set; }
        public int? SiteId { get; set; }
        public RefrenceType RefrenceType { get; set; }
        public bool? PreApprovalStatus { get; set; } = false;
        public bool? ApprovalStatus { get; set; } = false;
        public FileModel FileModel { get; set; }
        public string FileName { get; set; }
        public string DocumentPath { get; set; }
        public decimal? Percentage { get; set; }

    }

    public class DependentTaskParameterDto : BaseDto
    {
        public int StudyPlanTaskId { get; set; }
        public int DependentTaskId { get; set; }
        public ActivityType ActivityType { get; set; }
        public int OffSet { get; set; }

    }

    public class NextWorkingDateParameterDto
    {
        public int StudyPlanId { get; set; }
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }

    }

    public class PreApprovalStatusDto
    {
        public int Id { get; set; }
        public bool? PreApprovalStatus { get; set; }
        public int? DependentTaskId { get; set; }
    }
    public class ApprovalStatusDto
    {
        public int Id { get; set; }
        public bool? ApprovalStatus { get; set; } = false;
        public FileModel FileModel { get; set; }
        public string FileName { get; set; }
        public string DocumentPath { get; set; }
    }
}
