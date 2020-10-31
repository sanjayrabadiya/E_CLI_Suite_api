using System;
using System.Collections.Generic;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Master;
using GSC.Helper;

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

    }

    public class DataCaptureGridData : DataEntryTemplateQueryStatus
    {
        public int? AttendanceId { get; set; }
        public int? ScreeningEntryId { get; set; }
        public int? RandomizationId { get; set; }
        public string VolunteerName { get; set; }
        public bool IsRandomization { get; set; }
        public string SubjectNo { get; set; }
        public string PatientStatus { get; set; }
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
}