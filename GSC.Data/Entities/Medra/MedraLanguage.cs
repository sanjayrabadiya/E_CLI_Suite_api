using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Medra
{
    public class MedraLanguage : BaseEntity
    {
        public string LanguageName { get; set; }

        public string Culture { get; set; }

        public int? CompanyId { get; set; }
    }
}