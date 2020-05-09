using System;
using GSC.Helper;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningSearhParamDto
    {
        private DateTime? _FromDate;

        private DateTime? _ToDate;
        public int Id { get; set; }

        public DateTime? FromDate
        {
            get => _FromDate.UtcDate();
            set => _FromDate = value.UtcDate();
        }

        public DateTime? ToDate
        {
            get => _ToDate.UtcDate();
            set => _ToDate = value.UtcDate();
        }

        public ScreeningStatus? ScreeningStatus { get; set; }
        public string TextSearch { get; set; }
        public bool? IsFitnessFit { get; set; }
        public AttendanceType AttendanceType { get; set; }
        public bool IsFromScreening { get; set; }
        public int ProjectId { get; set; }
        public int PeriodNo { get; set; }
    }
}