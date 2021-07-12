using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class StudyPlan : BaseEntity, ICommonAduit
    {
        private DateTime _EndDate;


        private DateTime _StartDate;
        public int ProjectId { get; set; }
        public int TaskTemplateId { get; set; }
        public DateTime StartDate
        {
            get => _StartDate.UtcDate();
            set => _StartDate = value.UtcDate();
        }

        public DateTime EndDate
        {
            get => _EndDate.UtcDate();
            set => _EndDate = value.UtcDate();
        }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public Master.Project Project { get; set; }
    }
}
