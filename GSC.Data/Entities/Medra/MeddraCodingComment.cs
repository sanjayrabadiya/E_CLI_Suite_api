using GSC.Common.Base;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraCodingComment: BaseEntity
    {
        public int MeddraCodingId { get; set; }
        public string Value { get; set; }
        public string OldValue { get; set; }
        public long OldPTCode { get; set; }
        public long NewPTCode { get; set; }

        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public CommentStatus CommentStatus { get; set; }
        public string Note { get; set; }
        public int UserRole { get; set; }
        public int? CompanyId { get; set; }
        public MeddraCoding MeddraCoding { get; set; }
    }
}
