﻿using GSC.Shared.Extension;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Common.Common
{
    public class AuditTrailCommon
    {
        [Key]
        public int Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string ColumnName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
        public string ReasonOth { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public UserAduit User { get; set; }
        public string UserRole { get; set; }
        public int? CompanyId { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
        private DateTime? _createdDate;
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }

        public int? CreatedBy { get; set; }


    }
}