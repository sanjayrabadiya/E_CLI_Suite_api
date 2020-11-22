using GSC.Common.Base;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Report
{
    public class JobMonitoringDto : BaseEntity
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

        public string JobNamestr { get; set; }
        public string JobDescriptionstr { get; set; }
        public string JobTypestr { get; set; }
        public string JobStatusstr { get; set; }
        public string SubmittedBystr { get; set; }
        public string JobDetailsstr { get; set; }
        public string FullPath { get; set; }

    }
}
