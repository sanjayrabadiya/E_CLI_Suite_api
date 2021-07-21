using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EConsentDocumentHeader
    {
        public int DocumentId { get; set; }
        public int ReviewId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public Boolean IsReviewed { get; set; }
        public int TotalReviewTime { get; set; }

    }
}
