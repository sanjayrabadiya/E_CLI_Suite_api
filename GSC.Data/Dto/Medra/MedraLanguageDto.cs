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
        public int? CompanyId { get; set; }
    }

    public class MedraLanguageGridDto : BaseAuditDto
    {
        public string LanguageName { get; set; }
    }
}