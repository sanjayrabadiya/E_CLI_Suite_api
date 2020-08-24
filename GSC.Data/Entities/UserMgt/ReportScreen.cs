using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.UserMgt
{
   public class ReportScreen : BaseEntity
    {
        public int Id { get; set; }
        public string ReportCode { get; set; }
        public string ReportName { get; set; }
        public string ReportGroup { get; set; }
    }
}
