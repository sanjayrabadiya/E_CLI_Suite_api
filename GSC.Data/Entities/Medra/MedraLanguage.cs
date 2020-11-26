using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Medra
{
    public class MedraLanguage : BaseEntity, ICommonAduit
    {
        public string LanguageName { get; set; }

        public string Culture { get; set; }

        public int? CompanyId { get; set; }
    }
}