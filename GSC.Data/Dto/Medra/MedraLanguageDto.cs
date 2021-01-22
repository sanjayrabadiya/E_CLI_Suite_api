using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Medra
{
    public class MedraLanguageDto : BaseDto
    {
        public string LanguageName { get; set; }
    }

    public class MedraLanguageGridDto : BaseAuditDto
    {
        public string LanguageName { get; set; }
    }
}