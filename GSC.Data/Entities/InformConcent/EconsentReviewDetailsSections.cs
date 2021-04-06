using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentReviewDetailsSections : BaseEntity
    {
        public int EconsentReviewDetailId { get; set; }
        public int SectionNo { get; set; }
        public int TimeInSeconds { get; set; }
        public bool IsAgree { get; set; }

        [ForeignKey("EconsentReviewDetailId")]
        public EconsentReviewDetails EconsentReviewDetails { get; set; }
    }
}
