using System;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Attendance
{
    public class NoneRegister : BaseEntity
    {
        private DateTime? _dateOfRandomization;

        private DateTime? _dateOfScreening;

        public int AttendanceId { get; set; }
        public string Initial { get; set; }
        public string ScreeningNumber { get; set; }

        public DateTime? DateOfScreening
        {
            get => _dateOfScreening?.UtcDate();
            set => _dateOfScreening = value?.UtcDate();
        }

        public string RandomizationNumber { get; set; }

        public DateTime? DateOfRandomization
        {
            get => _dateOfRandomization?.UtcDate();
            set => _dateOfRandomization = value?.UtcDate();
        }

        public Attendance Attendance { get; set; }
        public int? CompanyId { get; set; }
    }
}