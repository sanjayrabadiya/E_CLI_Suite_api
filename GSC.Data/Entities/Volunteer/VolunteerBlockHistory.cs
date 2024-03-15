using System;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerBlockHistory : BaseEntity, ICommonAduit
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int VolunteerId { get; set; }
        public int BlockCategoryId { get; set; }
        public bool IsPermanently { get; set; }
        public bool IsBlock { get; set; }
       
        public BlockCategory BlockCategory { get; set; }

        public string Note { get; set; }
    }
}