using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Medra
{
    public class MedraLanguageDto : BaseDto
    {
        public string LanguageName { get; set; }

        public string Culture { get; set; }

        public bool IsDefault { get; set; }
    }
}