using System;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningHistoryDto : BaseDto
    {
        private DateTime? _LastPkSampleDate;

        private DateTime? _NextEligibleDate;

        private DateTime? _NextXrayDueDate;
        private DateTime _screeningDate;

        private DateTime? _XrayDate;
        public int ScreeningEntryId { get; set; }

        public DateTime? XrayDate
        {
            get => _XrayDate?.UtcDate();
            set => _XrayDate = value?.UtcDate();
        }

        public DateTime? NextXrayDueDate
        {
            get => _NextXrayDueDate?.UtcDate();
            set => _NextXrayDueDate = value?.UtcDate();
        }

        public DateTime? LastPkSampleDate
        {
            get => _LastPkSampleDate?.UtcDate();
            set => _LastPkSampleDate = value?.UtcDate();
        }

        public DateTime? NextEligibleDate
        {
            get => _NextEligibleDate?.UtcDate();
            set => _NextEligibleDate = value?.UtcDate();
        }

        public bool? Enrolled { get; set; }
        public bool? IsCompleted { get; set; }
        public string ProjectNumber { get; set; }
        public string Reason { get; set; }

        public DateTime ScreeningDate
        {
            get => _screeningDate.UtcDate();
            set => _screeningDate = value.UtcDate();
        }

        public string VolunteerNumber { get; set; }
        public string VolunteerName { get; set; }
        public string ScreeningStatus { get; set; }
    }
}