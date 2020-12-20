using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.Screening
{
    public class ScreeningTemplateValueQuery 
    {
        [Key]
        public int Id { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public string Value { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public ScreeningTemplateValue ScreeningTemplateValue { get; set; }

        [ForeignKey("ReasonId")] public AuditReason Reason { get; set; }

        public QueryStatus? QueryStatus { get; set; }
        public short QueryLevel { get; set; }
        public string Note { get; set; }

        public string OldValue { get; set; }
        public bool IsSystem { get; set; }
        public string EditCheckRefValue { get; set; }

        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string TimeZone { get; set; }
        private DateTime? _createdDate;
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }


    }
}