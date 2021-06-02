using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class StudyVersionDto : BaseDto
    {
        public int ProjectDesignId { get; set; }
        public double VersionNumber { get; set; }
        public IList<StudyVerionVisitStatus> StudyVersionVisitStatus { get; set; } = null;
        public bool IsGoLive { get; set; }
        public bool IsRunning { get; set; }
        public int? GoLiveBy { get; set; }
        public DateTime? GoLiveOn { get; set; }
    }

    public class StudyVersionGridDto : BaseAuditDto
    {
        public string StudyName { get; set; }
        public string VersionNumber { get; set; }
        public string VisitStatus { get; set; }
        public bool IsGoLive { get; set; }
        public bool IsRunning { get; set; }
        public string GoLiveBy { get; set; }
        public DateTime? GoLiveOn { get; set; }
    }
}
