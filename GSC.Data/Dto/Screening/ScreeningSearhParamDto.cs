using System;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningSearhParamDto
    {
        public int Id { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public DateTime? AttendanceDate { get; set; }

        public ScreeningTemplateStatus? ScreeningStatus { get; set; }
        public string TextSearch { get; set; }
        public bool? IsFitnessFit { get; set; }
        public DataEntryType AttendanceType { get; set; }
        public bool IsFromScreening { get; set; }
        public int ProjectId { get; set; }
        public int? StudyId { get; set; }
        public int VisitId { get; set; }

        public int PeriodNo { get; set; }
    }
}