using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.Project.Design
{
    public class StudyVersionDto : BaseDto
    {
        public int ProjectDesignId { get; set; }
        public int ProjectId { get; set; }
        public double VersionNumber { get; set; }
        public string Note { get; set; }
        public IList<StudyVersionStatus> StudyVersionStatus { get; set; } = null;
        public int? GoLiveBy { get; set; }
        public bool? IsMinor { get; set; }
        public DateTime? GoLiveOn { get; set; }
        public bool? IsTestSiteVerified { get; set; }
        public string TestNote { get; set; }
        public VersionStatus VersionStatus { get; set; }
    }

    public class StudyVersionGridDto : BaseAuditDto
    {
        public string StudyName { get; set; }
        public string VersionNumber { get; set; }
        public string Note { get; set; }
        public string GoLiveNote { get; set; }
        public string PatientStatus { get; set; }
        public bool IsRunning { get; set; }
        public string GoLiveBy { get; set; }
        public DateTime? GoLiveOn { get; set; }
        public string VersionStatus { get; set; }
    }

    public class CheckVersionDto 
    {
        public double? VersionNumber { get; set; }
        public bool AnyLive { get; set; }
    }


    public class StudyGoLiveDto 
    {
        public int ProjectDesignId { get; set; }
        public IList<ScreeningPatientStatus> PatientStatusId { get; set; }
        public string GoLiveNote { get; set; }
        public bool IsOnTrial { get; set; }

    }
}
