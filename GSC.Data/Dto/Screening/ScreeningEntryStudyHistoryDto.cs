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
        public int ScreeningEntryId { get; set; }
        public int StudyId { get; set; }
        public ScreeningEntry ScreeningEntry { get; set; }
    }
}
