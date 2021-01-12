using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.AdverseEvent
{
    public class AEReportingDto : BaseDto
    {
        public int RandomizationId { get; set; }
        public string EventDescription { get; set; }
        public DateTime StartDate { get; set; }
        public string EventEffect { get; set; }
        public bool IsReviewedDone { get; set; }
        public int? ReviewedByUser { get; set; }
        public int? ReviewedByRole { get; set; }
        public DateTime? ReviewedDateTime { get; set; }
    }
}
