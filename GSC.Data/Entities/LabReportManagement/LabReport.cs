using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.LabReportManagement
{
    public class LabReport : BaseEntity, ICommonAduit
    {
        public int UserId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
    }
}
