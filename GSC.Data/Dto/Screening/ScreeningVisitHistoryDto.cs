using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningVisitHistoryDto
    {
        public int ScreeningVisitId { get; set; }
        public DateTime? StatusDate { get; set; }
        public ScreeningVisitStatus VisitStatusId { get; set; }
        public string Notes { get; set; }
    }
}
