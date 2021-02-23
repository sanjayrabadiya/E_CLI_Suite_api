using GSC.Data.Entities.AdverseEvent;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.AdverseEvent
{
    public class AdverseEventSettingsLanguageDto : BaseDto
    {
        public int AdverseEventSettingsId { get; set; }
        public int LanguageId { get; set; }
        public string? LowSeverityDisplay { get; set; }
        public string? MediumSeverityDisplay { get; set; }
        public string? HighSeverityDisplay { get; set; }
        public IList<AdverseEventSettingsLanguageSaveDto> adverseEventSettingsLanguages { get; set; }
        public AdverseEventSettings AdverseEventSettings { get; set; }
        public Language Language { get; set; }
    }

    public class AdverseEventSettingsLanguageSaveDto
    {
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public int AdverseEventSettingsLanguageId { get; set; }
        public int SeqNo { get; set; }
    }
}
