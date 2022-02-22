using GSC.Data.Entities.AdverseEvent;
using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.AdverseEvent
{
    public class AdverseEventSettingsDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int ProjectDesignTemplateIdPatient { get; set; }
        public int ProjectDesignTemplateIdInvestigator { get; set; }      
        public int? ProjectDesignVisitIdInvestigator { get; set; }
        
        public List<AdverseEventSettingsDetails> adverseEventSettingsDetails { get; set; }
    }

    public class AdverseEventSettingsVariableValue
    {
        public int Id { get; set; }
        public int AdverseEventSettingsId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public int ProjectDesignVariableValueId { get; set; }
        public string Value { get; set; }
        public int SeqNo { get; set; }
        public int SeveritySeqNo { get; set; }
        public string Severity { get; set; }
    }
    public class AdverseEventSettingsListDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int ProjectDesignTemplateIdPatient { get; set; }
        public int ProjectDesignTemplateIdInvestigator { get; set; }
        public int? ProjectDesignVisitIdInvestigator { get; set; }

        public List<AdverseEventSettingsVariableValue> adverseEventSettingsDetails { get; set; }
    }
}
