using System;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerAddress : BaseEntity
    {
        private DateTime? _FromDate;

        private DateTime? _ToDate;
        public int VolunteerId { get; set; }

        public bool IsCurrent { get; set; }

        public bool IsPermanent { get; set; }

        public DateTime? FromDate
        {
            get => _FromDate?.UtcDate();
            set => _FromDate = value?.UtcDate();
        }

        public DateTime? ToDate
        {
            get => _ToDate?.UtcDate();
            set => _ToDate = value?.UtcDate();
        }

        [ForeignKey("LocationId")] public Location.Location Location { get; set; }
    }
}