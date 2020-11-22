using System;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Shared;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerBlockHistory : BaseEntity
    {
        private DateTime? _FromDate;

        private DateTime? _ToDate;

        public int VolunteerId { get; set; }
        public int BlockCategoryId { get; set; }
        public bool IsPermanently { get; set; }
        public bool IsBlock { get; set; }

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

        public BlockCategory BlockCategory { get; set; }

        public string Note { get; set; }
    }
}