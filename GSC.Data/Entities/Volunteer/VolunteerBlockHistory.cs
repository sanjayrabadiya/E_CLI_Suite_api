using System;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.Extension;

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