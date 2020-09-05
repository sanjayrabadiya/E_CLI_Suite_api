using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningVisit : BaseEntity
    {
        public int ScreeningEntryId { get; set; }
        public int? RepeatedVisit { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public ScreeningStatus Status { get; set; }
        public ScreeningEntry ScreeningEntry { get; set; }
    }
}
