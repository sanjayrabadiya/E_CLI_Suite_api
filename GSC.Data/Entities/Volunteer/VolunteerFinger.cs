using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerFinger : BaseEntity
    {
        public int VolunteerId { get; set; }
        public string FingerImage { get; set; }
        public Volunteer Volunteer { get; set; }
    }
}