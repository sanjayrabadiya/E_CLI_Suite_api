using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Language : BaseEntity, ICommonAduit
    {
        public string LanguageName { get; set; }

        public string Culture { get; set; }

        public bool IsDefault { get; set; }

        public int? CompanyId { get; set; }
        public string shortName { get; set; }
    }
}