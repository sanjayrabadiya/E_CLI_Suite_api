using System;
using System.Collections.Generic;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Attendance
{
    public class AttendanceScreeningGridDto : BaseAuditDto
    {
        public int Id { get; set; }
        public int ScreeningEntryId { get; set; }
        public int AttendanceId { get; set; }
        public int ProjectId { get; set; }
        public int? VolunteerId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime? ScreeningDate { get; set; }
        public bool IsFingerPrint { get; set; }
        public string ScreeningNo { get; set; }
        public string Note { get; set; }
        public string VolunteerNumber { get; set; }
        public string AliasName { get; set; }
        public string VolunteerName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectCode { get; set; }
        public string AttendedBy { get; set; }
        public string Gender { get; set; }
        public string IsFitnessFit { get; set; }
        public int ProjectDesignId { get; set; }
        public string ScreeningStatusName { get; set; }
        public string AuditReasonName { get; set; }
        public int? AttendanceScreeningEntryId { get; set; }
        public int? AttendanceTemplateId { get; set; }
        public int? DiscontinuedTemplateId { get; set; }
        public bool IsScreeningStarted { get; set; }
        public int? ProjectSubjectId { get; set; }
        public int? PeriodNo { get; set; }
        public string SubjectNumber { get; set; }
        public DataEntryType AttendanceType { get; set; }
        public AttendaceStatus? Status { get; set; }
        public string AttendaceStatusName { get; set; }
        public bool IsReplaced { get; set; }
        public string IsStandby { get; set; }
        public bool IsLocked { get; set; }
        public bool IsBarcodeGenerated { get; set; }
        public int? AttendanceBarcodeGenerateId { get; set; }
        public List<TemplateText> TemplateList { get; set; }
        public List<TemplateStatusList> TemplateStatusList { get; set; }

        public int? StudyId { get; set; }

        public string StudyCode { get; set; }

        public string Notes { get; set; }

        public string RandomizationNumber { get; set; }
    }

    public class ScreeningGridDto
    {
        public List<TemplateText> TemplateText { get; set; }
        public List<AttendanceScreeningGridDto> Data { get; set; }
    }

    public class TemplateText
    {
        public int ProjectDesignTemplateId { get; set; }
        public string ProjectDesignTemplateName { get; set; }
        public int DesignOrder { get; set; }
    }

    public class TemplateStatusList
    {
        public int ScreeningEntryId { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignTemplateId { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public int DesignOrder { get; set; }
    }
}