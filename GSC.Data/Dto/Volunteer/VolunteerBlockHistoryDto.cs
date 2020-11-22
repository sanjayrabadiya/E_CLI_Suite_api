using System;
using GSC.Data.Entities.Common;
using GSC.Shared;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerBlockHistoryDto : BaseDto
    {
        private DateTime? _BlockDate;
        private DateTime? _FromDate;

        private DateTime? _ToDate;
        public int VolunteerId { get; set; }
        public int? BlockCategoryId { get; set; }
        public bool IsPermanently { get; set; }
        public bool IsBlock { get; set; }

        public string BlockString { get; set; }

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

        public string UserName { get; set; }
        public string PermanentlyString { get; set; }

        public DateTime? BlockDate
        {
            get => _BlockDate.UtcDate();
            set => _BlockDate = value.UtcDate();
        }

        public string CategoryName { get; set; }
        public string Note { get; set; }
    }
}