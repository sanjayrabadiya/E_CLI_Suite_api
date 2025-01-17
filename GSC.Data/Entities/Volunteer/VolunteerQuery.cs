﻿using GSC.Common.Base;
using GSC.Helper;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerQuery : BaseEntity
    {
        public int VolunteerId { get; set; }
        public int TableId { get; set; }
        public string FieldName { get; set; }
        public string Comment { get; set; }
        public CommentStatus QueryStatus { get; set; }
        public QueryTypes QueryType { get; set; }
        public int ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public int UserRole { get; set; }
        public int? CompanyId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
