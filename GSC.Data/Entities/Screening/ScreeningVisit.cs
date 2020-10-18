using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningVisit : BaseEntity
    {
        public int ScreeningEntryId { get; set; }
        public int? RepeatedVisitNumber { get; set; }
        public int ProjectDesignVisitId { get; set; }
        public ScreeningVisitStatus Status { get; set; }
        public int? Progress { get; set; }
        private DateTime? _visitStartDate { get; set; }
        public DateTime? VisitStartDate
        {
            get => _visitStartDate.UtcDate();
            set => _visitStartDate = value == DateTime.MinValue ? value : value.UtcDate();
        }
        public bool IsSchedule { get; set; }
        private DateTime? _visitStartDate { get; set; }
        public DateTime? VisitStartDate
        {
            get => _visitStartDate.UtcDate();
            set => _visitStartDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public int? ParentId { get; set; }
        public ScreeningEntry ScreeningEntry { get; set; }
        [ForeignKey("ParentId")]
        public ICollection<ScreeningVisit> Children { get; set; }
      
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public ICollection<ScreeningTemplate> ScreeningTemplates { get; set; }
    }
}
