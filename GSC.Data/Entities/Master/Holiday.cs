using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared;
using System;

namespace GSC.Data.Entities.Master
{
    public class Holiday : BaseEntity
    {
        public int InvestigatorContactId { get; set; }
        public HolidayType HolidayType  { get; set; }
        public string HolidayName { get; set; }
        //public DateTime? HolidayDate { get; set; }
        public string Description { get; set; }
        public int? CompanyId { get; set; }

        private DateTime? _approveDate;

        public DateTime? HolidayDate
        {
            get => _approveDate?.UtcDateTime();
            set => _approveDate = value?.UtcDateTime();
        }
    }
}
