using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Volunteer;
using GSC.Helper;
using GSC.Shared.DocumentService;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerFingerDto : BaseDto
    {
        public int VolunteerId { get; set; }
        public string FingerImage { get; set; }
    }

    public class VolunteerFingerAddDto
    {
        public int VolunteerId { get; set; }
        public string Template { get; set; }
    }
}