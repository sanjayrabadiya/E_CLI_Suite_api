using GSC.Data.Entities.Screening;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using GSC.Data.Entities.Common;
using GSC.Data.Entities;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningEntryStudyHistoryDto
    {
        public string VolunteerNo { get; set; }
        public string ProjectNo { get; set; }
        public string Notes { get; set; }
        public string RoleName { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
