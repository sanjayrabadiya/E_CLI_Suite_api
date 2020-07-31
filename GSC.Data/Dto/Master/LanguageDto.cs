using GSC.Data.Entities.Common;
using System;

namespace GSC.Data.Dto.Master
{
    public class LanguageDto : BaseDto
    {
        public string LanguageName { get; set; }
        public string Culture { get; set; }
        public bool IsDefault { get; set; }
        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        //public int CreatedBy { get; set; }
        //public int? DeletedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        //public DateTime? CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        //public string CompanyName { get; set; }
    }
}