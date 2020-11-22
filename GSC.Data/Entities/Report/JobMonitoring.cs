using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Report
{
    public class JobMonitoring : BaseEntity
    {        
        public JobNameType JobName { get; set; }
        public int JobDescription { get; set; }
        public JobTypeEnum JobType { get; set; }
        public JobStatusType JobStatus { get; set; }
        public int SubmittedBy { get; set; }
        public DateTime SubmittedTime{ get; set; }
        public DateTime? CompletedTime { get; set; }    
        public DossierPdfStatus JobDetails { get; set; }
        public string FolderPath{ get; set; }
        public string FolderName { get; set; }
        public int? CompanyId { get; set; }
    }
}
