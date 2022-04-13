using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.CTMS
{
    public class CtmsActionPoint : BaseEntity, ICommonAduit
    {
        public int CtmsMonitoringId { get; set; }
        public CtmsActionPointStatus? Status { get; set; }
        public string QueryDescription { get; set; }
        public string Response { get; set; }
        public int? ResponseBy { get; set; }
        public DateTime? ResponseDate { get; set; }
        public CtmsMonitoring CtmsMonitoring { get; set; }
        public int? CloseBy { get; set; }
        public DateTime? CloseDate { get; set; }
        [ForeignKey("ResponseBy")]
        public User User { get; set; }

        [ForeignKey("CloseBy")]
        public User CloseUser { get; set; }

    }
}
