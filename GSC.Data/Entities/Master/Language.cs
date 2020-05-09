using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class Language : BaseEntity
    {
        public string LanguageName { get; set; }

        public string Culture { get; set; }

        public bool IsDefault { get; set; }

        public int? CompanyId { get; set; }
    }
}