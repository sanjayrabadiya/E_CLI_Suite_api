﻿using System;
using System.Collections.Generic;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Attendance
{
    public class DataCaptureGridDto
    {
        public DataCaptureGridDto()
        {
            Data = new List<DataCaptureGridData>();
        }
        public List<WorkFlowText> WorkFlowText { get; set; }
        public List<DataCaptureGridData> Data { get; set; }
        public short ReviewLevel { get; set; }
        public bool IsStartTemplate { get; set; }

    }

    public class DataCaptureGridData : DataEntryTemplateQueryStatus
    {
        public int? AttendanceId { get; set; }
        public int? ScreeningEntryId { get; set; }
        public int? RandomizationId { get; set; }
        public string VolunteerName { get; set; }
        public bool IsRandomization { get; set; }
        public string SubjectNo { get; set; }
        public string PatientStatusName { get; set; }
        public ScreeningPatientStatus? PatientStatusId { get; set; }
        public ScreeningPatientStatus? ScreeningPatientStatus { get; set; }
        public string RandomizationNumber { get; set; }
        public List<DataEntryVisitTemplateDto> Visit { get; set; }
    }

    public class DataEntryTemplateQueryStatus
    {
        public int? NotStarted { get; set; }
        public int? InProgress { get; set; }
        public int? MyQuery { get; set; }
        public int? Open { get; set; }
        public int? ReOpen { get; set; }
        public int? Answered { get; set; }
        public int? Resolved { get; set; }
        public int? Closed { get; set; }
        public int? SelfCorrection { get; set; }
        public int? Acknowledge { get; set; }
        public List<WorkFlowTemplateCount> TemplateCount { get; set; }
    }

    public class WorkFlowTemplateCount
    {
        public short LevelNo { get; set; }
        public int Count { get; set; }
    }


    public class DataEntryVisitTemplateDto : DataEntryTemplateQueryStatus
    {
        public int ScreeningVisitId { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public string VisitName { get; set; }
        public string VisitStatus { get; set; }
        public int VisitStatusId { get; set; }

        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate.UtcDate();
            set => _scheduleDate = value == DateTime.MinValue ? value : value.UtcDate();
        }
        public bool IsSchedule { get; set; }
        private DateTime? _actualDate { get; set; }
        public DateTime? ActualDate
        {
            get => _actualDate.UtcDate();
            set => _actualDate = value == DateTime.MinValue ? value : value.UtcDate();
        }
    }

    public class DataEntryTemplateCountDisplayDto
    {
        public int ScreeningEntryId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string VisitName { get; set; }
        public string SubjectName { get; set; }
    }

    public class DataEntryTemplateCountDisplayTree
    {
        public int ScreeningEntryId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string VisitName { get; set; }
        public string SubjectName { get; set; }
        public int ScreeningVisitId { get; set; }
        public int Id { get; set; }
        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate?.UtcDateTime();
            set => _scheduleDate = value?.UtcDateTime();
        }
        public int? ParentId { get; set; }
        public ScreeningTemplateStatus Status { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public decimal DesignOrder { get; set; }
        public int Progress { get; set; }
        public short? ReviewLevel { get; set; }
        public string StatusName { get; set; }
        public bool MyReview { get; set; }
        public bool IsLocked { get; set; }
        public QueryStatusDto TemplateQueryStatus { get; set; }
    }

}