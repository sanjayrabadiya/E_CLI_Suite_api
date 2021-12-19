using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Project.Design
{
    public class StudyVersion : BaseEntity, ICommonAduit
    {
        public double VersionNumber { get; set; }
        public int ProjectId { get; set; }
        public int ProjectDesignId { get; set; }
        public string Note { get; set; }
        public IList<StudyVersionStatus> StudyVersionStatus { get; set; } = null;
        public ProjectDesign ProjectDesign { get; set; }
        public string GoLiveNote { get; set; }
        public string TestNote { get; set; }
        public int? GoLiveBy { get; set; }
        public DateTime? GoLiveOn { get; set; }
        public bool IsMinor { get; set; }
        public bool? IsTestSiteVerified { get; set; }
        public VersionStatus? VersionStatus { get; set; }
        [ForeignKey("GoLiveBy")]
        public UserAduit GoLiveByUser { get; set; }

    }
}
