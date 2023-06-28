using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.CTMS
{
    public class TaskMasterGridDto: BaseAuditDto
    {
        // public int Id { get; set; }
        public string TaskName { get; set; }
        public int? ParentId { get; set; }
        public int TaskTemplateId { get; set;}
        public int TaskOrder { get; set;}
        public int Duration { get; set; }
        public string Predecessor { get; set; }
        public int OffSet { get; set; }
        public string RefrenceType { get; set; }
        public string RefrenceTypes { get; set; }
        public string ActivityType { get; set; }
    }
}
