using GSC.Common.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class StudyPlan: BaseEntity
    {
        public int ProjectId { get; set; }
        public int TaskTemplateId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public Master.Project Project { get; set; }
    }
}
