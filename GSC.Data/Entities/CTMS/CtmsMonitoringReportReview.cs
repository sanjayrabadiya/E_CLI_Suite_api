using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsMonitoringReportReview : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringReportId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsSendBack { get; set; } 
        public DateTime? SendBackDate { get; set; }
        public string Message { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApproveDate { get; set; }
        public CtmsMonitoringReport CtmsMonitoringReport { get; set; }
        public User User { get; set; }
    }
}
