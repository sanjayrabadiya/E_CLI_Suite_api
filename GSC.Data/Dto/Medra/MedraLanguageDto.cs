using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Medra
{
    public class MedraLanguageDto : BaseDto
    {
        public string LanguageName { get; set; }

        public string Culture { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }

        public int CreatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public System.DateTime? CreatedDate { get; set; }
        public System.DateTime? ModifiedDate { get; set; }
        public System.DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
    }
}