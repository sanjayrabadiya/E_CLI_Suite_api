using System;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerAddress : BaseEntity
    {
        public int VolunteerId { get; set; }

        public bool IsCurrent { get; set; }

        public bool IsPermanent { get; set; }
        [ForeignKey("LocationId")] public Location.Location Location { get; set; }
    }
}