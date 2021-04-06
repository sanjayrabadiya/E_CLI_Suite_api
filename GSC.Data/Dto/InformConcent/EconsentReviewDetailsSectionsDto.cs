using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentReviewDetailsSectionsDto : BaseDto
    {
        public int EconsentReviewDetailId { get; set; }
        public int SectionNo { get; set; }
        public int TimeInSeconds { get; set; }
        public bool IsAgree { get; set; }
        public List<EconsentReviewDetailsSectionsDto> EconsentReviewDetailsSections { get; set; }
    }
}
