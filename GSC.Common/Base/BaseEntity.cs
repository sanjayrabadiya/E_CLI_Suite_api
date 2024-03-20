using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Common;
using GSC.Shared.Extension;
using GSC.Shared.Generic;

namespace GSC.Common.Base
{
    public abstract class BaseEntity
    {
        private DateTime? _createdDate;
        private DateTime? _deletedDate;

        private DateTime? _modifiedDate;

        [Key]
        public int Id { get; set; }

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }

        public int? CreatedBy { get; set; }

        public DateTime? ModifiedDate
        {
            get => _modifiedDate.UtcDateTime();
            set => _modifiedDate = value?.UtcDateTime();
        }

        public int? ModifiedBy { get; set; }

        public DateTime? DeletedDate
        {
            get => _deletedDate?.UtcDateTime();
            set => _deletedDate = value?.UtcDateTime();
        }

        public int? DeletedBy { get; set; }

       
     
        [NotMapped] 
        public AuditAction AuditAction { get; set; }

        [ForeignKey("CreatedBy")]
        public UserAduit CreatedByUser { get; set; }

        [ForeignKey("DeletedBy")]
        public UserAduit DeletedByUser { get; set; }


        [ForeignKey("ModifiedBy")]
        public UserAduit ModifiedByUser { get; set; }
    }
}