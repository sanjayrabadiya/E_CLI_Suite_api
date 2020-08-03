using System;

namespace GSC.Data.Entities.Common
{
    public class BaseDto
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

    }

    public class BaseAuditDto : BaseDto
    {
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        public string CompanyName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}