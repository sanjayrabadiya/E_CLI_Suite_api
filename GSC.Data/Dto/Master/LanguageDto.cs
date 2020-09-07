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
        public int? CompanyId { get; set; }
    }

    public class LanguageGridDto : BaseAuditDto
    {
        public string LanguageName { get; set; }
    }
}