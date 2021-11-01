using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using GSC.Common.Common;
using GSC.Data.Entities.CTMS;

namespace GSC.Data.Entities.Etmf
{
    public class ManageMonitoringReportApprover : BaseEntity, ICommonAduit
    {
        public int ManageMonitoringReportId { get; set; }
        public int UserId { get; set; }
        public bool? IsApproved { get; set; }
        public string Comment { get; set; }
        public int CompanyId { get; set; }
        public ManageMonitoringReport ManageMonitoringReport { get; set; }
        //public List<ProjectArtificateDocumentHistory> ProjectArtificateDocumentHistory { get; set; }
    }
}
