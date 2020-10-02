using System;
using System.Collections.Generic;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Helper;

namespace GSC.Data.Dto.Attendance
{
    public class DataCaptureGridDto
    {
        public List<WorkFlowText> WorkFlowText { get; set; }
        public List<DataCaptureGridData> Data { get; set; }

    }

    public class DataCaptureGridData
    {
        public int? AttendanceId { get; set; }
        public int? ScreeningEntryId { get; set; }
        public int? RandomizationId { get; set; }
        public string VolunteerName { get; set; }
        public bool IsRandomization { get; set; }
        public string SubjectNo { get; set; }
        public string PatientStatus { get; set; }
        public string RandomizationNumber { get; set; }
        public DataEntryTemplateQueryStatus Count { get; set; }
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

    
    public class DataEntryVisitTemplateDto
    {
        public int ScreeningVisitId { get; set; }
        public string VisitName { get; set; }
        public DataEntryTemplateQueryStatus Count { get; set; }

        //Added property by vipul on 28092020 
        public ScreeningVisitStatus VisitStatus { get; set; }

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
}