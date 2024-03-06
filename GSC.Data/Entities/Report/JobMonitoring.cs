using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;


namespace GSC.Data.Entities.Report
{
    public class JobMonitoring : BaseEntity, ICommonAduit
    {
        public JobNameType JobName { get; set; }
        public int JobDescription { get; set; }
        public JobTypeEnum JobType { get; set; }
        public JobStatusType JobStatus { get; set; }
        public int SubmittedBy { get; set; }
        public DateTime SubmittedTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public DossierPdfStatus JobDetails { get; set; }
        public string FolderPath { get; set; }
        public string FolderName { get; set; }
        public int? CompanyId { get; set; }
    }
}
