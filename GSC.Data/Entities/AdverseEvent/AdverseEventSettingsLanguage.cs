using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.AdverseEvent
{
    public class AdverseEventSettingsLanguage : BaseEntity, ICommonAduit
    {
        public int AdverseEventSettingsId { get; set; }
        public int LanguageId { get; set; }
        public string? LowSeverityDisplay { get; set; }
        public string? MediumSeverityDisplay { get; set; }
        public string? HighSeverityDisplay { get; set; }
        public AdverseEventSettings AdverseEventSettings { get; set; }
        public Language Language { get; set; }
    }
}
